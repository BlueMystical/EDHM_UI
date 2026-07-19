export const FRONTIER_AUTH_SCHEME = 'edhm';
export const FRONTIER_AUTH_HOST = 'frontier-auth';
export const FRONTIER_REDIRECT_URI = `${FRONTIER_AUTH_SCHEME}://${FRONTIER_AUTH_HOST}`;

const normalizeProtocolArgument = (value) => {
  const argument = String(value || '').trim();
  if (argument.startsWith('"') && argument.endsWith('"')) return argument.slice(1, -1);
  return argument;
};

export const parseFrontierAuthCallback = (value) => {
  const argument = normalizeProtocolArgument(value);
  if (!argument) return null;

  try {
    const callbackUrl = new URL(argument);
    if (
      callbackUrl.protocol !== `${FRONTIER_AUTH_SCHEME}:` ||
      callbackUrl.hostname !== FRONTIER_AUTH_HOST ||
      callbackUrl.port ||
      callbackUrl.username ||
      callbackUrl.password ||
      callbackUrl.hash ||
      (callbackUrl.pathname && callbackUrl.pathname !== '/')
    ) {
      return null;
    }

    return {
      url: callbackUrl.toString(),
      code: callbackUrl.searchParams.get('code') || '',
      state: callbackUrl.searchParams.get('state') || '',
      error: callbackUrl.searchParams.get('error') || '',
      errorDescription: callbackUrl.searchParams.get('error_description') || '',
    };
  } catch {
    return null;
  }
};

export const findFrontierAuthCallback = (args = []) => {
  for (const argument of args) {
    if (parseFrontierAuthCallback(argument)) return normalizeProtocolArgument(argument);
  }
  return null;
};

export const filterFrontierAuthArguments = (args = []) =>
  args.filter((argument) => !parseFrontierAuthCallback(argument));
