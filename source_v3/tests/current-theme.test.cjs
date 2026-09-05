const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const os = require('node:os');
const vm = require('node:vm');
const { test } = require('node:test');
const { parse } = require('@vue/compiler-sfc');
const { transformSync } = require('esbuild');

const clone = value => JSON.parse(JSON.stringify(value));
const savedTheme = () => ({credits:{theme:'Dark Wolf'},ui_groups:[{Name:'HUD',Elements:[
  {Key:'color',Value:-7829368}, {Key:'brightness',Value:1},
]}],xml_profile:[{key:'x150',value:1},{key:'y150',value:0}]});

function fixture() {
  let current = savedTheme();
  let files = [savedTheme()];
  let overrides = null;
  const api = {
    getActiveInstance: async () => ({key:'ED_Odissey',path:'game'}),
    resolveEnvVariables: async value => value,
    joinPath: (...parts) => parts.join('/'),
    getAssetFileUrl: async value => value,
    getThemes: async () => files.map(theme => ({theme:clone(theme),credits:clone(theme.credits),path:'themes/'+theme.credits.theme})),
    fileExists: async () => false,
    GetCurrentSettings: async () => clone(current),
    LoadGlobalSettings: async () => clone(overrides),
    LoadUserSettings: async () => null,
  };
  function load(filename) {
    const source = fs.readFileSync(filename,'utf8');
    const code = transformSync(filename.endsWith('.vue') ? parse(source).descriptor.script.content : source,{format:'cjs'}).code;
    const module = {exports:{}};
    vm.runInNewContext(code, {module,exports:module.exports,console,window:{api},
      document:{querySelectorAll:()=>[]},
      require(id) {
        if(id.includes('EventBus')) return {emit(){}};
        if(id.includes('CurrentThemeStatus')) return load(path.resolve(path.dirname(filename),id));
        return {};
      },
    });
    return module.exports;
  }
  const component = load(path.resolve(__dirname,'../src/MainWindow/ThemeTab.vue')).default;
  const instance = () => {
    const result = {...component.data(),programSettings:{UserDataFolder:'data',FavToogle:false}};
    for(const [name,method] of Object.entries(component.methods)) result[name]=method.bind(result);
    return result;
  };
  return {instance,component,api,setCurrent:v=>{current=clone(v);},setThemes:v=>{files=clone(v);},setOverrides:v=>{overrides=clone(v);}};
}

test('Startup reconstructs applied theme identity from persisted settings', async () => {
  const f=fixture();
  const first=f.instance(); await first.loadThemes();
  assert.equal(first.themes[0].name,'Current Settings: Dark Wolf');
  const restarted=f.instance(); await restarted.loadThemes();
  assert.equal(restarted.themes[0].name,'Current Settings: Dark Wolf');
  restarted.selectedTheme=restarted.themes[0];
  assert.equal(f.component.computed.isCurrentSettings.call(restarted),true);
  assert.equal(restarted.themes[0].file.credits.theme,'Current Settings','Keep the special current-settings route');
});

test('Color and XML edits become Custom, survive restart, and revert to a match', async () => {
  const f=fixture(), tab=f.instance(); await tab.loadThemes();
  for(const change of [theme=>{theme.ui_groups[0].Elements[0].Value=42;},theme=>{theme.xml_profile[0].value=0.5;}]) {
    const changed=savedTheme(); change(changed); f.setCurrent(changed);
    await tab.CurretSettingsUpdated(changed);
    assert.equal(tab.themes[0].name,'Current Settings: Custom');
    assert.match(tab.themes[0].status,/Dark Wolf/);
    const restarted=f.instance();await restarted.loadThemes();
    assert.equal(restarted.themes[0].name,'Current Settings: Custom');
    f.setCurrent(savedTheme()); await tab.CurretSettingsUpdated(savedTheme());
    assert.equal(tab.themes[0].name,'Current Settings: Dark Wolf');
  }
});

test('Global/User overrides and harmless metadata do not look like unsaved theme changes',async()=>{
  const f=fixture(), current=savedTheme();
  current.ui_groups[0].Elements[1].Value='0.5';
  current.ui_groups[0].Elements.reverse();current.xml_profile.reverse();
  current.path='game';current.version='v22.02';current.credits.author='You';
  f.setOverrides({Elements:[{Key:'brightness',Value:0.5}]});f.setCurrent(current);
  const tab=f.instance();await tab.loadThemes();
  assert.equal(tab.themes[0].name,'Current Settings: Dark Wolf');
  assert.match(tab.themes[0].status,/overrides/i);
});

