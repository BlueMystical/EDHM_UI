const assert = require('node:assert/strict');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const { test } = require('node:test');
const { readBundle, commitVersion, findRelease, renderNotes } = require('../release-notes.cjs');

const sha = 'a0d14ec51117be9052f1cb337dad33178b7b7525';
const commit = (version = 'v22.02', id = sha) => ({ sha: id, commit: { message: `EDHM ${version} for FDev (Rhino Update)\n\n- Fix HUD\n  - Nested detail\n- Keep \`XML\` options` } });
const archive = { type: 'file', path: 'Odyssey/EDHM_Odyssey_v22.02.zip', sha: '0'.repeat(40) };
const request = commits => async endpoint => endpoint.startsWith('commits?') ? commits : archive;

test('Bundle version comes from the one packaged Odyssey ZIP, preserving its exact version', t => {
  const directory = fs.mkdtempSync(path.join(os.tmpdir(), 'edhm-release-notes-'));
  t.after(() => fs.rmSync(directory, { recursive: true, force: true }));
  fs.writeFileSync(path.join(directory, 'ODYSS_EDHM-Themes.zip'), '');
  assert.throws(() => readBundle(directory), /found 0/);
  fs.writeFileSync(path.join(directory, 'ODYSS_EDHM-v22.02.zip'), '');
  assert.deepEqual(readBundle(directory), { filename: 'ODYSS_EDHM-v22.02.zip', version: 'v22.02' });
  fs.writeFileSync(path.join(directory, 'ODYSS_EDHM-v22.03.zip'), '');
  assert.throws(() => readBundle(directory), /found 2/);
});

test('Exact title matching excludes newer versions, prefixes, suffixes and unrelated mentions', async () => {
  assert.equal(commitVersion('Update readme for EDHM v22.02'), undefined);
  const release = await findRelease('v22.02', request([commit('v22.03'), commit('v22.020'), commit('v22.02a'), commit()]));
  assert.equal(release.commit, sha);
  assert.equal(release.notes, '- Fix HUD\n  - Nested detail\n- Keep `XML` options');
});

test('History pagination resolves older bundled releases and pins the archive lookup to the matched SHA', async () => {
  const calls = [];
  const release = await findRelease('v22.02', async endpoint => {
    calls.push(endpoint);
    if (endpoint.endsWith('&page=1')) return Array.from({ length: 100 }, () => commit('v22.03'));
    if (endpoint.startsWith('commits?')) return [commit()];
    return archive;
  });
  assert.equal(release.commit, sha);
  assert.match(calls[0], /path=Odyssey%2FEDHM_Odyssey_v22.02.zip/);
  assert.match(calls[2], new RegExp(`ref=${sha}$`));
});

test('Missing, ambiguous, empty and incompatible notes stop the release', async () => {
  await assert.rejects(findRelease('v22.02', request([])), /found 0/);
  await assert.rejects(findRelease('v22.02', request([commit(), commit('v22.02', '1'.repeat(40))])), /found 2/);
  await assert.rejects(findRelease('v22.02', request([{ sha, commit: { message: 'EDHM v22.02' } }])), /no patch notes/);
  await assert.rejects(findRelease('v22.02', request([{ sha, commit: { message: 'EDHM v22.02\n\n----' } }])), /separator/);
  await assert.rejects(findRelease('v22.02', async endpoint => endpoint.startsWith('commits?') ? [commit()] : {}), /expected release archive/);
});

test('Release body delivers all notes through the existing app notification contract', async () => {
  const release = await findRelease('v22.02', request([commit()]));
  const body = renderNotes('3.0.71', release, '- Fix theme selection');
  // This is the existing AnalyseUpdate parser in App.vue.
  const notification = body.split('----')[0];
  assert.ok(notification.includes(release.notes));
  assert.ok(notification.includes('- Fix theme selection'));
  assert.ok(notification.includes(release.url));
  assert.ok(!notification.includes('Installing on Windows'));
  assert.ok(body.includes('edhm-ui-v3-windows-x64.exe'));
  assert.ok(body.includes('linux_installer.sh'));
});

test('Network/API failures do not produce fallback or stale release notes', async () => {
  await assert.rejects(findRelease('v22.02', async () => { throw new Error('HTTP 503'); }), /HTTP 503/);
});
