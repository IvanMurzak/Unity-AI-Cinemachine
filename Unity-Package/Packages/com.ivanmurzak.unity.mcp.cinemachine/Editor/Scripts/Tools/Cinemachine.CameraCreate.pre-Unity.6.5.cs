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
#if !UNITY_6000_5_OR_NEWER
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using AIGD;
using UnityEditor;
using UnityEngine;
using Unity.Cinemachine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Cinemachine
    {
        public const string CinemachineCameraCreateToolId = "cinemachine-camera-create";

        [AiTool
        (
            CinemachineCameraCreateToolId,
            Title = "Cinemachine / Create Camera",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Create a new GameObject with a `CinemachineCamera` (Cinemachine 3.x virtual camera) " +
            "in the active scene. Optionally set name, position, rotation, priority, and Follow / LookAt targets. " +
            "Returns the new GameObject reference and instanceId.")]
        [AiSkillBody("Create a new GameObject hosting a `CinemachineCamera` component in the active scene. The " +
            "`CinemachineCamera` is the Cinemachine 3.x virtual camera that a `CinemachineBrain` blends between to " +
            "drive the real Unity Camera.\n\n" +
            "## Inputs\n\n" +
            "- `name` — optional GameObject name (default `CinemachineCamera`).\n" +
            "- `position` — optional world `Vector3` position (default zero).\n" +
            "- `rotation` — optional euler-degrees `Vector3` rotation (default zero).\n" +
            "- `priority` — optional priority value; higher wins when multiple cameras compete (default 0).\n" +
            "- `followRef` — optional GameObject the camera should Follow (position target).\n" +
            "- `lookAtRef` — optional GameObject the camera should LookAt (aim target).\n\n" +
            "## Behavior\n\n" +
            "Creates the GameObject, adds a `CinemachineCamera`, sets transform / `Priority.Value` / `Follow` / " +
            "`LookAt`, marks the scene dirty, repaints the Editor, and returns the new GameObject reference and " +
            "instanceId. The whole call runs on the Unity main thread.")]
        [Description("Creates a new CinemachineCamera (virtual camera) GameObject in the active scene. " +
            "Optionally sets name, transform, priority, and Follow/LookAt targets.")]
        public CameraCreateResponse CreateCamera
        (
            [Description("Name of the new CinemachineCamera GameObject.")]
            string? name = null,
            [Description("World-space position of the camera.")]
            Vector3? position = null,
            [Description("World-space rotation of the camera in euler angles (degrees).")]
            Vector3? rotation = null,
            [Description("Priority value. Higher priority wins when several CinemachineCameras are active.")]
            int priority = 0,
            [Description("Optional GameObject the camera should Follow (drives position).")]
            GameObjectRef? followRef = null,
            [Description("Optional GameObject the camera should LookAt (drives aim).")]
            GameObjectRef? lookAtRef = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                Transform? followTarget = ResolveOptionalTransform(followRef, nameof(followRef));
                Transform? lookAtTarget = ResolveOptionalTransform(lookAtRef, nameof(lookAtRef));

                position ??= Vector3.zero;
                rotation ??= Vector3.zero;

                var go = new GameObject(string.IsNullOrEmpty(name) ? "CinemachineCamera" : name);
                go.transform.position = position.Value;
                go.transform.eulerAngles = rotation.Value;

                var cam = go.AddComponent<CinemachineCamera>();
                cam.Priority = new PrioritySettings { Value = priority };
                if (followTarget != null)
                    cam.Follow = followTarget;
                if (lookAtTarget != null)
                    cam.LookAt = lookAtTarget;

                EditorUtility.SetDirty(go);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
                EditorUtils.RepaintAllEditorWindows();

                return new CameraCreateResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    cameraRef = new ComponentRef(cam),
                    instanceId = go.GetInstanceID(),
                    gameObjectName = go.name,
                    priority = cam.Priority.Value,
                    follow = followTarget != null ? followTarget.name : null,
                    lookAt = lookAtTarget != null ? lookAtTarget.name : null
                };
            });
        }

        public class CameraCreateResponse
        {
            [Description("Reference to the created CinemachineCamera GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the created CinemachineCamera component.")]
            public ComponentRef? cameraRef;

            [Description("Instance id of the created GameObject.")]
            public int instanceId;

            [Description("Name of the created GameObject.")]
            public string gameObjectName = string.Empty;

            [Description("Resolved priority value.")]
            public int priority;

            [Description("Name of the Follow target, or null.")]
            public string? follow;

            [Description("Name of the LookAt target, or null.")]
            public string? lookAt;
        }
    }
}
#endif
