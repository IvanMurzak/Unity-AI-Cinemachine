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
        public const string CinemachineSetTargetsToolId = "cinemachine-set-targets";

        [AiTool
        (
            CinemachineSetTargetsToolId,
            Title = "Cinemachine / Set Targets",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Set the `Follow` (position) and/or `LookAt` (aim) targets of a `CinemachineCamera`. " +
            "Pass `clearFollow` / `clearLookAt` to explicitly remove a target instead of setting one.")]
        [AiSkillBody("Set the `Follow` and/or `LookAt` targets of a `CinemachineCamera`. The Follow target drives " +
            "the position-control (Body) component; the LookAt target drives the rotation-control (Aim) component.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `CinemachineCamera` (required).\n" +
            "- `followRef` — optional GameObject to set as the Follow target.\n" +
            "- `lookAtRef` — optional GameObject to set as the LookAt target.\n" +
            "- `clearFollow` — when `true`, sets Follow to null (ignored if `followRef` is provided).\n" +
            "- `clearLookAt` — when `true`, sets LookAt to null (ignored if `lookAtRef` is provided).\n\n" +
            "## Behavior\n\n" +
            "Only the targets you specify are changed; omitted targets are left untouched unless the matching " +
            "`clear*` flag is set. Marks the scene dirty and repaints the Editor. The whole call runs on the Unity " +
            "main thread.")]
        [Description("Sets the Follow and/or LookAt targets of a CinemachineCamera. Use clearFollow/clearLookAt to remove a target.")]
        public SetTargetsResponse SetTargets
        (
            [Description("Reference to the GameObject containing the CinemachineCamera component.")]
            GameObjectRef gameObjectRef,
            [Description("Optional GameObject to set as the Follow (position) target.")]
            GameObjectRef? followRef = null,
            [Description("Optional GameObject to set as the LookAt (aim) target.")]
            GameObjectRef? lookAtRef = null,
            [Description("If true, clears the Follow target (ignored when followRef is provided).")]
            bool clearFollow = false,
            [Description("If true, clears the LookAt target (ignored when lookAtRef is provided).")]
            bool clearLookAt = false
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var cam = ResolveCinemachineCamera(gameObjectRef, nameof(gameObjectRef));

                var followTarget = ResolveOptionalTransform(followRef, nameof(followRef));
                var lookAtTarget = ResolveOptionalTransform(lookAtRef, nameof(lookAtRef));

                if (followTarget != null)
                    cam.Follow = followTarget;
                else if (clearFollow)
                    cam.Follow = null;

                if (lookAtTarget != null)
                    cam.LookAt = lookAtTarget;
                else if (clearLookAt)
                    cam.LookAt = null;

                MarkDirtyAndRepaint(cam, cam.gameObject.scene);

                return new SetTargetsResponse
                {
                    gameObjectRef = new GameObjectRef(cam.gameObject),
                    cameraRef = new ComponentRef(cam),
                    follow = cam.Follow != null ? cam.Follow.name : null,
                    lookAt = cam.LookAt != null ? cam.LookAt.name : null,
                    success = true
                };
            });
        }

        public class SetTargetsResponse
        {
            [Description("Reference to the CinemachineCamera GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the CinemachineCamera component.")]
            public ComponentRef? cameraRef;

            [Description("Name of the resulting Follow target, or null.")]
            public string? follow;

            [Description("Name of the resulting LookAt target, or null.")]
            public string? lookAt;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
