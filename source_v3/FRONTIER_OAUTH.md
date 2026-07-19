# Frontier OAuth and Shipyard Fleet Sync

## Registered application

- Client ID: `ec3e9295-0633-462a-955c-8f41e3b8d7b4`
- Redirect URI: `edhm://frontier-auth`
- Scopes: `auth capi`
- Shared key: not used

EDHM-UI is a public desktop client. It uses OAuth 2.0 Authorization Code with PKCE, so a client secret is neither required nor safe to distribute with the application.

## User flow

1. The user opens the Shipyard and selects **Connect Frontier**.
2. EDHM-UI creates a random PKCE verifier, challenge, and OAuth state value.
3. EDHM-UI registers itself as the operating-system handler for the `edhm://` protocol.
4. The system browser opens Frontier's authorization page.
5. Frontier redirects the browser to `edhm://frontier-auth` after approval. The operating system sends that URL to the running EDHM-UI instance.
6. EDHM-UI validates the returned state and exchanges the authorization code and PKCE verifier for access and refresh tokens.
7. EDHM-UI requests `/profile`, normalizes the owned fleet, and merges it into `Shipyard_v3.json` while retaining themes, custom images, and unmatched Journal/V2 records.

The callback wait times out after five minutes. If the browser is closed before approval, the user can select **Cancel Connection** to clear the pending request and try again immediately. No localhost web server, listening port, TLS certificate, or certificate exception is required. Depending on browser settings, the user may be asked to confirm that the browser is allowed to open EDHM-UI.

## Token security

- Tokens remain in the Electron main process and are never returned through preload IPC.
- Authorization callback URLs are removed from renderer arguments and are not logged by EDHM-UI.
- Tokens are encrypted using Electron `safeStorage` and stored as `frontier-auth-token.dat` under Electron's `app.getPath('userData')` directory.
- The encrypted file is written with owner-only permissions where the operating system supports them.
- Linux `basic_text` storage is rejected. Users must have an available, unlocked system keyring.
- There is no plaintext fallback.
- Disconnecting removes the token file but intentionally retains the user's Shipyard configuration.

User-facing Linux setup and troubleshooting are documented in [Linux Users](../Linux%20Users.md#frontier-account-authorization).

## Frontier requests

- Authorization: `https://auth.frontierstore.net/auth`
- Token exchange and refresh: `https://auth.frontierstore.net/token`
- Live fleet: `https://companion.orerve.net/profile?language=en`
- Legacy fleet: `https://legacy-companion.orerve.net/profile?language=en`
- User-Agent: `EDCD-EDHMUI-<app version>`

The active game instance selects the Live or Legacy CAPI host. Fleet requests occur only after login or an explicit **Refresh Fleet** action. Successful refreshes are limited to one per minute, following Frontier's CAPI guidance.

Access tokens are refreshed shortly before expiration. Refresh-token failure caused by expiration or revoked authorization removes the unusable local authorization and asks the user to connect again. Frontier currently requires users to reauthorize after the refresh-token authorization window expires.

## Shipyard merge behavior

Frontier's `ships` property may be an array or an object keyed by ship ID. Both forms are supported. Each normalized ship receives a stable `frontier_id` and `record_id`, allowing multiple ships of the same model to be customized independently.

On refresh:

- Existing theme assignments and custom images are preserved.
- Frontier-sourced ships no longer present in the fleet are removed.
- Journal and V2-imported records not represented by Frontier are retained.
- Unknown future ship models remain visible using the default ship image.
- The raw CAPI profile, loadout modules, access token, and refresh token are not written to `Shipyard_v3.json`.

### Ship identity and journal matching

Frontier's numeric ship `id` and the Player Journal `Loadout.ShipID` identify the same owned ship and are the primary match key. This keeps multiple ships of the same model separate even when they share a custom name or displayed identifier. Renaming a ship or changing its identifier updates the saved display data without losing its assigned theme or custom image.

Older Shipyard records that do not contain an owned-ship ID are migrated using a normalized model, custom name (`name` or legacy `custom_name`), and displayed identifier match. Once matched, they receive the numeric ID so later journal events cannot create a duplicate. A reused numeric ID associated with a different ship model is treated as a new ship and does not inherit the sold ship's customization.

## Development checks

From `source_v3`:

```text
npm test
npm run package
```

The unit tests cover custom-protocol callback recognition, array and ID-keyed Frontier responses, ship-model normalization, duplicate-current-ship prevention, customization preservation, journal rename handling, legacy-record migration, same-model ship separation, sold-ship removal, and retention of local-only Shipyard records.

## References

- [EDCD Frontier OAuth notes](https://github.com/EDCD/FDevIDs/blob/master/Frontier%20API/FrontierDevelopments-oAuth2-notes.md)
- [EDCD Frontier CAPI endpoints](https://github.com/EDCD/FDevIDs/blob/master/Frontier%20API/FrontierDevelopments-CAPI-endpoints.md)
