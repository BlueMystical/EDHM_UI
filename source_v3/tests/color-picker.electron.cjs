// Run the real Vue templates and vendored picker in a hidden Electron renderer.
// Only startup/persistence are stubbed; no player settings or game files are used.
const { app, BrowserWindow } = require('electron');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const { parse, compileTemplate } = require('@vue/compiler-sfc');
const { transformSync } = require('esbuild');
const root = path.resolve(__dirname, '..');
const cjs = source => transformSync(source, { format: 'cjs' }).code;
const definitions = {};
for (const name of ['PropertiesTabEx.vue', 'Components/ColorDisplay.vue']) {
  const filename = path.join(root, 'src/MainWindow', name);
  const { descriptor } = parse(fs.readFileSync(filename, 'utf8'));
  definitions[name] = {
    script: cjs(descriptor.script.content),
    template: cjs(compileTemplate({ source: descriptor.template.content, filename, id: name }).code),
  };
}
const util = cjs(fs.readFileSync(path.join(root, 'src/Helpers/Utils.js'), 'utf8'));
const picker = cjs(fs.readFileSync(path.join(root, 'src/MainWindow/Components/vanilla-picker.csp.mjs'), 'utf8'));
app.setPath('userData', fs.mkdtempSync(path.join(os.tmpdir(), 'edhm-picker-test-')));
app.whenReady().then(async () => {
  let win;
  try {
    win = new BrowserWindow({ show: false, webPreferences: {
      nodeIntegration: true, contextIsolation: false, offscreen: true,
    } });
    await win.loadURL('data:text/html,<html><body><div id="test"></div></body></html>');
    const result = await win.webContents.executeJavaScript(`(async () => {
      const Vue = require(${JSON.stringify(path.join(root, 'node_modules/vue/dist/vue.cjs.js'))});
      const assert = require('node:assert/strict');
      const defs = ${JSON.stringify(definitions)};
      function evaluate(code, imports) {
        const m = {exports:{}};
        new Function('require','module','exports',code)(imports,m,m.exports);
        return m.exports;
      }
      const Util = evaluate(${JSON.stringify(util)}).default;
      const Picker = evaluate(${JSON.stringify(picker)}).default;
      const bus = {on(){},off(){},emit(){}};
      let changeEvents = 0;
      function load(name) {
        const imports = id => id === 'vue' ? Vue : id.includes('Utils') ? Util
          : id.includes('EventBus') ? bus : id.endsWith('.mjs') ? Picker
          : id.endsWith('.vue') ? load('Components/ColorDisplay.vue') : {};
        const component = evaluate(defs[name].script,imports).default;
        component.render = evaluate(defs[name].template,imports).render;
        return component;
      }
      const component = load('PropertiesTabEx.vue');
      component.mounted = () => {};
      let saves = 0;
      component.methods.saveChanges = () => { saves++; };
      const onChange = component.methods.OnColorValueChange;
      component.methods.OnColorValueChange = function(...args) {
        changeEvents++; return onChange.apply(this,args);
      };
      const vueApp = Vue.createApp(component), parent = vueApp.mount('#test');
      const dataset = (name, hex) => ({Name:name, Elements:[{
        Key:name,Title:name,Category:'Colors',ValueType:'Color',Value:Util.hexToSignedInt(hex),
      }]});
      parent.themeTemplate = {ui_groups:[dataset('menuA','#DEDEDEFF'),dataset('menuB','#888888FF')]};
      const flush = async () => {await Vue.nextTick(); await Vue.nextTick();};
      const child = () => document.querySelector('.color-display').__vueParentComponent.proxy;
      const check = expected => {
        assert.equal(child().hex.toUpperCase(), expected, 'Swatch must show incoming color');
        assert.equal(document.querySelector('.picker_editor input').value.toUpperCase(), expected,
          'Popup must match the current field instead of retaining another color');
      };
      parent.loadProperties({id:'menuA'}); await flush();
      document.querySelector('.color-display').click(); await flush();
      check('#DEDEDEFF');
      const firstUid = child().$.uid;
      child().pickerInstance.closeHandler();
      parent.loadProperties({id:'menuB'}); await flush();
      document.querySelector('.color-display').click(); await flush();
      check('#888888FF');
      assert.notEqual(child().$.uid, firstUid, 'Different fields should not share a picker instance');
      const secondUid = child().$.uid;
      // A theme/color update to the same field must synchronize an existing open picker.
      parent.dataSource = dataset('menuB','#12345680'); await flush();
      assert.equal(child().$.uid, secondUid);
      check('#12345680');
      child().pickerInstance.closeHandler();
      parent.dataSource = dataset('menuB','#ABCDEF00'); await flush();
      document.querySelector('.color-display').click(); await flush();
      check('#ABCDEF00');
      assert.equal(changeEvents, 0, 'Mounting and incoming colors must not emit user edits');
      assert.equal(saves, 0);
      const editor = document.querySelector('.picker_editor input');
      editor.value = '#2468ACFF';
      editor.dispatchEvent(new Event('input', {bubbles:true})); await flush();
      check('#2468ACFF');
      assert.equal(Util.intToHexColor(parent.dataSource.Elements[0].Value), '#2468ACFF');
      assert.equal(changeEvents, 1, 'A user edit must be emitted once without a feedback loop');
      assert.equal(saves, 1);
      vueApp.unmount();
      assert.equal(document.querySelector('.picker_wrapper'), null);
      return 'PASS: menu switch, open/closed prop updates, alpha, silent sync, and user edits';
    })()`);
    console.log(result);
    win.destroy(); app.exit(0);
  } catch (error) {
    console.error(error); if (win) win.destroy(); app.exit(1);
  }
}).catch(error => { console.error(error); app.exit(1); });
