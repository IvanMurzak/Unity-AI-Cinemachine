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
        public const string CinemachineSetNoiseToolId = "cinemachine-set-noise";

        [AiTool
        (
            CinemachineSetNoiseToolId,
            Title = "Cinemachine / Set Noise",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Add or configure a `CinemachineBasicMultiChannelPerlin` (procedural camera shake / " +
            "handheld noise) on a `CinemachineCamera`. Sets `AmplitudeGain` and `FrequencyGain`. The noise profile " +
            "asset is optional and left untouched when not supplied.")]
        [AiSkillBody("Add or configure a `CinemachineBasicMultiChannelPerlin` noise component on a " +
            "`CinemachineCamera`. This drives procedural Perlin-noise shake (handheld camera feel). The actual noise " +
            "shape comes from a `NoiseSettings` profile asset; without one, the amplitude/frequency gains have no " +
            "visible effect, so this tool is null-safe and does not require a profile.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `CinemachineCamera` (required).\n" +
            "- `amplitudeGain` — optional gain applied to the profile amplitudes (1 = normal).\n" +
            "- `frequencyGain` — optional gain applied to the profile frequencies (1 = normal).\n\n" +
            "## Behavior\n\n" +
            "Reuses an existing `CinemachineBasicMultiChannelPerlin` on the GameObject or adds one, then applies the " +
            "provided gains. The noise profile asset is never modified here (leave it null-safe). Marks the scene " +
            "dirty and repaints. Runs on the Unity main thread.")]
        [Description("Adds/configures a CinemachineBasicMultiChannelPerlin (camera shake) on a CinemachineCamera. " +
            "Sets amplitudeGain/frequencyGain. Noise profile asset is optional.")]
        public SetNoiseResponse SetNoise
        (
            [Description("Reference to the GameObject containing the CinemachineCamera component.")]
            GameObjectRef gameObjectRef,
            [Description("Gain applied to the noise profile amplitudes (1 = normal).")]
            float? amplitudeGain = null,
            [Description("Gain applied to the noise profile frequencies (1 = normal).")]
            float? frequencyGain = null
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var cam = ResolveCinemachineCamera(gameObjectRef, nameof(gameObjectRef));
                var go = cam.gameObject;

                var noise = go.GetComponent<CinemachineBasicMultiChannelPerlin>();
                if (noise == null)
                    noise = go.AddComponent<CinemachineBasicMultiChannelPerlin>();

                if (amplitudeGain.HasValue) noise.AmplitudeGain = amplitudeGain.Value;
                if (frequencyGain.HasValue) noise.FrequencyGain = frequencyGain.Value;

                MarkDirtyAndRepaint(cam, go.scene);

                return new SetNoiseResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    cameraRef = new ComponentRef(cam),
                    amplitudeGain = noise.AmplitudeGain,
                    frequencyGain = noise.FrequencyGain,
                    hasNoiseProfile = noise.NoiseProfile != null,
                    success = true
                };
            });
        }

        public class SetNoiseResponse
        {
            [Description("Reference to the CinemachineCamera GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the CinemachineCamera component.")]
            public ComponentRef? cameraRef;

            [Description("Resulting amplitude gain.")]
            public float amplitudeGain;

            [Description("Resulting frequency gain.")]
            public float frequencyGain;

            [Description("Whether a NoiseSettings profile asset is assigned.")]
            public bool hasNoiseProfile;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
