/*
┌─────────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)                        │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-AI-Cinemachine)    │
│  Copyright (c) 2025 Ivan Murzak                                             │
│  Licensed under the MIT License.                                            │
│  See the LICENSE file in the project root for more information.             │
└─────────────────────────────────────────────────────────────────────────────┘
*/

#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using AIGD;
using UnityEngine;
using Unity.Cinemachine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Cinemachine
    {
        public const string CinemachineSetAimToolId = "cinemachine-set-aim";

        [AiTool
        (
            CinemachineSetAimToolId,
            Title = "Cinemachine / Set Aim",
            ReadOnlyHint = false,
            DestructiveHint = true,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Add or replace the rotation-control (Aim) component of a `CinemachineCamera`, chosen by " +
            "`AimType` (RotationComposer, HardLookAt, PanTilt, or None). Applies common params (screen X/Y, damping) " +
            "to RotationComposer where applicable.")]
        [AiSkillBody("Add or replace the Aim (rotation-control) component of a `CinemachineCamera`. Any existing Aim " +
            "component is removed first, then the chosen one is added — so this is destructive to the previous Aim " +
            "component.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `CinemachineCamera` (required).\n" +
            "- `aimType` — `AimType` enum: `RotationComposer`, `HardLookAt`, `PanTilt`, or `None` (removes the Aim " +
            "component).\n" +
            "- `screenX`, `screenY` — optional normalized on-screen target position for `RotationComposer` " +
            "(`Composition.ScreenPosition`). Typically in the range -0.5..0.5 (0,0 = center).\n" +
            "- `damping` — optional uniform damping value (applies to `RotationComposer.Damping`).\n\n" +
            "## Behavior\n\n" +
            "Removes every existing `CinemachineRotationComposer`, `CinemachineHardLookAt`, and `CinemachinePanTilt` " +
            "on the GameObject, then adds the requested one and applies the optional params. Marks the scene dirty " +
            "and repaints. Runs on the Unity main thread.")]
        [Description("Adds/replaces the Aim (rotation-control) component of a CinemachineCamera by AimType. " +
            "Applies screen X/Y and damping to RotationComposer where applicable.")]
        public SetAimResponse SetAim
        (
            [Description("Reference to the GameObject containing the CinemachineCamera component.")]
            GameObjectRef gameObjectRef,
            [Description("Which rotation-control (Aim) component to use.")]
            AimType aimType,
            [Description("Normalized on-screen X target position for RotationComposer (Composition.ScreenPosition.x).")]
            float? screenX = null,
            [Description("Normalized on-screen Y target position for RotationComposer (Composition.ScreenPosition.y).")]
            float? screenY = null,
            [Description("Uniform damping value applied where the component supports damping (RotationComposer).")]
            float? damping = null
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var cam = ResolveCinemachineCamera(gameObjectRef, nameof(gameObjectRef));
                var go = cam.gameObject;

                RemoveComponents<CinemachineRotationComposer>(go);
                RemoveComponents<CinemachineHardLookAt>(go);
                RemoveComponents<CinemachinePanTilt>(go);

                string componentName = "None";
                switch (aimType)
                {
                    case AimType.None:
                        break;

                    case AimType.RotationComposer:
                    {
                        var c = go.AddComponent<CinemachineRotationComposer>();
                        if (screenX.HasValue || screenY.HasValue)
                        {
                            var comp = c.Composition;
                            var sp = comp.ScreenPosition;
                            if (screenX.HasValue) sp.x = screenX.Value;
                            if (screenY.HasValue) sp.y = screenY.Value;
                            comp.ScreenPosition = sp;
                            c.Composition = comp;
                        }
                        if (damping.HasValue) c.Damping = Vector2.one * damping.Value;
                        componentName = nameof(CinemachineRotationComposer);
                        break;
                    }

                    case AimType.HardLookAt:
                    {
                        go.AddComponent<CinemachineHardLookAt>();
                        componentName = nameof(CinemachineHardLookAt);
                        break;
                    }

                    case AimType.PanTilt:
                    {
                        go.AddComponent<CinemachinePanTilt>();
                        componentName = nameof(CinemachinePanTilt);
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException(nameof(aimType), aimType, "Unsupported aim type.");
                }

                MarkDirtyAndRepaint(cam, go.scene);

                return new SetAimResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    cameraRef = new ComponentRef(cam),
                    aimType = aimType.ToString(),
                    componentName = componentName,
                    success = true
                };
            });
        }

        public class SetAimResponse
        {
            [Description("Reference to the CinemachineCamera GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the CinemachineCamera component.")]
            public ComponentRef? cameraRef;

            [Description("The AimType that was applied.")]
            public string aimType = string.Empty;

            [Description("The Cinemachine component type name that was added (or 'None').")]
            public string componentName = string.Empty;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
