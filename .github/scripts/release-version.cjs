const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../..');
const pkg = JSON.parse(fs.readFileSync(path.join(root, 'source_v3/package.json'), 'utf8'));
const lock = JSON.parse(fs.readFileSync(path.join(root, 'source_v3/package-lock.json'), 'utf8'));
// Advanced Installer's ProductVersion uses three numeric fields.
if (!/^\d+\.\d+\.\d+$/.test(pkg.version)) throw new Error('Expected a numeric major.minor.patch app version');
const [major, minor, patch] = pkg.version.split('.').map(Number);
if (major > 255 || minor > 255 || patch > 65535) throw new Error('App version exceeds Windows Installer limits');
if (pkg.version !== lock.version || pkg.version !== lock.packages[''].version) {
  throw new Error('package.json and package-lock.json versions differ');
}
console.log(`Packaging EDHM-UI v${pkg.version}`);
if (process.env.GITHUB_OUTPUT) fs.appendFileSync(process.env.GITHUB_OUTPUT, `version=${pkg.version}\n`);
