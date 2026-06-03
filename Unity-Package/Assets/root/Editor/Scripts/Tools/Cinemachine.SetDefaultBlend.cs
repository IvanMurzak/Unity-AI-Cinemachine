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
        public const string CinemachineSetDefaultBlendToolId = "cinemachine-set-default-blend";

        [AiTool
        (
            CinemachineSetDefaultBlendToolId,
            Title = "Cinemachine / Set Default Blend",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Set the `DefaultBlend` of a `CinemachineBrain` — the blend style (Cut, EaseInOut, " +
            "Linear, etc.) and duration used when transitioning between CinemachineCameras. Targets the referenced " +
            "Camera's brain, or `Camera.main`'s brain.")]
        [AiSkillBody("Set the default blend on a `CinemachineBrain`. The default blend is used whenever the brain " +
            "transitions from one live CinemachineCamera to another and no custom blend overrides it.\n\n" +
            "## Inputs\n\n" +
            "- `cameraRef` — optional GameObject hosting the Camera + `CinemachineBrain`. When omitted, " +
            "`Camera.main` is used.\n" +
            "- `style` — `CinemachineBlendDefinition.Styles` enum (`Cut`, `EaseInOut`, `EaseIn`, `EaseOut`, " +
            "`HardIn`, `HardOut`, `Linear`, `Custom`).\n" +
            "- `time` — blend duration in seconds.\n\n" +
            "## Behavior\n\n" +
            "Resolves the brain (throws if the Camera has none), assigns a new `CinemachineBlendDefinition(style, " +
            "time)` to `DefaultBlend`, marks the scene dirty, and repaints. Runs on the Unity main thread.")]
        [Description("Sets the CinemachineBrain DefaultBlend (style + time in seconds). Targets the given Camera's brain or Camera.main.")]
        public SetDefaultBlendResponse SetDefaultBlend
        (
            [Description("Blend style used when transitioning between CinemachineCameras.")]
            CinemachineBlendDefinition.Styles style,
            [Description("Blend duration in seconds.")]
            float time = 2f,
            [Description("Optional reference to the GameObject hosting the Camera + CinemachineBrain. If omitted, Camera.main is used.")]
            GameObjectRef? cameraRef = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                Camera? camera;
                if (cameraRef != null && cameraRef.IsValid(out _))
                {
                    var go = ResolveGameObject(cameraRef, nameof(cameraRef));
                    camera = go.GetComponent<Camera>();
                    if (camera == null)
                        throw new Exception($"[Error] GameObject '{go.name}' has no Camera component.");
                }
                else
                {
                    camera = Camera.main;
                    if (camera == null)
                        throw new Exception(Error.NoMainCamera());
                }

                var brain = camera.GetComponent<CinemachineBrain>();
                if (brain == null)
                    throw new Exception(Error.CinemachineBrainNotFound());

                brain.DefaultBlend = new CinemachineBlendDefinition(style, time);

                MarkDirtyAndRepaint(brain, brain.gameObject.scene);

                return new SetDefaultBlendResponse
                {
                    cameraRef = new GameObjectRef(brain.gameObject),
                    brainRef = new ComponentRef(brain),
                    style = brain.DefaultBlend.Style.ToString(),
                    time = brain.DefaultBlend.Time,
                    success = true
                };
            });
        }

        public class SetDefaultBlendResponse
        {
            [Description("Reference to the Camera GameObject hosting the CinemachineBrain.")]
            public GameObjectRef? cameraRef;

            [Description("Reference to the CinemachineBrain component.")]
            public ComponentRef? brainRef;

            [Description("Resulting blend style.")]
            public string style = string.Empty;

            [Description("Resulting blend time (seconds).")]
            public float time;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
