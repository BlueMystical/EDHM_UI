// Render the actual editor and Bootstrap modal without loading game files.
const { app, BrowserWindow } = require('electron');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const { parse, compileTemplate } = require('@vue/compiler-sfc');
const { transformSync } = require('esbuild');
const root = path.resolve(__dirname, '..');
const filename = path.join(root, 'src/MainWindow/Components/XmlEditor.vue');
const { descriptor } = parse(fs.readFileSync(filename, 'utf8'));
const cjs = source => transformSync(source, {format:'cjs'}).code;
const script = cjs(descriptor.script.content);
const template = cjs(compileTemplate({source:descriptor.template.content, filename, id:'xml-test'}).code);
const css = fs.readFileSync(path.join(root, 'node_modules/bootstrap/dist/css/bootstrap.min.css'), 'utf8')
  + descriptor.styles.map(s => s.content).join('\n');
app.setPath('userData', fs.mkdtempSync(path.join(os.tmpdir(), 'edhm-xml-test-')));
app.whenReady().then(async () => {
  let win;
  try {
    win = new BrowserWindow({show:false, width:1400, height:850,
      webPreferences:{nodeIntegration:true, contextIsolation:false, offscreen:true}});
    await win.loadURL('data:text/html,<html data-bs-theme="dark"><body><div id="test"></div></body></html>');
    const results = await win.webContents.executeJavaScript(`(async () => {
      const Vue = require(${JSON.stringify(path.join(root, 'node_modules/vue/dist/vue.cjs.js'))});
      const assert = require('node:assert/strict');
      window.bootstrap = require(${JSON.stringify(path.join(root, 'node_modules/bootstrap/dist/js/bootstrap.bundle.js'))});
      const style = document.createElement('style'); style.textContent=${JSON.stringify(css)}; document.head.append(style);
      const imports = id => id === 'vue' ? Vue : {emit(){}};
      function evaluate(code) {const m={exports:{}}; new Function('require','module','exports',code)(imports,m,m.exports); return m.exports;}
      const component = evaluate(${JSON.stringify(script)}).default;
      component.render = evaluate(${JSON.stringify(template)}).render;
      component.methods.OnInitialize = () => {};
      component.methods.applyFilter = function() { this.TransformXMLColors(); };
      let saves = 0;
      const vm = Vue.createApp(component, {onOnCloseModal(){saves++;}}).mount('#test');
      const modal = document.getElementById('XmlEditorModal');
      const data = {name:'Regression',matrix:[[1,0,0],[0,1,0],[0,0,1]]};
      const waitFor = (event, action) => new Promise(resolve => {modal.addEventListener(event,resolve,{once:true});action();});
      const open = async () => {await waitFor('shown.bs.modal',()=>vm.ShowModal(data));await Vue.nextTick();};
      const close = async label => {
        await waitFor('hidden.bs.modal',()=> {
          if(label === 'Close') modal.querySelector('.modal-header .btn-close').click();
          else [...modal.querySelectorAll('.modal-footer button')].find(b=>b.textContent===label).click();
        });
      };
      const input = () => modal.querySelector('input[type=color]');
      const choose = async color => {input().value=color;input().dispatchEvent(new Event('input',{bubbles:true}));await Vue.nextTick();};
      const errors = [];
      await open();
      bootstrap.Tab.getOrCreateInstance(document.getElementById('profile-tab')).show();
      await new Promise(resolve=>setTimeout(resolve,200));
      const rect = input().getBoundingClientRect(), cell = input().parentElement.getBoundingClientRect();
      const reference = input().parentElement.parentElement.previousElementSibling.children[1].getBoundingClientRect();
      const border = getComputedStyle(input()).borderTopColor;
      if(Math.abs(rect.width-cell.width)>2 || Math.abs(rect.height-cell.height)>2) errors.push('Picker does not fill swatch: '+JSON.stringify({width:rect.width,height:rect.height,cellWidth:cell.width,cellHeight:cell.height}));
      if(Math.abs(rect.width-reference.width)>2 || Math.abs(rect.x-reference.x)>2) errors.push('Custom swatch must align with the other source colors');
      if(border !== 'rgb(255, 165, 0)') errors.push('Picker border is not orange: '+border);
      const original = input().value;
      await choose('#c1efc1'); await close('Cancel'); await open();
      if(input().value !== original) errors.push('Cancel retained draft color: '+input().value+' instead of '+original);
      await choose('#123456'); await close('Apply Changes'); await open();
      if(input().value !== '#123456') errors.push('Apply failed to retain accepted color');
      await choose('#fedcba'); await close('Close'); await open();
      if(input().value !== '#123456') errors.push('Close retained draft color');
      assert.equal(saves,1,'Only Apply should emit changes');
      assert.equal(errors.length,0,errors.join(' | '));
      return {width:rect.width,height:rect.height,border,color:input().value};
    })().catch(error => {throw new Error(error.stack || String(error));})`);
    console.log('PASS', JSON.stringify(results));
    if (process.env.EDHM_XML_TEST_SCREENSHOT) {
      fs.writeFileSync(process.env.EDHM_XML_TEST_SCREENSHOT,(await win.webContents.capturePage()).toPNG());
    }
    win.destroy(); app.exit(0);
  } catch (error) {console.error(error);if(win)win.destroy();app.exit(1);}
}).catch(error=>{console.error(error);app.exit(1);});
