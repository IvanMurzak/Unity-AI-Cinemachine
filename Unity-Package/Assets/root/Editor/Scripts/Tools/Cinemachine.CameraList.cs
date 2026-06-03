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
using com.IvanMurzak.ReflectorNet.Utils;
using AIGD;
using UnityEngine;
using Unity.Cinemachine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Cinemachine
    {
        public const string CinemachineCameraListToolId = "cinemachine-camera-list";

        [AiTool
        (
            CinemachineCameraListToolId,
            Title = "Cinemachine / List Cameras",
            ReadOnlyHint = true,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("List every `CinemachineCamera` in the active scene with its name, priority, and " +
            "whether it is currently the live (active) camera according to an active `CinemachineBrain`. Read-only.")]
        [AiSkillBody("List every `CinemachineCamera` in the active scene. For each, returns the name, the priority " +
            "value, a reference, and whether the camera is currently live (driving the view through an active " +
            "`CinemachineBrain`).\n\n" +
            "## Inputs\n\n" +
            "_(none)_\n\n" +
            "## Behavior\n\n" +
            "Finds all `CinemachineCamera` instances (including inactive ones), reads each `Priority.Value`, and " +
            "compares against `CinemachineBrain.ActiveVirtualCamera` across all active brains to compute `isLive`. " +
            "Read-only. The whole call runs on the Unity main thread.")]
        [Description("Lists all CinemachineCameras in the active scene with name, priority and isLive. Read-only.")]
        public CameraListResponse ListCameras()
        {
            return MainThread.Instance.Run(() =>
            {
#if UNITY_2023_1_OR_NEWER
                var cameras = UnityEngine.Object.FindObjectsByType<CinemachineCamera>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
                var cameras = UnityEngine.Object.FindObjectsOfType<CinemachineCamera>(includeInactive: true);
#endif

                // Collect the set of live virtual cameras across all active brains.
                var liveCameras = new HashSet<UnityEngine.Object>();
                for (int i = 0; i < CinemachineBrain.ActiveBrainCount; i++)
                {
                    var brain = CinemachineBrain.GetActiveBrain(i);
                    var active = brain != null ? brain.ActiveVirtualCamera : null;
                    if (active is UnityEngine.Object activeObj && activeObj != null)
                        liveCameras.Add(activeObj);
                }

                var items = new List<CameraListItem>(cameras.Length);
                foreach (var cam in cameras)
                {
                    if (cam == null)
                        continue;
                    items.Add(new CameraListItem
                    {
                        gameObjectRef = new GameObjectRef(cam.gameObject),
                        cameraRef = new ComponentRef(cam),
                        name = cam.name,
                        priority = cam.Priority.Value,
                        isLive = liveCameras.Contains(cam)
                    });
                }

                return new CameraListResponse
                {
                    count = items.Count,
                    cameras = items.ToArray()
                };
            });
        }

        public class CameraListResponse
        {
            [Description("Number of CinemachineCameras found.")]
            public int count;

            [Description("The CinemachineCameras in the active scene.")]
            public CameraListItem[] cameras = Array.Empty<CameraListItem>();
        }

        public class CameraListItem
        {
            [Description("Reference to the CinemachineCamera GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the CinemachineCamera component.")]
            public ComponentRef? cameraRef;

            [Description("Name of the camera GameObject.")]
            public string name = string.Empty;

            [Description("Priority value of the camera.")]
            public int priority;

            [Description("Whether this camera is currently live (active through a CinemachineBrain).")]
            public bool isLive;
        }
    }
}
