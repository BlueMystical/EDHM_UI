// Exercise rapid clicks against the real Vue confirmation and Bootstrap timing.
const { app, BrowserWindow } = require('electron');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const { parse, compileTemplate } = require('@vue/compiler-sfc');
const { transformSync } = require('esbuild');
const root = path.resolve(__dirname, '..');
const filename = path.join(root, 'src/MainWindow/NavBars.vue');
const { descriptor } = parse(fs.readFileSync(filename, 'utf8'));
const cjs = source => transformSync(source, { format: 'cjs' }).code;
const script = cjs(descriptor.script.content);
const template = cjs(compileTemplate({ source: descriptor.template.content, filename, id: 'confirmation-test' }).code);
const css = fs.readFileSync(path.join(root, 'node_modules/bootstrap/dist/css/bootstrap.min.css'), 'utf8');
app.setPath('userData', fs.mkdtempSync(path.join(os.tmpdir(), 'edhm-confirmation-test-')));
app.whenReady().then(async () => {
  let win;
  try {
    win = new BrowserWindow({ show: false, webPreferences: {
      nodeIntegration: true, contextIsolation: false, offscreen: true, backgroundThrottling: false,
    } });
    await win.loadURL('data:text/html,<html><body><div id="test"></div></body></html>');
    await win.webContents.executeJavaScript(`(async () => {
      const Vue = require(${JSON.stringify(path.join(root, 'node_modules/vue/dist/vue.cjs.js'))});
      const assert = require('node:assert/strict');
      window.bootstrap = require(${JSON.stringify(path.join(root, 'node_modules/bootstrap/dist/js/bootstrap.bundle.js'))});
      const style = document.createElement('style'); style.textContent = ${JSON.stringify(css)}; document.head.append(style);
      const events = [];
      const imports = id => id === 'vue' ? Vue : id.includes('EventBus')
        ? { emit: (...args) => events.push(args) } : { render: () => null };
      function evaluate(code) { const m = { exports: {} }; new Function('require', 'module', 'exports', code)(imports, m, m.exports); return m.exports; }
      const component = evaluate(${JSON.stringify(script)}).default;
      component.render = evaluate(${JSON.stringify(template)}).render;
      component.mounted = () => {};
      Vue.createApp(component).mount('#test');
      const modal = document.getElementById('NewThemeModal');
      const add = document.getElementById('cmdAddNewTheme');
      const yes = document.getElementById('confirmNewTheme');
      const creates = () => events.filter(([name]) => name === 'OnCreateTheme').length;
      // No delay: Yes must work even during the opening transition.
      add.click(); yes.click(); yes.click();
      await new Promise(resolve => setTimeout(resolve, 600));
      assert.equal(modal.classList.contains('show'), false, 'Rapid Yes must dismiss the confirmation');
      assert.equal(creates(), 1, 'Rapid Yes must begin creation exactly once');
      for (const label of ['Cancel', 'No, take me back', 'Close', 'Escape']) {
        add.click();
        if (label === 'Escape') modal.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
        else if (label === 'Close') modal.querySelector('.btn-close').click();
        else [...modal.querySelectorAll('button')].find(button => button.textContent === label).click();
        await Vue.nextTick();
        assert.equal(modal.classList.contains('show'), false, label + ' must dismiss');
        assert.equal(creates(), 1, label + ' must not start another theme');
      }
    })()`);
    console.log('PASS: rapid Yes creates once; Cancel, No, Close and Escape discard');
    win.destroy(); app.exit(0);
  } catch (error) { console.error(error); if (win) win.destroy(); app.exit(1); }
}).catch(error => { console.error(error); app.exit(1); });
