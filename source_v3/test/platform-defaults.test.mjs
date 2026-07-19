import test from 'node:test';
import assert from 'node:assert/strict';
import {
  applyPlatformSettingsDefaults,
  LINUX_PLAYER_JOURNAL_DEFAULT,
  WINDOWS_PLAYER_JOURNAL_DEFAULT,
} from '../src/Helpers/PlatformDefaults.mjs';

test('migrates only untouched journal defaults for the active platform', () => {
  const linuxSettings = { PlayerJournal: WINDOWS_PLAYER_JOURNAL_DEFAULT };
  assert.equal(applyPlatformSettingsDefaults(linuxSettings, 'linux'), true);
  assert.equal(linuxSettings.PlayerJournal, LINUX_PLAYER_JOURNAL_DEFAULT);

  const customSettings = { PlayerJournal: '/mnt/games/custom-journals' };
  assert.equal(applyPlatformSettingsDefaults(customSettings, 'linux'), false);
  assert.equal(customSettings.PlayerJournal, '/mnt/games/custom-journals');

  const windowsSettings = { PlayerJournal: LINUX_PLAYER_JOURNAL_DEFAULT };
  assert.equal(applyPlatformSettingsDefaults(windowsSettings, 'win32'), true);
  assert.equal(windowsSettings.PlayerJournal, WINDOWS_PLAYER_JOURNAL_DEFAULT);
});
