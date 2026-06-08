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
using Unity.Cinemachine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Cinemachine
    {
        public const string CinemachineSetPriorityToolId = "cinemachine-set-priority";

        [AiTool
        (
            CinemachineSetPriorityToolId,
            Title = "Cinemachine / Set Priority",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Set the `Priority.Value` of a `CinemachineCamera`. The highest-priority active camera " +
            "wins control of the `CinemachineBrain`.")]
        [AiSkillBody("Set the priority of a `CinemachineCamera`. When multiple CinemachineCameras are active, the " +
            "one with the highest priority becomes live (drives the rendered view through the `CinemachineBrain`).\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `CinemachineCamera` (required).\n" +
            "- `priority` — the new integer priority value.\n\n" +
            "## Behavior\n\n" +
            "Assigns `CinemachineCamera.Priority` with the given value, marks the scene dirty, and repaints the " +
            "Editor. The whole call runs on the Unity main thread.")]
        [Description("Sets the Priority value of a CinemachineCamera. Higher priority wins control of the CinemachineBrain.")]
        public SetPriorityResponse SetPriority
        (
            [Description("Reference to the GameObject containing the CinemachineCamera component.")]
            GameObjectRef gameObjectRef,
            [Description("The new priority value. Higher wins when multiple cameras are active.")]
            int priority
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var cam = ResolveCinemachineCamera(gameObjectRef, nameof(gameObjectRef));

                cam.Priority = new PrioritySettings { Value = priority };

                MarkDirtyAndRepaint(cam, cam.gameObject.scene);

                return new SetPriorityResponse
                {
                    gameObjectRef = new GameObjectRef(cam.gameObject),
                    cameraRef = new ComponentRef(cam),
                    priority = cam.Priority.Value,
                    success = true
                };
            });
        }

        public class SetPriorityResponse
        {
            [Description("Reference to the CinemachineCamera GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the CinemachineCamera component.")]
            public ComponentRef? cameraRef;

            [Description("Resulting priority value.")]
            public int priority;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
