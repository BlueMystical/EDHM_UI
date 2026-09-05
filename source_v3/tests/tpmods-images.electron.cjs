const { app, BrowserWindow } = require('electron');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const http = require('node:http');
const { parse, compileTemplate } = require('@vue/compiler-sfc');
const { transformSync } = require('esbuild');
const root = path.resolve(__dirname, '..');
const filename = path.join(root, 'src/TPMods/TPModsManagerPop.vue');
const { descriptor } = parse(fs.readFileSync(filename, 'utf8'));
const cjs = source => transformSync(source, { format: 'cjs' }).code;
const script = cjs(descriptor.script.content);
const template = cjs(compileTemplate({ source: descriptor.template.content, filename, id: 'tpmods-test' }).code);
const css = fs.readFileSync(path.join(root, 'node_modules/bootstrap/dist/css/bootstrap.min.css'), 'utf8')
  + descriptor.styles.map(style => style.content).join('\n');
const placeholder = fs.readFileSync(path.join(root, 'src/images/3PM_Default.png'));
const fallbackUrl = 'data:image/png;base64,' + placeholder.toString('base64');
app.setPath('userData', fs.mkdtempSync(path.join(os.tmpdir(), 'edhm-tpmods-images-')));
app.whenReady().then(async () => {
  let win;
  const server = http.createServer((request, response) => {
    if (request.url === '/down.png') { request.socket.destroy(); return; }
    if (request.url === '/ok.png') {
      response.writeHead(200, { 'Content-Type': 'image/png' }); response.end(placeholder);
    } else { response.writeHead(404); response.end(); }
  });
  try {
    await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
    const host = `http://127.0.0.1:${server.address().port}`;
    win = new BrowserWindow({ show: false, width: 1100, height: 850, webPreferences: {
      nodeIntegration: true, contextIsolation: false, offscreen: true, backgroundThrottling: false,
    } });
    await win.loadURL('data:text/html,<html><body><div id="test"></div></body></html>');
    await win.webContents.executeJavaScript(`(async () => {
      const Vue = require(${JSON.stringify(path.join(root, 'node_modules/vue/dist/vue.cjs.js'))});
      const assert = require('node:assert/strict');
      window.bootstrap = require(${JSON.stringify(path.join(root, 'node_modules/bootstrap/dist/js/bootstrap.bundle.js'))});
      const style = document.createElement('style'); style.textContent = ${JSON.stringify(css)}; document.head.append(style);
      const fallback = ${JSON.stringify(fallbackUrl)}, host = ${JSON.stringify(host)};
      const imports = id => id === 'vue' ? Vue : id.endsWith('.png') ? fallback
        : id.includes('EventBus') ? { on() {}, off() {}, emit() {} } : { render: () => null };
      function evaluate(code) { const m = { exports: {} }; new Function('require', 'module', 'exports', code)(imports, m, m.exports); return m.exports; }
      const component = evaluate(${JSON.stringify(script)}).default;
      component.render = evaluate(${JSON.stringify(template)}).render;
      component.mounted = () => {};
      const vm = Vue.createApp(component).mount('#test');
      const mod = (name, url, isActive) => ({ mod_name: name, thumbnail_url: url, isActive });
      vm.TPmods = [{ ...mod('Parent', host + '/missing.png', true), childs: [
        mod('Offline', host + '/down.png', false), mod('Empty', '', true), mod('Healthy', host + '/ok.png', true),
      ] }];
      vm.alert.thumbnail = host + '/missing-alert.png';
      vm.visible = vm.showAlert = true;
      await Vue.nextTick();
      const images = [...document.querySelectorAll('#thumbnailAccordion img, .right-column .alert img')];
      assert.equal(images.length, 5);
      const waitFor = async predicate => {
        for (let i = 0; i < 100 && !predicate(); i++) await new Promise(resolve => setTimeout(resolve, 25));
        assert.ok(predicate(), 'Image failed to resolve to a drawable local fallback');
      };
      await waitFor(() => images.every(image => image.complete && image.naturalWidth > 0));
      for (const index of [0, 1, 2, 4]) assert.equal(images[index].src, fallback);
      assert.equal(images[3].src, host + '/ok.png', 'Healthy remote images must remain in use');
      assert.equal(getComputedStyle(images[1]).filter, 'grayscale(1)', 'Inactive fallback must stay grayscale');
      const offline = images[1].getBoundingClientRect(), healthy = images[3].getBoundingClientRect();
      assert.equal(offline.width, healthy.width);
      assert.equal(offline.height, healthy.height);
      assert.ok(offline.width > 0 && offline.height > 0);
      assert.ok(Math.abs(offline.width / offline.height - 200 / 60) < 0.05);
      const oldSource = images[0].src;
      images[0].dispatchEvent(new Event('error'));
      assert.equal(images[0].src, oldSource, 'A fallback error must not start another retry');
      vm.TPmods[0].thumbnail_url = host + '/ok.png';
      await Vue.nextTick();
      await waitFor(() => images[0].src === host + '/ok.png' && images[0].complete && images[0].naturalWidth > 0);
    })()`);
    console.log('PASS: 404, offline, empty, healthy and changed image URLs; stable layout and grayscale');
    win.destroy(); server.close(); app.exit(0);
  } catch (error) { console.error(error); if (win) win.destroy(); server.close(); app.exit(1); }
}).catch(error => { console.error(error); app.exit(1); });
