const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');
const { parse } = require('@vue/compiler-sfc');
const { transformSync } = require('esbuild');

// Exercise the component methods themselves, isolating Electron IPC and drawing
// so these regressions can run without touching a player's settings or game.
function loadComponent(name, globals = {}) {
  const filename = path.join(__dirname, '../src/MainWindow', name);
  const { descriptor } = parse(readFileSync(filename, 'utf8'));
  const { code } = transformSync(descriptor.script.content, { format: 'cjs' });
  const module = { exports: {} };
  vm.runInNewContext(code, {
    module, exports: module.exports, console,
    document: { querySelectorAll: () => [] },
    require(id) {
      if (id === 'vue') return require('vue');
      if (id.includes('EventBus')) return globals.EventBus;
      if (id.endsWith('.json')) return {};
      return {};
    },
    ...globals,
  }, { filename });
  return module.exports.default;
}

function instance(component, extra = {}) {
  const result = { ...component.data(), ...extra };
  for (const [name, method] of Object.entries(component.methods)) {
    result[name] = method.bind(result);
  }
  return result;
}

test('Add New Theme opens the internal editor without an OS dialog or saving', async () => {
  const events = [];
  let nativeDialogs = 0;
  let writes = 0;
  const globals = {
    EventBus: { emit: (...args) => events.push(args) },
    window: { api: {
      ShowMessageBox: async () => { nativeDialogs++; return { response: 0 }; },
      CreateNewTheme: async () => { writes++; },
    } },
  };
  const nav = instance(loadComponent('NavBars.vue', globals));
  await nav.addNewTheme_Click();
  assert.equal(nativeDialogs, 0, 'Add New Theme must not open an OS message box');
  assert.equal(events.length, 1);
  assert.equal(events[0][0], 'OnCreateTheme');
  const app = instance(loadComponent('App.vue', globals));
  await app.OnCreateTheme(events[0][1]);
  assert.equal(app.showThemeImageEditorModal, true);
  assert.equal(app.editingTheme, null);
  app.closeThemeImageEditor();
  assert.equal(app.showThemeImageEditorModal, false);
  assert.equal(writes, 0, 'Opening and cancelling must not save a theme');
});

function hudFixture() {
  const fills = [];
  const context = {
    clearRect() {}, strokeRect() {}, fillText() {},
    fillRect: (...rect) => fills.push(rect),
  };
  const container = { clientWidth: 1000, clientHeight: 500 };
  const events = [];
  const component = loadComponent('HudImage.vue', {
    EventBus: { emit: (...args) => events.push(args) },
  });
  const hud = instance(component, {
    hudData: { Colors: {} },
    areas: [{ id: 'Radar', title: 'Radar & Common Group', x: 100, y: 50, width: 400, height: 200 }],
    $refs: {
      image: { complete: true, naturalWidth: 1000, naturalHeight: 500 },
      container,
      canvas: { getContext: () => context, getBoundingClientRect: () => ({ left: 0, top: 0 }) },
    },
  });
  hud.setupCanvas();
  return { hud, container, fills, events };
}

test('Selected highlight follows repeated resizes without selecting another area', () => {
  const { hud, container, fills, events } = hudFixture();
  hud.OnArea_MouseMove({ clientX: 150, clientY: 100 });
  hud.OnArea_Click(null);
  const selectionEvents = events.length;
  container.clientWidth = 500;
  hud.setupCanvas();
  assert.deepEqual(fills.at(-1), [50, 150, 200, 95]);
  assert.equal(hud.clickedArea.id, 'Radar');
  container.clientWidth = 1200;
  container.clientHeight = 400;
  hud.setupCanvas();
  assert.deepEqual(fills.at(-1), [280, 40, 320, 155]);
  hud.OnArea_MouseLeave();
  assert.deepEqual(fills.at(-1), [280, 40, 320, 155]);
  assert.equal(events.length, selectionEvents, 'Resizing must not change the selected menu');
});

test('Hover followed by resize and click uses the new coordinates', () => {
  const { hud, container, fills } = hudFixture();
  hud.OnArea_MouseMove({ clientX: 150, clientY: 100 });
  container.clientWidth = 500;
  hud.setupCanvas();
  hud.OnArea_Click(null);
  assert.deepEqual(fills.at(-1), [50, 150, 200, 95]);
});

test('Menu-driven area selection uses scaled coordinates', () => {
  const { hud, container, fills } = hudFixture();
  container.clientWidth = 500;
  hud.setupCanvas();
  hud.DoLoadArea('Radar');
  assert.deepEqual(fills.at(-1), [50, 150, 200, 95]);
});

test('Installing the bundled Odyssey update refreshes the footer to its version', async () => {
  const packageData = require('../package.json');
  const settings = require('../src/data/Settings.json');
  const { readdirSync } = require('node:fs');
  const bundles = readdirSync(path.join(__dirname, '../src/data/ODYSS'))
    .filter(name => /^ODYSS_EDHM-v\d+\.\d+\.zip$/.test(name));
  assert.equal(bundles.length, 1);
  const bundledVersion = bundles[0].match(/v\d+\.\d+/)[0];
  assert.equal(packageData.version, '3.0.71');
  assert.equal(settings.Version_ODYSS, bundledVersion);
  const events = [];
  const globals = {
    EventBus: { emit: (...args) => events.push(args) },
    window: { api: {
      getInstanceByName: async () => ({ key: 'ED_Odissey', instance: 'Test Odyssey' }),
      installEDHMmod: async () => ({ game: 'ODYSS', version: bundledVersion }),
      RestoreCurrentSettings: async () => true,
      saveSettings: async value => JSON.parse(value),
    } },
  };
  const app = instance(loadComponent('App.vue', globals), {
    settings: { ActiveInstance: 'Test Odyssey', Version_ODYSS: 'v22.01' },
  });
  app.StartShipyard = () => {};
  await app.OnGameInstance_Changed({ GameInstanceName: 'Test Odyssey', InstallMod: true });
  assert.equal(events.some(([name]) => name === 'ShowError'), false);
  const nav = instance(loadComponent('NavBars.vue', globals));
  nav.RefreshEDHMStatus = () => {};
  nav.OnModUpdated(events.find(([name]) => name === 'modUpdated')[1]);
  assert.equal(nav.modVersion, 'v22.02');
});
