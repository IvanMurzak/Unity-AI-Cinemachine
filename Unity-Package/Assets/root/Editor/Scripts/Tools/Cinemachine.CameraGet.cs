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
using System.Collections.Generic;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Utils;
using AIGD;
using com.IvanMurzak.Unity.MCP.Utils;
using Unity.Cinemachine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Cinemachine
    {
        public const string CinemachineCameraGetToolId = "cinemachine-camera-get";

        [AiTool
        (
            CinemachineCameraGetToolId,
            Title = "Cinemachine / Get Camera",
            ReadOnlyHint = true,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Inspect a `CinemachineCamera` — its lens, Follow / LookAt targets, priority, and the " +
            "list of pipeline components (Body / Aim / Noise / extensions) currently attached. Read-only.")]
        [AiSkillBody("Inspect a `CinemachineCamera` component in the active scene. Returns the priority, the " +
            "Follow and LookAt target names, the serialized `LensSettings`, and the list of pipeline / extension " +
            "components attached to the camera GameObject.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `CinemachineCamera` (required).\n" +
            "- `deepSerialization` — when `true`, the lens is serialized recursively through nested objects; " +
            "otherwise only top-level members are serialized.\n\n" +
            "## Behavior\n\n" +
            "Serializes `CinemachineCamera.Lens` via ReflectorNet and enumerates every `CinemachineComponentBase` " +
            "and `CinemachineExtension` on the GameObject by type name. Read-only — nothing is mutated. The whole " +
            "call runs on the Unity main thread.")]
        [Description("Get the full configuration of a CinemachineCamera: lens, targets, priority, and the list of " +
            "pipeline/extension components. Read-only.")]
        public CameraGetResponse GetCamera
        (
            [Description("Reference to the GameObject containing the CinemachineCamera component.")]
            GameObjectRef gameObjectRef,
            [Description("Performs deep serialization of the lens including nested objects. Otherwise only top-level members.")]
            bool deepSerialization = false
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));
            if (!gameObjectRef.IsValid(out var validationError))
                throw new ArgumentException(validationError, nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var cam = ResolveCinemachineCamera(gameObjectRef, nameof(gameObjectRef));
                var go = cam.gameObject;

                var reflector = UnityMcpPluginEditor.Instance.Reflector ?? throw new Exception(Error.ReflectorNotAvailable());
                var logger = UnityLoggerFactory.LoggerFactory.CreateLogger<Tool_Cinemachine>();

                var response = new CameraGetResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    cameraRef = new ComponentRef(cam),
                    priority = cam.Priority.Value,
                    follow = cam.Follow != null ? cam.Follow.name : null,
                    lookAt = cam.LookAt != null ? cam.LookAt.name : null,
                    lens = reflector.Serialize(
                        obj: cam.Lens,
                        name: nameof(cam.Lens),
                        recursive: deepSerialization,
                        logger: logger)
                };

                var pipeline = new List<string>();
                foreach (var component in go.GetComponents<CinemachineComponentBase>())
                    if (component != null)
                        pipeline.Add(component.GetType().Name);
                response.pipelineComponents = pipeline.ToArray();

                var extensions = new List<string>();
                foreach (var ext in go.GetComponents<CinemachineExtension>())
                    if (ext != null)
                        extensions.Add(ext.GetType().Name);
                response.extensions = extensions.ToArray();

                return response;
            });
        }

        public class CameraGetResponse
        {
            [Description("Reference to the CinemachineCamera GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the CinemachineCamera component.")]
            public ComponentRef? cameraRef;

            [Description("Priority value of the camera.")]
            public int priority;

            [Description("Name of the Follow target, or null.")]
            public string? follow;

            [Description("Name of the LookAt target, or null.")]
            public string? lookAt;

            [Description("Serialized LensSettings of the camera.")]
            public SerializedMember? lens;

            [Description("Type names of the CinemachineComponentBase pipeline components (Body/Aim/Noise) attached.")]
            public string[] pipelineComponents = Array.Empty<string>();

            [Description("Type names of the CinemachineExtension components attached.")]
            public string[] extensions = Array.Empty<string>();
        }
    }
}
