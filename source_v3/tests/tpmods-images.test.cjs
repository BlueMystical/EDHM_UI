const assert = require('node:assert/strict');
const { spawnSync } = require('node:child_process');
const path = require('node:path');
const { test } = require('node:test');

test('3P Mods thumbnails fall back locally without changing layout or inactive styling', { timeout: 30000 }, () => {
  const env = { ...process.env };
  delete env.ELECTRON_RUN_AS_NODE;
  const result = spawnSync(require('electron'), [path.join(__dirname, 'tpmods-images.electron.cjs')], {
    env, encoding: 'utf8', timeout: 25000, windowsHide: true,
  });
  assert.equal(result.status, 0, `${result.error || ''}\n${result.stdout}\n${result.stderr}`);
});
