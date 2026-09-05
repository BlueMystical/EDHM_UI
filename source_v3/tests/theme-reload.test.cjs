const assert = require('node:assert/strict');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');
const { transformSync } = require('esbuild');

function fixture(t) {
  const directory = fs.mkdtempSync(path.join(os.tmpdir(), 'edhm-reload-'));
  t.after(() => fs.rmSync(directory, { recursive: true, force: true }));
  const game = path.join(directory, 'game');
  const current = path.join(game, 'EDHM-ini');
  fs.mkdirSync(current, {recursive:true});
  const signal = path.join(current, 'ThemeSettings.json');
  fs.writeFileSync(signal, '{}');
  const jobs = [], cache = new Map();
  let writes = 0, active = {key:'ED_Odissey',path:game}, iniSave = async () => true;
  function load(filename) {
    if(cache.has(filename)) return cache.get(filename);
    const module = {exports:{}};
    vm.runInNewContext(transformSync(fs.readFileSync(filename,'utf8'),{format:'cjs'}).code, {
      module, exports:module.exports, console,
      setTimeout(fn, delay) { const job={fn,delay,cancelled:false,unref(){}};jobs.push(job);return job; },
      clearTimeout(job) { job.cancelled=true; },
      require(id) {
        if(id==='electron') return {ipcMain:{handle(){}}};
        if(id.startsWith('node:') || id==='fs') return require(id);
        if(id.includes('ThemeReloadSignal')) return load(path.resolve(path.dirname(filename),id));
        if(id.includes('SettingsHelper')) return {getActiveInstance:()=>active};
        if(id.includes('IniParser')) return {SaveIniFile:(...args)=>iniSave(...args)};
        if(id.includes('FileHelper')) return {
          resolveEnvVariables:value=>value,
          ensureDirectoryExists:folder=>{fs.mkdirSync(folder,{recursive:true});return true;},
          writeJsonFile:(file,data)=>{writes++;fs.writeFileSync(file,JSON.stringify(data));return true;},
          checkFileExists:fs.existsSync,
        };
        return {};
      },
    });
    cache.set(filename,module.exports);return module.exports;
  }
  const helper=load(path.resolve(__dirname,'../src/Helpers/ThemeHelper.js')).default;
  const save=()=>helper.SaveTheme({path:current,credits:{theme:'Dark Wolf'},ui_groups:[]});
  return {directory,current,signal,helper,jobs,save,get writes(){return writes;},
    switchGame:()=>{active={...active,path:path.join(directory,'other-game')};},
    onIniSave:fn=>{iniSave=fn;}};
}

test('One delayed timestamp retry recovers a first-poll miss without rewriting the theme',async t=>{
  const f=fixture(t);
  assert.equal(await f.save(),true);
  // The DLL's first poll accepts this timestamp as its baseline (no reload).
  const baseline=fs.statSync(f.signal).mtimeMs;
  const contents=fs.readFileSync(f.signal);
  assert.equal(f.jobs.length,1,'Successful current-settings writes must queue a reload retry');
  assert.equal(f.jobs[0].delay,3000);
  f.jobs[0].fn();
  assert.ok(fs.statSync(f.signal).mtimeMs>baseline,'The next DLL poll must see a new timestamp');
  assert.deepEqual(fs.readFileSync(f.signal),contents,'Retry must preserve exact file contents');
  assert.equal(f.writes,1);
  assert.equal(f.jobs.length,1,'Retry must be bounded, not reschedule itself');
});

test('A newer apply cancels the old retry before writing any INIs',async t=>{
  const f=fixture(t);await f.save();
  const first=f.jobs[0], baseline=fs.statSync(f.signal).mtimeMs;
  assert.ok(first,'Initial apply must have a pending retry');
  f.onIniSave(async()=>{
    assert.equal(first.cancelled,true);
    first.fn(); // Even an already-queued callback must be harmless.
    assert.equal(fs.statSync(f.signal).mtimeMs,baseline);
    return true;
  });
  await f.helper.SaveThemeINIs(f.current,{StartupProfile:{}});
  await f.save();
  assert.equal(f.jobs.length,2);
  f.jobs[1].fn();
  assert.equal(f.writes,2);
});

test('Later edits supersede a pending retry and inactive instances are never touched',async t=>{
  const f=fixture(t);await f.save();await f.save();
  assert.equal(f.jobs.length,2);
  assert.equal(f.jobs[0].cancelled,true);
  const baseline=fs.statSync(f.signal).mtimeMs;
  f.jobs[0].fn();assert.equal(fs.statSync(f.signal).mtimeMs,baseline);
  f.switchGame();f.jobs[1].fn();
  assert.equal(fs.statSync(f.signal).mtimeMs,baseline);
});

test('Deleted or externally changed signal files are not recreated or retouched',async t=>{
  const f=fixture(t);await f.save();
  assert.equal(f.jobs.length,1);
  fs.unlinkSync(f.signal);f.jobs[0].fn();assert.equal(fs.existsSync(f.signal),false);
  await f.save();fs.appendFileSync(f.signal,' ');
  const changed=fs.statSync(f.signal).mtimeMs;
  f.jobs[1].fn();assert.equal(fs.statSync(f.signal).mtimeMs,changed);
});

test('Saving a library theme never queues a game reload',async t=>{
  const f=fixture(t);
  await f.helper.SaveTheme({path:path.join(f.directory,'themes','Dark Wolf'),credits:{theme:'Dark Wolf'}});
  assert.equal(f.jobs.length,0);
});
