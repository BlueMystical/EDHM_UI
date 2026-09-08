# Release packaging

Two independent GitHub Actions workflows build Windows and Linux packages from the
same source commit. Neither requires an installer license or a personal access token.

| Workflow | Windows installer | Configuration |
| --- | --- | --- |
| **Package release (Velopack)** | Velopack Setup with a native launcher that preserves the app's UAC behavior | Set `VELOPACK_PACK_ID` before manual/main builds |
| **Package release (free installer)** | WiX MSI inside an NSIS EXE, compatible with the legacy Advanced Installer MSI family | No repository variables or installer license |

Both workflows are complete definitions, usable upstream or in a fork without
editing a repository name or pointing at another branch. The application version
comes from `source_v3/package.json` and must match `package-lock.json`. This packaging
change does not bump the app or shader version and is independent of the v22.02 app update.

## How to run and publish

Once merged into the default branch, open **Actions**, choose either workflow, and
click **Run workflow**. Select the branch to build. **Publish release** defaults to
unchecked in both workflows: the run produces downloadable Actions artifacts only.
Check it to publish a normal stable release after all tests and both builds succeed.
There is no version input: commit the intended version and lockfile before running.
GitHub requires the workflow on the default branch for the Run workflow button;
see [manual workflow runs](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/manually-run-a-workflow).

Changes to application/package sources, installer scripts, release notes, or these
workflows also trigger build-only checks on pull requests targeting `main` and
pushes to `main`. Pull requests use ordinary `pull_request`, read-only permissions,
and an isolated Velopack test identity. They cannot publish. No developer installer
is executed locally: installation tests are restricted to disposable hosted runners.

Publishing uses the run's own `GITHUB_REPOSITORY` and exact `GITHUB_SHA`; a fork run
publishes only in that fork. Only the manual publish job has `contents: write`.
The built-in `GITHUB_TOKEN` is sufficient if repository/organization policy permits
that permission. No Advanced Installer key, third-party service, or PAT is needed.

Choose **one** Windows installer family for a given app version. Both workflows
publish the same release asset names, so running both in publishing mode is not a
way to offer two installers in one release. Publishing is serialized by app version.
An existing published version cannot be overwritten; increment the version for a
new release. Tags/drafts from another source commit and unexpected draft assets are
rejected. An interrupted upload can leave a draft; rerun the same commit to resume.

## Velopack: one-time maintainer setup

1. Find the **existing Velopack package ID** in the local `vpk pack --packId` command,
   build script, or the `<id>` element of the `.nuspec` inside an existing full `.nupkg`
   (open it as a ZIP). Preserve the exact value. The EXE filename, app display name,
   and GitHub repository do not establish this identity.
2. Open **Settings → Secrets and variables → Actions → Variables** and create
   **`VELOPACK_PACK_ID`** with that value. This is a repository variable, not a secret
   or license key. Missing/invalid values fail early with setup guidance.
3. Run **Package release (Velopack)** with **Publish release** unchecked. Download its
   `windows-velopack` artifact and test an upgrade over the actual previous Velopack
   installer, including launch, settings retention, shortcuts, and uninstall.
4. After that succeeds, increment the app version and run with **Publish release**
   checked. Use the Velopack workflow for future releases of that installation family.

PR builds use `EDHMUI.CI.PackagingTest` regardless of repository configuration. Fork
maintainers can use that ID as their variable for manual build-only testing. Both it
and the earlier `Fenris.EDHMUI.PackagingTest` identity are blocked from publication.
These test IDs do not establish compatibility with the maintainer's existing build.

`.github/installers/velopack/sdk.json` pins the native SDK version and SHA-256 of its
official archive. CI installs the same `vpk` version. Update both fields together when
upgrading Velopack; no per-release edits to these values are needed. App version,
author, and display name come from the existing app metadata; icon, executable,
shortcut options, and packaging flags are in `build-velopack-installer.ps1`.

