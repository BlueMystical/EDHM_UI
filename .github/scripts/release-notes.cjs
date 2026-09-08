const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');

const upstream = 'psychicEgg/EDHM';
const root = path.resolve(__dirname, '../..');

function readBundle(directory) {
  // Derive the shader version from the bundled Odyssey archive.
  const pattern = /^ODYSS_EDHM-(v\d+(?:\.\d+)*(?:\.?[a-z]+)?)\.zip$/i;
  const bundles = fs.readdirSync(directory, { withFileTypes: true })
    .filter(entry => entry.isFile() && pattern.test(entry.name));
  if (bundles.length !== 1) throw new Error(`Expected one Odyssey EDHM bundle; found ${bundles.length}`);
  const filename = bundles[0].name;
  return { filename, version: filename.match(pattern)[1] };
}

async function github(endpoint) {
  const response = await fetch(`https://api.github.com/repos/${upstream}/${endpoint}`, {
    headers: {
      Accept: 'application/vnd.github+json',
      'X-GitHub-Api-Version': '2022-11-28',
      ...(process.env.GH_TOKEN ? { Authorization: `Bearer ${process.env.GH_TOKEN}` } : {}),
    },
    signal: AbortSignal.timeout(30000),
  });
  if (!response.ok) throw new Error(`EDHM release lookup failed: HTTP ${response.status}`);
  return response.json();
}

function commitVersion(message) {
  // The complete token must match: v22.02 must never select v22.020 or v22.02a.
  return /^EDHM\s+(v\d+(?:\.\d+)*(?:\.?[a-z]+)?)(?=\s|$)/i.exec(message.split('\n')[0])?.[1];
}

async function findRelease(version, request = github) {
  const archivePath = `Odyssey/EDHM_Odyssey_${version}.zip`;
  const matches = new Map();
  let complete = false;
  // Scope history to this version's archive; do not select the latest EDHM release.
  for (let page = 1; page <= 20; page++) {
    const commits = await request(`commits?sha=main&path=${encodeURIComponent(archivePath)}&per_page=100&page=${page}`);
    if (!Array.isArray(commits)) throw new Error('Invalid EDHM commit history response');
    for (const commit of commits) {
      if (commitVersion(commit.commit?.message || '') === version) matches.set(commit.sha, commit);
    }
    if (commits.length < 100) { complete = true; break; }
  }
  if (!complete) throw new Error('EDHM commit search exceeded its history limit');
  if (matches.size !== 1) throw new Error(`Expected one EDHM ${version} release commit; found ${matches.size}`);
  const commit = [...matches.values()][0];
  if (!/^[a-f0-9]{40}$/.test(commit.sha)) throw new Error('Invalid EDHM commit SHA');
  const message = commit.commit.message.replace(/\r\n/g, '\n').trim();
  const [title, ...lines] = message.split('\n');
  const notes = lines.join('\n').trim();
  if (!notes) throw new Error(`EDHM ${version} release commit has no patch notes`);
  // This separator is reserved by EDHM-UI's update notification parser.
  if (message.includes('----')) throw new Error('EDHM notes contain the reserved update-notification separator');
  const archive = await request(`contents/${archivePath.split('/').map(encodeURIComponent).join('/')}?ref=${commit.sha}`);
  if (archive.type !== 'file' || archive.path !== archivePath || !/^[a-f0-9]{40}$/.test(archive.sha)) {
    throw new Error('Matching EDHM commit does not contain the expected release archive');
  }
  return { version, commit: commit.sha, url: `https://github.com/${upstream}/commit/${commit.sha}`,
    title, notes, archivePath, archiveBlob: archive.sha };
}

function renderNotes(appVersion, release, uiNotes = '') {
  if (uiNotes.includes('----')) throw new Error('UI notes contain the reserved update-notification separator');
  return [
    `## EDHM-UI v${appVersion}`, '',
    `Includes EDHM ${release.version}.`, '',
    ...(uiNotes.trim() ? ['### EDHM-UI changes', '', uiNotes.trim(), ''] : []),
    `### ${release.title}`, '', release.notes, '',
    `[EDHM patch notes source (${release.commit.slice(0, 7)})](${release.url})`, '',
    '----', '',
    '### Installing on Windows', '',
    '- Download `edhm-ui-v3-windows-x64.exe` from Assets and run it.',
    '- Launch EDHM-UI from its Desktop or Start Menu shortcut after setup.', '',
    '### Installing on Linux', '',
    '- Download `edhm-ui-v3-linux-x64.zip` and `linux_installer.sh` into the same folder.',
    '- Run `chmod +x linux_installer.sh`, then `./linux_installer.sh`.',
    '- Default installation folder: `~/.local/share/EDHM-UI-V3`.', '',
  ].join('\n');
}

async function main() {
  const appVersion = JSON.parse(fs.readFileSync(path.join(root, 'source_v3/package.json'), 'utf8')).version;
  const directory = path.join(root, 'source_v3/src/data/ODYSS');
  const bundle = readBundle(directory);
  const release = await findRelease(bundle.version);
  const uiNotesPath = path.join(root, '.github/release-notes', `v${appVersion}.md`);
  const uiNotes = fs.existsSync(uiNotesPath) ? fs.readFileSync(uiNotesPath, 'utf8') : '';
  const output = path.join(root, 'source_v3/out/release/notes');
  fs.mkdirSync(output, { recursive: true });
  const body = renderNotes(appVersion, release, uiNotes);
  fs.writeFileSync(path.join(output, 'release-notes.md'), body);
  fs.writeFileSync(path.join(output, 'release-metadata.json'), JSON.stringify({
    appVersion, uiCommit: process.env.GITHUB_SHA || null, edhm: release,
    bundle: { ...bundle, sha256: crypto.createHash('sha256').update(fs.readFileSync(path.join(directory, bundle.filename))).digest('hex') },
  }, null, 2) + '\n');
  console.log(`EDHM-UI v${appVersion}: ${bundle.filename} -> ${release.url}`);
  if (process.env.GITHUB_OUTPUT) fs.appendFileSync(process.env.GITHUB_OUTPUT, `edhm_version=${bundle.version}\nedhm_commit=${release.commit}\n`);
  if (process.env.GITHUB_STEP_SUMMARY) fs.appendFileSync(process.env.GITHUB_STEP_SUMMARY, body);
}

module.exports = { readBundle, commitVersion, findRelease, renderNotes };
if (require.main === module) main().catch(error => { console.error(error.message); process.exitCode = 1; });
