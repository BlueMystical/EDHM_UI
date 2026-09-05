const assert = require('node:assert/strict');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');
const { transformSync } = require('esbuild');

function fixture(t) {
  const directory = fs.mkdtempSync(path.join(os.tmpdir(), 'edhm-versions-'));
  t.after(() => fs.rmSync(directory, { recursive: true, force: true }));
  const settingsPath = path.join(directory, 'Settings.json');
  const assets = path.resolve(__dirname, '../src/data');
  const oldSettings = { Version_ODYSS: 'v22.01', Version_HORIZ: 'v1.52b', FirstRun: false, FontSize: '16px' };
  fs.writeFileSync(settingsPath, JSON.stringify(oldSettings));
  const handlers = new Map();
  const fileHelper = {
    getAssetPath: relative => path.join(assets, relative.replace(/^data[\\/]/, '')),
    resolveEnvVariables: () => settingsPath,
    checkFileExists: () => false,
  };
  function load(filename) {
    const module = { exports: {} };
    const { code } = transformSync(fs.readFileSync(filename, 'utf8'), { format: 'cjs' });
    vm.runInNewContext(code, {
      module, exports: module.exports, console,
      require(id) {
        if (id === 'electron') return { ipcMain: { handle: (name, callback) => handlers.set(name, callback) } };
        if (id.startsWith('node:') || ['fs', 'fs/promises'].includes(id)) return require(id);
        if (id.includes('FileHelper')) return fileHelper;
        if (id.includes('ModBundle')) return load(path.resolve(path.dirname(filename), id));
        return {};
      },
    }, { filename });
    return module.exports;
  }
  const helper = load(path.resolve(__dirname, '../src/Helpers/SettingsHelper.js')).default;
  return {
    helper, handlers, oldSettings, settingsPath, directory,
    getModBundle: load(path.resolve(__dirname, '../src/Helpers/ModBundle.js')).getModBundle,
  };
}

test('Existing settings use bundled versions even when FirstRun is false', async t => {
  const { helper, handlers } = fixture(t);
  const settings = await helper.initializeSettings();
  assert.equal(settings.Version_ODYSS, 'v22.02');
  assert.equal(settings.Version_HORIZ, 'v1.52.b');
  assert.equal(settings.FirstRun, false, 'Reading bundle metadata must not trigger an install');
  assert.equal(helper.readSetting('Version_ODYSS'), 'v22.02');
  assert.equal(handlers.get('get-settings')().Version_ODYSS, 'v22.02');
  assert.equal(helper.loadSettings().Version_ODYSS, 'v22.02');
});

test('Saving an older renderer copy cannot restore stale version metadata', async t => {
  const { helper, oldSettings, settingsPath } = fixture(t);
  await helper.initializeSettings();
  const saved = await helper.saveSettings(JSON.stringify({ ...oldSettings, FontSize: '18px' }));
  assert.equal(saved.Version_ODYSS, 'v22.02');
  assert.equal(helper.readSetting('Version_ODYSS'), 'v22.02');
  assert.equal(JSON.parse(fs.readFileSync(settingsPath)).Version_ODYSS, 'v22.02');
  assert.equal(saved.FontSize, '18px');
});

test('Single-setting writes keep shared settings and bundled versions consistent', async t => {
  const { helper, handlers, settingsPath } = fixture(t);
  await helper.initializeSettings();
  helper.writeSetting('FontSize', '20px');
  assert.equal(helper.readSetting('FontSize'), '20px');
  assert.equal(handlers.get('get-settings')().Version_ODYSS, 'v22.02');
  assert.equal(JSON.parse(fs.readFileSync(settingsPath)).Version_ODYSS, 'v22.02');
});

test('Bundle selection preserves suffixes and rejects missing or ambiguous releases', t => {
  const { directory, getModBundle } = fixture(t);
  assert.throws(() => getModBundle(directory, 'HORIZ'), /found 0/);
  fs.writeFileSync(path.join(directory, 'HORIZ_EDHM-Themes.zip'), '');
  fs.writeFileSync(path.join(directory, 'HORIZ_EDHM-v1.52.b.zip'), '');
  assert.equal(getModBundle(directory, 'HORIZ').version, 'v1.52.b');
  fs.writeFileSync(path.join(directory, 'HORIZ_EDHM-v1.53.zip'), '');
  assert.throws(() => getModBundle(directory, 'HORIZ'), /found 2/);
});
