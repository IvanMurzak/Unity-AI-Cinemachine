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
        public const string CinemachineSetBodyToolId = "cinemachine-set-body";

        [AiTool
        (
            CinemachineSetBodyToolId,
            Title = "Cinemachine / Set Body",
            ReadOnlyHint = false,
            DestructiveHint = true,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Add or replace the position-control (Body) component of a `CinemachineCamera`, chosen " +
            "by `BodyType` (Follow, OrbitalFollow, ThirdPersonFollow, PositionComposer, HardLockToTarget, or None). " +
            "Applies common params (followOffset, damping, cameraDistance) where applicable.")]
        [AiSkillBody("Add or replace the Body (position-control) component of a `CinemachineCamera`. Any existing " +
            "Body component is removed first, then the chosen one is added — so this is destructive to the previous " +
            "Body component.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `CinemachineCamera` (required).\n" +
            "- `bodyType` — `BodyType` enum: `Follow`, `OrbitalFollow`, `ThirdPersonFollow`, `PositionComposer`, " +
            "`HardLockToTarget`, or `None` (removes the Body component).\n" +
            "- `followOffset` — optional `Vector3` offset from the Follow target (applies to `Follow`).\n" +
            "- `damping` — optional uniform damping value (applied where the component supports damping).\n" +
            "- `cameraDistance` — optional distance from the target (applies to `ThirdPersonFollow` / " +
            "`PositionComposer` / `OrbitalFollow` radius).\n\n" +
            "## Behavior\n\n" +
            "Removes every existing `CinemachineFollow`, `CinemachineOrbitalFollow`, `CinemachineThirdPersonFollow`, " +
            "`CinemachinePositionComposer`, and `CinemachineHardLockToTarget` on the GameObject, then adds the " +
            "requested one and applies the optional params. Marks the scene dirty and repaints. Runs on the Unity " +
            "main thread.")]
        [Description("Adds/replaces the Body (position-control) component of a CinemachineCamera by BodyType. " +
            "Applies followOffset/damping/cameraDistance where applicable.")]
        public SetBodyResponse SetBody
        (
            [Description("Reference to the GameObject containing the CinemachineCamera component.")]
            GameObjectRef gameObjectRef,
            [Description("Which position-control (Body) component to use.")]
            BodyType bodyType,
            [Description("Offset from the Follow target (applies to Follow).")]
            Vector3? followOffset = null,
            [Description("Uniform damping value applied where the component supports damping.")]
            float? damping = null,
            [Description("Distance from the target (ThirdPersonFollow/PositionComposer/OrbitalFollow radius).")]
            float? cameraDistance = null
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var cam = ResolveCinemachineCamera(gameObjectRef, nameof(gameObjectRef));
                var go = cam.gameObject;

                RemoveComponents<CinemachineFollow>(go);
                RemoveComponents<CinemachineOrbitalFollow>(go);
                RemoveComponents<CinemachineThirdPersonFollow>(go);
                RemoveComponents<CinemachinePositionComposer>(go);
                RemoveComponents<CinemachineHardLockToTarget>(go);

                string componentName = "None";
                switch (bodyType)
                {
                    case BodyType.None:
                        break;

                    case BodyType.Follow:
                    {
                        var c = go.AddComponent<CinemachineFollow>();
                        if (followOffset.HasValue) c.FollowOffset = followOffset.Value;
                        if (damping.HasValue)
                        {
                            var t = c.TrackerSettings;
                            t.PositionDamping = Vector3.one * damping.Value;
                            c.TrackerSettings = t;
                        }
                        componentName = nameof(CinemachineFollow);
                        break;
                    }

                    case BodyType.OrbitalFollow:
                    {
                        var c = go.AddComponent<CinemachineOrbitalFollow>();
                        if (cameraDistance.HasValue) c.Radius = cameraDistance.Value;
                        if (damping.HasValue)
                        {
                            var t = c.TrackerSettings;
                            t.PositionDamping = Vector3.one * damping.Value;
                            c.TrackerSettings = t;
                        }
                        componentName = nameof(CinemachineOrbitalFollow);
                        break;
                    }

                    case BodyType.ThirdPersonFollow:
                    {
                        var c = go.AddComponent<CinemachineThirdPersonFollow>();
                        if (cameraDistance.HasValue) c.CameraDistance = cameraDistance.Value;
                        if (damping.HasValue) c.Damping = Vector3.one * damping.Value;
                        componentName = nameof(CinemachineThirdPersonFollow);
                        break;
                    }

                    case BodyType.PositionComposer:
                    {
                        var c = go.AddComponent<CinemachinePositionComposer>();
                        if (cameraDistance.HasValue) c.CameraDistance = cameraDistance.Value;
                        if (damping.HasValue) c.Damping = Vector3.one * damping.Value;
                        componentName = nameof(CinemachinePositionComposer);
                        break;
                    }

                    case BodyType.HardLockToTarget:
                    {
                        var c = go.AddComponent<CinemachineHardLockToTarget>();
                        if (damping.HasValue) c.Damping = damping.Value;
                        componentName = nameof(CinemachineHardLockToTarget);
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException(nameof(bodyType), bodyType, "Unsupported body type.");
                }

                MarkDirtyAndRepaint(cam, go.scene);

                return new SetBodyResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    cameraRef = new ComponentRef(cam),
                    bodyType = bodyType.ToString(),
                    componentName = componentName,
                    success = true
                };
            });
        }

        static void RemoveComponents<T>(GameObject go) where T : UnityEngine.Component
        {
            foreach (var c in go.GetComponents<T>())
                if (c != null)
                    UnityEngine.Object.DestroyImmediate(c);
        }

        public class SetBodyResponse
        {
            [Description("Reference to the CinemachineCamera GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the CinemachineCamera component.")]
            public ComponentRef? cameraRef;

            [Description("The BodyType that was applied.")]
            public string bodyType = string.Empty;

            [Description("The Cinemachine component type name that was added (or 'None').")]
            public string componentName = string.Empty;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
