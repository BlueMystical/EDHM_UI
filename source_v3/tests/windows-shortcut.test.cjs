const { test } = require('node:test');
const assert = require('node:assert/strict');
const vm = require('node:vm');
const fs = require('node:fs');
const path = require('node:path');

test('Custom Windows shortcut follows the actual installed executable and redirected desktop', () => {
  const source = fs.readFileSync(path.join(__dirname, '../src/Helpers/FileHelper.js'), 'utf8');
  const code = source.slice(source.indexOf('function createWindowsShortcut('), source.indexOf('function createLinuxShortcut('));
  const exe = "C:\\Users\\O'Brien Ω\\AppData\\Local\\Example.App\\current\\EDHM-UI-V3.exe";
  const desktop = "C:\\Users\\O'Brien Ω\\OneDrive\\Desktop";
  let written;
  vm.runInNewContext(`${code}\ncreateWindowsShortcut('D:\\icon.ico');`, {
    path: path.win32, console,
    app: { getPath: name => name === 'exe' ? exe : desktop },
    shell: { writeShortcutLink(...args) { written = args; return true; } },
  });
  assert.equal(written[0], path.win32.join(desktop, 'EDHM-UI-V3.lnk'));
  assert.equal(written[1], 'create');
  assert.equal(written[2].target, exe);
  assert.equal(written[2].cwd, path.win32.dirname(exe));
});
