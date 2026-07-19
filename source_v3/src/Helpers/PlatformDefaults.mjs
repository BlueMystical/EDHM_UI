export const WINDOWS_PLAYER_JOURNAL_DEFAULT =
  '%USERPROFILE%\\Saved Games\\Frontier Developments\\Elite Dangerous';

export const LINUX_PLAYER_JOURNAL_DEFAULT =
  '~/.local/share/Steam/steamapps/compatdata/359320/pfx/drive_c/users/steamuser/Saved Games/Frontier Developments/Elite Dangerous';

const normalizeDefaultPath = (value) => String(value || '')
  .trim()
  .replace(/\\/g, '/')
  .replace(/\/+$/g, '')
  .toLowerCase();

export const getDefaultPlayerJournal = (platform = process.platform) =>
  platform === 'linux'
    ? LINUX_PLAYER_JOURNAL_DEFAULT
    : WINDOWS_PLAYER_JOURNAL_DEFAULT;

export const applyPlatformSettingsDefaults = (settings, platform = process.platform) => {
  if (!settings || typeof settings !== 'object') return false;

  const configured = normalizeDefaultPath(settings.PlayerJournal);
  const windowsDefault = normalizeDefaultPath(WINDOWS_PLAYER_JOURNAL_DEFAULT);
  const linuxDefault = normalizeDefaultPath(LINUX_PLAYER_JOURNAL_DEFAULT);
  const desiredDefault = getDefaultPlayerJournal(platform);

  const shouldUsePlatformDefault = !configured ||
    (platform === 'linux' && configured === windowsDefault) ||
    (platform === 'win32' && configured === linuxDefault);

  if (!shouldUsePlatformDefault || settings.PlayerJournal === desiredDefault) return false;
  settings.PlayerJournal = desiredDefault;
  return true;
};
