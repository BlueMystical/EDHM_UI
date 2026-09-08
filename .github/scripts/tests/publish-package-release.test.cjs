const assert = require('node:assert/strict');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const { test } = require('node:test');
const { publishRelease } = require('../publish-package-release.cjs');

function fixture(t, existing = []) {
  const directory = fs.mkdtempSync(path.join(os.tmpdir(), 'edhm-publish-'));
  t.after(() => fs.rmSync(directory, { recursive: true, force: true }));
  const env = { INSTALLER_VARIANT: 'free', GITHUB_REPOSITORY: 'BlueMystical/EDHM_UI', GITHUB_EVENT_NAME: 'workflow_dispatch', GITHUB_SHA: 'a'.repeat(40) };
  fs.writeFileSync(path.join(directory, 'release-metadata.json'), JSON.stringify({ appVersion: '3.0.71', uiCommit: env.GITHUB_SHA, edhm: { version: 'v22.02' } }));
  for (const file of ['edhm-ui-v3-windows-x64.exe', 'edhm-ui-v3-linux-x64.zip', 'linux_installer.sh', 'release-notes.md']) fs.writeFileSync(path.join(directory, file), 'fixture');
  const calls = [];
  const gh = (...args) => {
    calls.push(args);
    if (args[0] === 'api') return JSON.stringify(args.includes('--paginate') ? [existing] : []);
    if (args[1] === 'view') return JSON.stringify({ url: 'https://example.test/release', isDraft: false, isPrerelease: false, assets: ['edhm-ui-v3-windows-x64.exe', 'edhm-ui-v3-linux-x64.zip', 'linux_installer.sh'].map(name => ({name})) });
    return '';
  };
  return { directory, env, gh, calls };
}

test('Publishing stages three assets then publishes a stable release at the exact app commit', t => {
  const f = fixture(t);
  publishRelease(f.directory, f.env, f.gh);
  const create = f.calls.find(args => args[1] === 'create');
  assert.ok(create.includes('--draft'));
  assert.ok(!create.includes('--prerelease'));
  assert.ok(!create.includes(path.join(f.directory, 'release-metadata.json')));
  const edit = f.calls.find(args => args[1] === 'edit');
  assert.ok(edit.includes('--draft=false'));
  assert.ok(edit.includes('--prerelease=false'));
  assert.ok(edit.includes('--latest'));
  assert.equal(create[create.indexOf('--target') + 1], f.env.GITHUB_SHA);
  assert.equal(create[create.indexOf('--notes-file') + 1], path.join(f.directory, 'release-notes.md'));
  assert.equal(create[2], 'v3.0.71');
});

test('Published releases cannot be modified and automatic runs cannot publish', t => {
  const f = fixture(t, [{ tag_name: 'v3.0.71', draft: false }]);
  assert.throws(() => publishRelease(f.directory, f.env, f.gh), /already published/);
  assert.equal(f.calls.length, 1);
  assert.throws(() => publishRelease(f.directory, { ...f.env, GITHUB_EVENT_NAME: 'pull_request' }, f.gh), /manual workflow run/);
  assert.equal(f.calls.length, 1);
});

test('Missing assets and mismatched metadata fail before a release is created', t => {
  const f = fixture(t);
  assert.throws(() => publishRelease(f.directory, { ...f.env, GITHUB_SHA: 'b'.repeat(40) }, f.gh), /another app commit/);
  fs.unlinkSync(path.join(f.directory, 'edhm-ui-v3-windows-x64.exe'));
  assert.throws(() => publishRelease(f.directory, f.env, f.gh), /ENOENT/);
  assert.equal(f.calls.length, 0);
});

test('A matching draft can resume, but drafts and tags from other commits cannot be replaced', t => {
  const f = fixture(t, [{ tag_name: 'v3.0.71', draft: true, target_commitish: 'b'.repeat(40) }]);
  assert.throws(() => publishRelease(f.directory, f.env, f.gh), /different build commit/);
  assert.ok(!f.calls.some(args => args[0] === 'release'));
  const retry = fixture(t, [{ tag_name: 'v3.0.71', draft: true, target_commitish: f.env.GITHUB_SHA, assets: [] }]);
  publishRelease(retry.directory, retry.env, retry.gh);
  assert.deepEqual(retry.calls.filter(args => args[0] === 'release').map(args => args[1]), ['upload', 'edit', 'view']);
  const badTag = (...args) => args.includes('--paginate') ? '[[]]' : JSON.stringify([{ ref: 'refs/tags/v3.0.71', object: {type: 'commit', sha: 'b'.repeat(40)} }]);
  assert.throws(() => publishRelease(retry.directory, retry.env, badTag), /tag points to a different build commit/);
});

test('An upload failure leaves the release unpublished', t => {
  const f = fixture(t);
  const gh = (...args) => {
    if (args[1] === 'create') throw new Error('Upload failed');
    return f.gh(...args);
  };
  assert.throws(() => publishRelease(f.directory, f.env, gh), /Upload failed/);
  assert.ok(!f.calls.some(args => args[1] === 'edit'));
});

 test('Forks publish only to their own workflow repository; malformed destinations are rejected', t => {
  const f = fixture(t);
  publishRelease(f.directory, { ...f.env, GITHUB_REPOSITORY: 'Fenris159/EDHM_UI' }, f.gh);
  for (const args of f.calls) {
    if (args[0] === 'release') assert.equal(args[args.indexOf('--repo') + 1], 'Fenris159/EDHM_UI');
    else assert.ok(args.some(value => value.startsWith('repos/Fenris159/EDHM_UI/')));
  }
  assert.throws(() => publishRelease(f.directory, { ...f.env, GITHUB_REPOSITORY: '../another/repo' }, f.gh), /Invalid workflow repository/);
});
