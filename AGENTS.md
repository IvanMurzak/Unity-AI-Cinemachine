# AGENTS.md — Unity-AI-Cinemachine

Unity package `com.ivanmurzak.unity.mcp.cinemachine` — wraps **Cinemachine** (`com.unity.cinemachine`) as
`cinemachine-*` MCP tools: create cameras, set follow/look-at targets, configure pipeline components (body /
aim / noise / lens), manage blends, and modify arbitrary Cinemachine component fields. An extension of
[AI Game Developer](https://github.com/IvanMurzak/Unity-MCP) (`com.ivanmurzak.unity.mcp`), generated from
`Unity-AI-Tools-Template`; the user-facing tool list is in `README.md`.

## Layout

- `Unity-Package/` — Unity project hosting the package; only `Packages/com.ivanmurzak.unity.mcp.cinemachine/`
  ships.
  - `package.json` — the version source of truth and the dependency pins.
  - `Editor/Scripts/Tools/` — the MCP tools (`Tool_Cinemachine`); new tools go here.
  - `Runtime/` — runtime assembly (currently empty).
  - `Tests/Editor/` (EditMode, `BaseTest` + one file per tool group) and `Tests/Runtime/` (PlayMode).
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

- **Main thread only.** Every Unity API call inside a tool MUST be wrapped in
  `MainThread.Instance.Run(() => { ... })` — MCP calls arrive off the main thread; only argument validation
  runs outside. So must ReflectorNet calls (`reflector.Serialize`, `TryModify`).
- **Tool shape.** One `[AiToolType] partial class Tool_Cinemachine`, split one operation per file
  (`Cinemachine.CameraCreate.cs`, `Cinemachine.SetBody.cs`, `Cinemachine.Modify.cs`, …). Each tool method
  carries `[AiTool(<id>, Title=…, …Hint=…)]`, `[AiSkillDescription]` / `[AiSkillBody]` and `[Description]`
  (all LLM-facing); tool IDs are `public const string …ToolId = "cinemachine-…"`. Shared helpers
  (`ResolveGameObject`, `ResolveCinemachineCamera`, `MarkDirtyAndRepaint`) live in `Cinemachine.Helpers.cs`;
  error strings in `Tool_Cinemachine.Error` (`Cinemachine.cs`).
- **EntityId split.** Unity 6.5+ returns `UnityEngine.EntityId` from `GameObject.GetEntityId()`; pre-6.5
  returns `int` from `GetInstanceID()`. Files surfacing an instance ID ship as a `*.cs`
  (`#if UNITY_6000_5_OR_NEWER`) + `*.pre-Unity.6.5.cs` (`#if !UNITY_6000_5_OR_NEWER`) pair — e.g.
  `Cinemachine.CameraCreate.cs` / `Cinemachine.CameraCreate.pre-Unity.6.5.cs`. Keep both variants in sync.
- **Generic modify via ReflectorNet.** `cinemachine-modify` applies a `SerializedMember` diff through
  `reflector.TryModify(ref boxed, data, …)`. The `fields` channel resolves `FieldInfo` and `props` resolves
  `PropertyInfo`, with **no cross-fallback**: a public field (e.g. `CinemachineFollow.FollowOffset`) MUST go
  in `fields`, a property in `props` (otherwise: `Property '…' not found or not writable`). It mirrors the
  base `gameobject-component-modify` tool.
- Every C# file starts with the copyright box header and `#nullable enable`.
