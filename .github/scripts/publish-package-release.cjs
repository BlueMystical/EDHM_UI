const fs = require('node:fs');
const path = require('node:path');
const { execFileSync } = require('node:child_process');

function publishRelease(directory, env = process.env, gh = (...args) => execFileSync('gh', args, { encoding: 'utf8', stdio: ['ignore', 'pipe', 'pipe'] })) {
  directory = path.resolve(directory);
  const variant = env.INSTALLER_VARIANT;
  if (!['free', 'velopack'].includes(variant)) throw new Error('Invalid installer variant');
  if (!/^[A-Za-z0-9-]+\/[A-Za-z0-9_.-]+$/.test(env.GITHUB_REPOSITORY || '')) throw new Error('Invalid workflow repository');
  if (env.GITHUB_EVENT_NAME !== 'workflow_dispatch') throw new Error('Publishing requires a manual workflow run');
  const metadata = JSON.parse(fs.readFileSync(path.join(directory, 'release-metadata.json'), 'utf8'));
  if (metadata.uiCommit !== env.GITHUB_SHA) throw new Error('Release metadata belongs to another app commit');
  if (!/^\d+\.\d+\.\d+$/.test(metadata.appVersion)) throw new Error('Invalid application version');
  const tag = `v${metadata.appVersion}`;
  const assets = ['edhm-ui-v3-windows-x64.exe', 'edhm-ui-v3-linux-x64.zip', 'linux_installer.sh']
    .map(name => path.join(directory, name));
  for (const file of assets) if (!fs.statSync(file).size) throw new Error(`Empty release asset: ${file}`);
  // Enumerate rather than treating any failed lookup (auth/network) as "not found".
  const pages = JSON.parse(gh('api', '--paginate', '--slurp', `repos/${env.GITHUB_REPOSITORY}/releases?per_page=100`));
  const existing = pages.flat().find(release => release.tag_name === tag);
  if (existing && !existing.draft) throw new Error(`${tag} is already published; increment the app version to publish another release`);
  // --target does not move an existing tag. Reject any tag pointing at other code.
  const refs = JSON.parse(gh('api', `repos/${env.GITHUB_REPOSITORY}/git/matching-refs/tags/${tag}`));
  let object = refs.find(ref => ref.ref === `refs/tags/${tag}`)?.object;
  for (let depth = 0; object?.type === 'tag' && depth < 10; depth++) {
    object = JSON.parse(gh('api', `repos/${env.GITHUB_REPOSITORY}/git/tags/${object.sha}`)).object;
  }
  if (object && (object.type !== 'commit' || object.sha !== env.GITHUB_SHA)) throw new Error('Release tag points to a different build commit');
  if (existing && existing.target_commitish !== env.GITHUB_SHA) throw new Error('Existing draft belongs to a different build commit');
  // Stage all files before publishing, so users never see a half-uploaded release.
  if (!existing) {
    gh('release', 'create', tag, '--draft', '--target', env.GITHUB_SHA,
      '--title', tag, '--notes-file', path.join(directory, 'release-notes.md'), '--repo', env.GITHUB_REPOSITORY, ...assets);
  } else {
    const unexpected = existing.assets?.filter(asset => !assets.some(file => path.basename(file) === asset.name)) || [];
    if (unexpected.length) throw new Error('Existing draft has unexpected assets; review it before publishing');
    gh('release', 'upload', tag, ...assets, '--clobber', '--repo', env.GITHUB_REPOSITORY);
  }
  gh('release', 'edit', tag, '--draft=false', '--prerelease=false', '--latest', '--title', tag,
    '--notes-file', path.join(directory, 'release-notes.md'), '--repo', env.GITHUB_REPOSITORY);
  const result = JSON.parse(gh('release', 'view', tag, '--json', 'url,isDraft,isPrerelease,assets', '--repo', env.GITHUB_REPOSITORY));
  if (result.isDraft || result.isPrerelease) throw new Error('Expected a published stable release');
  const names = result.assets.map(asset => asset.name).sort();
  if (JSON.stringify(names) !== JSON.stringify(assets.map(file => path.basename(file)).sort())) throw new Error('Release assets do not match the upstream layout');
  console.log(result.url);
  if (env.GITHUB_STEP_SUMMARY) fs.appendFileSync(env.GITHUB_STEP_SUMMARY, `\n[Published ${tag}](${result.url}) using the ${variant} Windows installer. GitHub supplies source ZIP/tar.gz archives from the release tag.\n`);
}
module.exports = { publishRelease };
if (require.main === module) publishRelease(process.argv[2]);