test('Missing source themes are explicitly unverified and never reported as a match',async()=>{
  const f=fixture();f.setThemes([]);
  const tab=f.instance();await tab.loadThemes();
  assert.equal(tab.themes[0].name,'Current Settings: Unverified');
  assert.match(tab.themes[0].status,/Dark Wolf/);
});

test('Saving custom settings as a copy restores a named match without reapplying',async()=>{
  const f=fixture(), changed=savedTheme();changed.ui_groups[0].Elements[0].Value=123;
  f.setCurrent(changed);
  const tab=f.instance();await tab.loadThemes();
  assert.equal(tab.themes[0].name,'Current Settings: Custom');
  const copy=clone(changed);copy.credits.theme='My Wolf';
  f.setThemes([savedTheme(),copy]);await tab.loadThemes();
  assert.equal(tab.themes[0].name,'Current Settings: My Wolf');
});

test('Real ThemeHelper save/load preserves identity across process-style reloads and custom edits',async t=>{
  const directory=fs.mkdtempSync(path.join(os.tmpdir(),'edhm-theme-identity-'));
  t.after(()=>fs.rmSync(directory,{recursive:true,force:true}));
  const templatePath=path.join(directory,'template.json');
  fs.writeFileSync(templatePath,JSON.stringify(savedTheme()));
  const gamePath=path.join(directory,'game','EDHM-ini');
  const fileHelper={
    getAssetPath:()=>templatePath,
    loadJsonFile:async file=>JSON.parse(fs.readFileSync(file,'utf8')),
    ensureDirectoryExists:folder=>{fs.mkdirSync(folder,{recursive:true});return true;},
    writeJsonFile:(file,data)=>fs.writeFileSync(file,JSON.stringify(data)),
    checkFileExists:fs.existsSync,
  };
  const settings={getActiveInstance:async()=>({key:'ED_Odissey'})};
  const filename=path.resolve(__dirname,'../src/Helpers/ThemeHelper.js');
  function loadHelper(){
    const module={exports:{}};
    vm.runInNewContext(transformSync(fs.readFileSync(filename,'utf8'),{format:'cjs'}).code,{
      module,exports:module.exports,console,
      require(id){
        if(id==='electron')return {ipcMain:{handle(){}}};
        if(id.startsWith('node:')||id==='fs')return require(id);
        if(id.includes('SettingsHelper'))return settings;
        if(id.includes('FileHelper'))return fileHelper;
        return {};
      },
    });
    return module.exports.default;
  }
  const helper=loadHelper(), f=fixture();
  assert.equal(await helper.SaveTheme({...savedTheme(),path:gamePath}),true);
  f.api.GetCurrentSettings=()=>loadHelper().GetCurrentSettingsTheme(gamePath);
  let tab=f.instance();await tab.loadThemes();
  assert.equal(tab.themes[0].name,'Current Settings: Dark Wolf');
  const changed=await helper.GetCurrentSettingsTheme(gamePath);
  changed.ui_groups[0].Elements[0].Value=123;
  assert.equal(await helper.SaveTheme(changed),true);
  tab=f.instance();await tab.loadThemes();
  assert.equal(tab.themes[0].name,'Current Settings: Custom');
  // Updating the saved theme to these values restores the match on reload.
  f.setThemes([changed]);await tab.loadThemes();
  assert.equal(tab.themes[0].name,'Current Settings: Dark Wolf');
});

test('Switching game instances compares each current-settings file independently',async()=>{
  const f=fixture();let game='odyssey';
  f.api.getActiveInstance=async()=>({key:game==='odyssey'?'ED_Odissey':'ED_Horizons',path:game});
  f.api.GetCurrentSettings=async file=>{
    const theme=savedTheme();
    if(file.startsWith('horizons/'))theme.xml_profile[0].value=0.7;
    return theme;
  };
  const tab=f.instance();await tab.loadThemes();
  assert.equal(tab.themes[0].name,'Current Settings: Dark Wolf');
  game='horizons';await tab.loadThemes();
  assert.equal(tab.themes[0].name,'Current Settings: Custom');
  game='odyssey';await tab.loadThemes();
  assert.equal(tab.themes[0].name,'Current Settings: Dark Wolf');
});

test('An older asynchronous comparison cannot overwrite a newer settings status',async()=>{
  const f=fixture(),tab=f.instance();await tab.loadThemes();
  let finishOldRead;
  f.api.GetCurrentSettings=()=>new Promise(resolve=>{finishOldRead=resolve;});
  const old=tab.CurretSettingsUpdated();
  f.api.GetCurrentSettings=async()=>{const theme=savedTheme();theme.xml_profile[0].value=0.2;return theme;};
  await tab.CurretSettingsUpdated();
  finishOldRead(savedTheme());await old;
  assert.equal(tab.themes[0].name,'Current Settings: Custom');
});
