# CLAUDE.md

## What this is

Unity package `com.ivanmurzak.unity.mcp.cinemachine` — wraps **Cinemachine 3.1.6**
(`com.unity.cinemachine`) and exposes 14 `cinemachine-*` MCP tools so AI assistants can
create cameras, set follow/look-at targets, configure pipeline components (body / aim /
noise / lens), manage blends, and modify arbitrary Cinemachine component fields. Built on
top of [Unity-MCP](https://github.com/IvanMurzak/Unity-MCP) (`com.ivanmurzak.unity.mcp`).

## Build / run

- Package source: `Unity-Package/Packages/com.ivanmurzak.unity.mcp.cinemachine/` (only this folder ships; Editor tools under `Editor/Scripts/Tools/`).
- Version source of truth: `Unity-Package/Packages/com.ivanmurzak.unity.mcp.cinemachine/package.json`. Bump with `.\commands\bump-version.ps1 -NewVersion "x.y.z"` (`-WhatIf` to preview).
- Update Unity-MCP dependency: `.\commands\update-ai-game-developer.ps1` (`-WhatIf` to preview).
- Multi-version test rigs: `Unity-Tests/{2022.3.62f3,2023.2.22f1,6000.3.1f1}`. Tests run inside the Unity Editor (NUnit + `[UnityTest]`); CI uses `game-ci/unity-test-runner@v4`. Releases trigger on push to `main` when the version tag is new.

## Critical invariants

- **Main thread only.** Every Unity API call inside a tool method MUST be wrapped in `MainThread.Instance.Run(() => { ... })` — MCP calls arrive off the main thread. ReflectorNet calls (`reflector.Serialize`, `TryModify`) touch Unity objects and must not run off the main thread.
- **Tool attributes.** The tool host is one `partial class Tool_Cinemachine` decorated `[AiToolType]`, split one-op-per-file (`Cinemachine.CameraCreate.cs`, `Cinemachine.SetBody.cs`, `Cinemachine.Modify.cs`, …). Each tool method is decorated `[AiTool(<id>, Title=…, …Hint=…)]` plus `[AiSkillDescription]` / `[AiSkillBody]` (LLM-facing skill copy) and a `[Description]` (parameter/return docs). Tool IDs are declared as `public const string …ToolId = "cinemachine-…"`.
- **EntityId split.** Unity 6.5+ returns `UnityEngine.EntityId` from `GameObject.GetEntityId()`; pre-6.5 returns `int` from `GetInstanceID()`. Files needing both ship as a `*.cs` (`#if UNITY_6000_5_OR_NEWER`) + `*.pre-Unity.6.5.cs` (`#if !UNITY_6000_5_OR_NEWER`) pair — e.g. `Cinemachine.CameraCreate.cs` / `Cinemachine.CameraCreate.pre-Unity.6.5.cs`. Keep both variants in sync when editing.
- **Generic modify via ReflectorNet.** `cinemachine-modify` applies a `SerializedMember` diff through `reflector.TryModify(ref boxed, data, …)` (mirrors the base `gameobject-component-modify` tool). ReflectorNet resolves the `fields` channel as `FieldInfo` and the `props` channel as `PropertyInfo` with **no cross-fallback** — a public field (e.g. `CinemachineFollow.FollowOffset`) MUST go in `fields`; a property (e.g. `transform.position`) MUST go in `props`. Putting a field under `props` fails with `Property '…' not found or not writable`.

## Find detail in

- `docs/claude/architecture.md` — repo layout, MCP tool pattern, ReflectorNet usage, assembly defs
- `docs/claude/release.md` — `bump-version.ps1` mechanics and the files it touches
- `docs/claude/ci.md` — release / test workflows, required secrets, Unity version matrix
- `README.md` — user-facing setup walkthrough and the full `cinemachine-*` tool list