Installers are currently **unsigned**. Signing is optional and is not wired to a
certificate secret by this PR. If desired, add credentials through Actions secrets
and the [Velopack signing options](https://docs.velopack.io/reference/cli/content/vpk-windows).
Do not put signing passwords or certificates in workflow source.

## Matching shader patch notes and release layout

Before packaging, `release-notes.cjs` derives the EDHM version from the sole bundled
`source_v3/src/data/ODYSS/ODYSS_EDHM-v*.zip`. It searches `psychicEgg/EDHM` main history
for that exact release archive and `EDHM vVERSION` commit-title token, then verifies
that the matching commit contains the archive. Missing/ambiguous matches, empty
notes, and API errors fail the run instead of selecting the latest shader release.

For v22.02 this resolves to
[`a0d14ec`](https://github.com/psychicEgg/EDHM/commit/a0d14ec51117be9052f1cb337dad33178b7b7525).
Upstream main's v22.01 bundle resolves independently to
[`1fd086d`](https://github.com/psychicEgg/EDHM/commit/1fd086d35deb93333c3055a53425d52ff89773bb).
The complete matching commit body, including credits, is retained with its source
link. Optional UI notes go in `.github/release-notes/vAPP_VERSION.md`. Both appear
before `----`, the existing updater's notification separator; installation guidance
follows it. The app's GitHub update endpoint and full-installer update model remain unchanged.

Publishing creates a stable release titled/tagged `vAPP_VERSION`, marked Latest, with:

- `edhm-ui-v3-windows-x64.exe`
- `edhm-ui-v3-linux-x64.zip`
- `linux_installer.sh`

GitHub supplies Source code (zip) and Source code (tar.gz) from the exact release tag.
Notes are the release body. `release-notes.md`, `release-metadata.json`, payload
manifests, and Velopack `.nupkg`/feed files remain Actions artifacts, not public release
assets. Metadata records the app commit, bundled ZIP hash, shader version, matching
upstream commit, and archive blob. Matching by version does not claim that the
repackaged UI ZIP has identical bytes to the upstream shader archive.

Files are uploaded into a staging draft and published only after successful upload.
The finished release is not a draft. Build artifacts and Velopack diagnostics are
retained for 14 days.

## Windows behavior

### Velopack and UAC

Velopack remains a per-user installation. `EDHM-UI-V3.Launcher.exe` is a small native
`asInvoker` executable using the official C++ SDK at entry. `vpk --mainExe` points to
it so install/update/obsolete/uninstall hooks exit before Electron or settings code.
This avoids CreateProcess error 740 when maintenance starts without elevation.

For normal launches, the launcher uses ShellExecuteEx with the `open` verb, preserving
arguments and working directory. The existing `EDHM-UI-V3.exe` retains its
`highestAvailable` manifest: administrators receive the normal consent prompt and
standard users keep their original non-elevated behavior. Cancelling consent closes
the launcher without retrying or undoing installation. Other launch errors are shown.
Setup is not forced to run as a different administrator or use a different profile.

Only the launcher and `velopack_libc.dll` are added to the Velopack payload. The native
runtime is statically linked; its SDK DLL needs only Windows libraries. Hosted runners
provide MSVC/CMake. Local native builds need Visual Studio C++ build tools and CMake.
Custom app shortcuts and hotfix paths now follow the running executable, including
Velopack's `current` directory, instead of assuming the legacy install folder.

The app continues to download the full installer from GitHub. This does not enable
Velopack UpdateManager, delta updates, or a public Velopack feed. The original AIP
project is retained for legacy MSI identity; obsolete Squirrel startup/maker code is
removed because it does not implement either new installer's lifecycle.

**Compatibility boundary:** matching the existing package ID is necessary for
Velopack upgrades, but the maintainer's previous installer and local customizations
must still be checked. Velopack does not automatically migrate an Advanced Installer
or WiX MSI installation. Do not point it at an MSI-owned directory to simulate migration.

References: [Velopack Actions](https://docs.velopack.io/distributing/github-actions),
[C++ SDK](https://docs.velopack.io/getting-started/cpp),
[Setup options](https://docs.velopack.io/reference/cli/content/setup-windows).

### Free installer / legacy MSI family

WiX generates stable per-file components, a fresh product code, and the upgrade code
read from the original `.aip`. NSIS carries the MSI; Windows Installer handles
installation, repair, rollback, upgrades, downgrade rejection, and uninstall. It
retains the legacy machine-scoped MSI registration and default
`%LOCALAPPDATA%\EDHM-UI-V3` path, with existing locations read from the legacy registry.
Desktop/Start Menu shortcuts are selectable on first install. User-owned files and
settings are preserved. The dialogs reproduce functionality, not Advanced Installer's
proprietary visual design or bootstrapper switches.

The Finish launch option uses a small ShellExecuteEx helper so Windows can show the
app's UAC prompt. Silent installs never launch the app. MSI source is cached under
`%LOCALAPPDATA%\Blue Mystic\EDHM-UI-V3\Installer\VERSION\PRODUCT-CODE` for maintenance;
that installer-only cache remains after uninstall. Same-version rebuilds replace the
cached package; published application changes must receive a new version.

WiX **5.0.2** and NSIS **3.12** are pinned. WiX 5 is an older, out-of-support build tool,
chosen before the maintenance-fee terms introduced in WiX 6. Revisit licensing and
compatibility when updating it. Only compiler/dialog definitions are used; WiX Burn
and WiX native custom-action DLLs are not shipped.
See [WiX lifecycle](https://docs.firegiant.com/wix/) and
[maintenance terms](https://docs.firegiant.com/wix/osmf/).

## Linux

Both workflows package the same Linux application. The ZIP has root
`edhm-ui-v3-linux-x64/`, and `linux_installer.sh` uses Unix line endings. The active
Forge configuration includes both renderers and uses the executable name expected
by the existing shell script. Download both Linux assets into the same directory.

## Validation and remaining manual checks

Release tests cover exact shader matching, pagination, note rendering, lookup failures,
publication permissions/destination, immutable published versions, commit/tag mismatch,
and interrupted uploads. Windows builds also run the available app regression tests.

Free installer CI checks the real, checksum-pinned upstream 3.0.70 installer, migration,
rollback after a fault-injected upgrade, same-version rebuilds, payload hashes, repair,
fresh installs, shortcuts, uninstall, and user-file preservation. The fixture downgrade
check runs only when the new app version is newer than 3.0.70. A separate test executes
the actual Finish action with a harmless probe under a spaced/Unicode path.

Velopack CI checks native launch arguments/cancellation/error handling, all four real
SDK hooks, installed payload hashes, registration, shortcut targets, repeat installation,
and uninstall. Hook tests verify that no app settings are created. Those four hooks
also passed locally without elevation where direct Electron hooks failed with error 740.

Earlier fork runs using the separate v22.02 application branch passed both platform
builds and all installer checks: [free](https://github.com/Fenris159/EDHM_UI/actions/runs/34001691787)
and [Velopack](https://github.com/Fenris159/EDHM_UI/actions/runs/34001691802).
Those runs included the app branch's additional regression tests; this PR carries
only the packaging-related shortcut regression, alongside the release/native checks.

Hosted CI cannot verify the visible Windows 11 secure-desktop consent/cancel experience.
Before the first Velopack publication, test it and an upgrade using the maintainer's
actual previous installer. Live Linux installation and Windows files-in-use dialogs
also remain manual checks. No Elite Dangerous installation is needed or modified by CI.
