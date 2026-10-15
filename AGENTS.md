# AGENTS.md — Unity-AI-Cinemachine

Unity package `com.ivanmurzak.unity.mcp.cinemachine` — **Cinemachine** (`com.unity.cinemachine`) for AI agents. On
branch `experiment/rql` (local experiment) it ships no MCP tools: an RQL module for Unity-MCP's `rql-query` /
`rql-mutate` (the `cinemachine` source with `CreateCamera` / `EnsureBrain`, CinemachineCamera `SetBody` / `SetAim` /
`SetNoise` / `SetTargets`, pipeline members `body` / `aim` / `noise` / `extensions`, and a guide rendered into the
`unity-rql` skill). An extension of [AI Game Developer](https://github.com/IvanMurzak/Unity-MCP)
(`com.ivanmurzak.unity.mcp`), generated from `Unity-AI-Tools-Template`; the former tool → RQL table is in `README.md`.

## Layout

- `Unity-Package/` — Unity project hosting the package; only `Packages/com.ivanmurzak.unity.mcp.cinemachine/`
  ships.
  - `package.json` — the version source of truth and the dependency pins.
  - `Editor/Scripts/Rql/` — the RQL module (`CinemachineModule`), the `cinemachine` source (`CinemachineApi`), pipeline
    helpers (`CinemachinePipeline`), one adapter per file.
  - `Runtime/` — runtime assembly (currently empty).
  - `Tests/Editor/` (EditMode; `Rql/`: `CinemachineTestBase` + one file per adapter / source) and `Tests/Runtime/` (PlayMode).
- `Unity-Tests/<unity-version>/` — one test project per supported Unity version, consuming the package via
  `file:`. A new version must also be added to `test_pull_request.yml` and `release.yml` (and
  `open-all-projects-*`).
- `Installer/` — Unity project that exports the `.unitypackage` installer
  (`Assets/com.IvanMurzak/<name> Installer/`).
- `commands/` — PowerShell helpers (below) and `check-versions.py` (synced in by the release cascade — do not
  hand-edit; CI runs it as the `version-consistency` gate).
- `.github/workflows/` — `test_pull_request.yml` (PR test matrix), `test_unity_plugin.yml` (reusable test
  job), `release.yml`, `bump_version.yml`.
- `docs/` — user docs: `Deploy-{OpenUPM,GitHub,npmjs}.md`, `Manual-Package-Rename.md`, `openupm-signing.md`,
  `img/`, `promo/`.

## Commands (PowerShell, from the repo root)

```powershell
./commands/bump-version.ps1 -NewVersion "x.y.z"   # package.json + Installer.cs in lock-step; -WhatIf to preview
./commands/get-version.ps1
./commands/update-ai-game-developer.ps1           # bump the com.ivanmurzak.unity.mcp pin; -WhatIf to preview
./commands/open-all-projects-windows.ps1          # open all Unity projects (or -unix.sh)
```

Tests run inside the Unity Editor (NUnit + `[UnityTest]`); CI runs them with `game-ci/unity-test-runner` in
EditMode / PlayMode / Standalone for every Unity version. `release.yml` releases on push to `main` only when
the `package.json` version has no tag yet (installer, signed UPM tarball for OpenUPM, test matrix, tag, GitHub
Release); dispatch `bump_version.yml` to bump in CI. Under `pull_request_target`, tests need the
maintainer-applied `ci-ok` label and abort if the PR touches `.github/workflows/`.

## Critical invariants

- **RQL surface (branch `experiment/rql`).** Agents use Unity-MCP's `rql-query` / `rql-mutate`; per-camera operations are
  adapter methods of CinemachineCamera, owner-less creation lives on the `cinemachine` source (`[RqlCallable]`), and every
  example belongs in the module guide (`CinemachineModule.Guide`, rendered into the `unity-rql` skill;
  `EveryGuideExampleRuns` executes it in order).
- **Pipeline stages.** Body / Aim / Noise component types come from `[CameraPipeline(stage)]` via TypeCache, excluding
  `[Obsolete]` CM2 components; `SetBody` / `SetAim` remove every other component of that stage. Components are added /
  removed with `Undo.AddComponent` / `Undo.DestroyObjectImmediate` so one Ctrl+Z reverts a call.
- **Unity fake null.** Never `??` / `?.` on `UnityEngine.Object` results (`GetComponent<T>() ?? throw` throws
  MissingComponentException); compare with `== null`.
- **Hidden internals.** Private `[SerializeField, HideInInspector]` bookkeeping (m_StreamingVersion, m_LegacyPriority,
  m_NoiseOffsets) and runtime caches (*TargetAsGroup / *TargetAsVcam …) are hidden; CameraState members are explicit-only.
- Every C# file starts with the copyright box header and `#nullable enable`.
