import { app, safeStorage, shell } from 'electron';
import { createHash, randomBytes, timingSafeEqual } from 'node:crypto';
import path from 'node:path';
import { chmod, mkdir, readFile, rename, unlink, writeFile } from 'node:fs/promises';
import {
  FRONTIER_REDIRECT_URI,
  parseFrontierAuthCallback,
} from './FrontierOAuth.mjs';

const FRONTIER_CLIENT_ID = 'ec3e9295-0633-462a-955c-8f41e3b8d7b4';
const FRONTIER_AUTHORIZE_URL = 'https://auth.frontierstore.net/auth';
const FRONTIER_TOKEN_URL = 'https://auth.frontierstore.net/token';
const FRONTIER_PROFILE_URLS = {
  live: 'https://companion.orerve.net/profile?language=en',
  legacy: 'https://legacy-companion.orerve.net/profile?language=en',
};
const AUTH_TIMEOUT_MS = 5 * 60 * 1000;
const REQUEST_TIMEOUT_MS = 30 * 1000;
const MIN_PROFILE_INTERVAL_MS = 60 * 1000;

const base64Url = (buffer) => buffer
  .toString('base64')
  .replace(/\+/g, '-')
  .replace(/\//g, '_')
  .replace(/=+$/g, '');

const constantTimeEqual = (left, right) => {
  const leftBuffer = Buffer.from(String(left || ''), 'utf8');
  const rightBuffer = Buffer.from(String(right || ''), 'utf8');
  return leftBuffer.length === rightBuffer.length && timingSafeEqual(leftBuffer, rightBuffer);
};

class FrontierApi {
  constructor() {
    this.tokenFilePath = path.join(app.getPath('userData'), 'frontier-auth-token.dat');
    this.authorizationPromise = null;
    this.authorizationCallback = null;
  }

  get userAgent() {
    return `EDCD-EDHMUI-${app.getVersion()}`;
  }

  assertPrivateStorageAvailable() {
    if (!safeStorage.isEncryptionAvailable()) {
      throw new Error(
        'Secure token storage is unavailable. Configure your operating system credential store and try again.'
      );
    }
    if (process.platform === 'linux' && safeStorage.getSelectedStorageBackend() === 'basic_text') {
      throw new Error(
        'A secure Linux credential store is unavailable. Install and unlock a supported keyring before connecting Frontier.'
      );
    }
  }

  async loadTokens() {
    try {
      const encrypted = await readFile(this.tokenFilePath, 'utf8');
      this.assertPrivateStorageAvailable();
      const decrypted = safeStorage.decryptString(Buffer.from(encrypted.trim(), 'base64'));
      const tokens = JSON.parse(decrypted);
      if (!tokens?.accessToken && !tokens?.refreshToken) return null;
      return tokens;
    } catch (error) {
      if (error?.code === 'ENOENT') return null;
      throw new Error(`Unable to read the securely stored Frontier authorization: ${error.message}`);
    }
  }

  async saveTokens(tokens) {
    this.assertPrivateStorageAvailable();
    const directory = path.dirname(this.tokenFilePath);
    const temporaryPath = `${this.tokenFilePath}.tmp`;
    await mkdir(directory, { recursive: true });
    const encrypted = safeStorage.encryptString(JSON.stringify(tokens)).toString('base64');
    await writeFile(temporaryPath, encrypted, { encoding: 'utf8', mode: 0o600 });
    await rename(temporaryPath, this.tokenFilePath);
    try {
      await chmod(this.tokenFilePath, 0o600);
    } catch (error) {
      if (process.platform !== 'win32') throw error;
    }
  }

  async clearTokens() {
    try {
      await unlink(this.tokenFilePath);
    } catch (error) {
      if (error?.code !== 'ENOENT') throw error;
    }
  }

  async getStatus() {
    const tokens = await this.loadTokens();
    return {
      authorized: Boolean(tokens?.accessToken || tokens?.refreshToken),
      commanderName: tokens?.commanderName || '',
      expiresAt: tokens?.expiresAt || null,
      lastSyncAt: tokens?.lastSyncAt || null,
      shipCount: Number(tokens?.shipCount || 0),
    };
  }

  async requestJson(url, options, purpose) {
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS);
    try {
      const response = await fetch(url, { ...options, signal: controller.signal });
      const responseText = await response.text();
      let data = null;
      if (responseText) {
        try {
          data = JSON.parse(responseText);
        } catch {
          data = responseText;
        }
      }
      if (!response.ok) {
        const detail = typeof data === 'string'
          ? data.slice(0, 300)
          : data?.message || data?.error_description || data?.error || response.statusText;
        const error = new Error(`${purpose} failed (${response.status}): ${detail}`);
        error.status = response.status;
        throw error;
      }
      return data;
    } catch (error) {
      if (error?.name === 'AbortError') {
        throw new Error(`${purpose} timed out. Please check your connection and try again.`);
      }
      throw error;
    } finally {
      clearTimeout(timeout);
    }
  }

  normalizeTokenResponse(response, previousTokens = {}) {
    if (!response?.access_token) {
      throw new Error('Frontier did not return an access token.');
    }
    const now = Date.now();
    return {
      accessToken: response.access_token,
      refreshToken: response.refresh_token || previousTokens.refreshToken || '',
      tokenType: response.token_type || previousTokens.tokenType || 'Bearer',
      expiresAt: now + (Number(response.expires_in || 0) * 1000),
      authorizedAt: previousTokens.authorizedAt || now,
      commanderName: previousTokens.commanderName || '',
      lastSyncAt: previousTokens.lastSyncAt || null,
      shipCount: Number(previousTokens.shipCount || 0),
    };
  }

  async exchangeAuthorizationCode(code, verifier, redirectUri) {
    const body = new URLSearchParams({
      redirect_uri: redirectUri,
      code,
      grant_type: 'authorization_code',
      code_verifier: verifier,
      client_id: FRONTIER_CLIENT_ID,
    });
    const response = await this.requestJson(FRONTIER_TOKEN_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        'User-Agent': this.userAgent,
      },
      body: body.toString(),
    }, 'Frontier token exchange');
    const tokens = this.normalizeTokenResponse(response);
    await this.saveTokens(tokens);
    return tokens;
  }

  async refreshAccessToken(previousTokens) {
    if (!previousTokens?.refreshToken) {
      throw new Error('Frontier authorization has expired. Connect your Frontier account again.');
    }
    const body = new URLSearchParams({
      grant_type: 'refresh_token',
      client_id: FRONTIER_CLIENT_ID,
      refresh_token: previousTokens.refreshToken,
    });
    let response;
    try {
      response = await this.requestJson(FRONTIER_TOKEN_URL, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
          'User-Agent': this.userAgent,
        },
        body: body.toString(),
      }, 'Frontier token refresh');
    } catch (error) {
      if (error?.status === 400 || error?.status === 401) {
        await this.clearTokens();
        throw new Error('Frontier authorization expired or was revoked. Connect your Frontier account again.');
      }
      throw error;
    }
    const tokens = this.normalizeTokenResponse(response, previousTokens);
    await this.saveTokens(tokens);
    return tokens;
  }

  async ensureAccessToken(forceRefresh = false) {
    const tokens = await this.loadTokens();
    if (!tokens) {
      throw new Error('Connect your Frontier account before refreshing the fleet.');
    }
    if (!forceRefresh && tokens.accessToken && Number(tokens.expiresAt) > Date.now() + 60_000) {
      return tokens;
    }
    return this.refreshAccessToken(tokens);
  }

  createAuthorizationCallback(expectedState) {
    return new Promise((resolve, reject) => {
      const timeoutId = setTimeout(() => {
        if (this.authorizationCallback?.expectedState === expectedState) {
          this.authorizationCallback = null;
        }
        reject(new Error('Frontier authorization timed out. Please try again.'));
      }, AUTH_TIMEOUT_MS);

      this.authorizationCallback = {
        expectedState,
        timeoutId,
        resolve,
        reject,
      };
    });
  }

  clearAuthorizationCallback(expectedState) {
    if (!this.authorizationCallback || this.authorizationCallback.expectedState !== expectedState) return;
    clearTimeout(this.authorizationCallback.timeoutId);
    this.authorizationCallback = null;
  }

  cancelAuthorization() {
    const pending = this.authorizationCallback;
    if (!pending) return false;

    clearTimeout(pending.timeoutId);
    this.authorizationCallback = null;
    pending.reject(new Error('Frontier authorization was cancelled.'));
    return true;
  }

  handleAuthorizationCallback(callbackValue) {
    const callback = parseFrontierAuthCallback(callbackValue);
    if (!callback) return false;

    const pending = this.authorizationCallback;
    if (!pending) {
      console.warn('Received a Frontier authorization callback with no authorization in progress.');
      return false;
    }

    clearTimeout(pending.timeoutId);
    this.authorizationCallback = null;

    if (!constantTimeEqual(callback.state, pending.expectedState)) {
      pending.reject(new Error('Frontier authorization returned an invalid security state. Please try again.'));
      return true;
    }
    if (callback.error) {
      pending.reject(new Error(
        `Frontier authorization was not granted: ${callback.errorDescription || callback.error}`
      ));
      return true;
    }
    if (!callback.code) {
      pending.reject(new Error('Frontier did not return an authorization code.'));
      return true;
    }

    pending.resolve(callback.code);
    return true;
  }

  async authorize() {
    if (this.authorizationPromise) return this.authorizationPromise;
    this.authorizationPromise = this.performAuthorization();
    try {
      return await this.authorizationPromise;
    } finally {
      this.authorizationPromise = null;
    }
  }

  async performAuthorization() {
    this.assertPrivateStorageAvailable();
    const verifier = base64Url(randomBytes(32));
    const challenge = base64Url(createHash('sha256').update(verifier, 'ascii').digest());
    const state = base64Url(randomBytes(32));
    const callback = this.createAuthorizationCallback(state);
    // Cancellation can reject this while openExternal is still resolving.
    // Awaiting the original promise below continues to propagate that error.
    void callback.catch(() => {});

    const authorizationUrl = new URL(FRONTIER_AUTHORIZE_URL);
    authorizationUrl.search = new URLSearchParams({
      audience: 'frontier',
      scope: 'auth capi',
      response_type: 'code',
      client_id: FRONTIER_CLIENT_ID,
      code_challenge: challenge,
      code_challenge_method: 'S256',
      state,
      redirect_uri: FRONTIER_REDIRECT_URI,
    }).toString();

    try {
      await shell.openExternal(authorizationUrl.toString());
      const code = await callback;
      await this.exchangeAuthorizationCode(code, verifier, FRONTIER_REDIRECT_URI);
      return this.getStatus();
    } finally {
      this.clearAuthorizationCallback(state);
    }
  }

  async fetchProfile(galaxy = 'live') {
    const currentTokens = await this.loadTokens();
    if (currentTokens?.lastSyncAt) {
      const waitMs = MIN_PROFILE_INTERVAL_MS - (Date.now() - Number(currentTokens.lastSyncAt));
      if (waitMs > 0) {
        throw new Error(`Please wait ${Math.ceil(waitMs / 1000)} seconds before refreshing the Frontier fleet again.`);
      }
    }

    const profileUrl = FRONTIER_PROFILE_URLS[galaxy] || FRONTIER_PROFILE_URLS.live;
    let tokens = await this.ensureAccessToken();
    const requestProfile = () => this.requestJson(profileUrl, {
      method: 'GET',
      headers: {
        Authorization: `${tokens.tokenType || 'Bearer'} ${tokens.accessToken}`,
        'User-Agent': this.userAgent,
        Accept: 'application/json',
      },
    }, 'Frontier fleet request');

    try {
      return await requestProfile();
    } catch (error) {
      if (error?.status !== 401 && error?.status !== 422) throw error;
      tokens = await this.ensureAccessToken(true);
      return requestProfile();
    }
  }

  async recordFleetSync(commanderName, shipCount) {
    const tokens = await this.loadTokens();
    if (!tokens) return;
    await this.saveTokens({
      ...tokens,
      commanderName: commanderName || tokens.commanderName || '',
      lastSyncAt: Date.now(),
      shipCount: Number(shipCount || 0),
    });
  }
}

export default FrontierApi;
