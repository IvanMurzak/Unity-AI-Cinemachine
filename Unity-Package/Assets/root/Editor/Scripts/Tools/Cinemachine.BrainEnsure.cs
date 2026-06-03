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
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using AIGD;
using UnityEditor;
using UnityEngine;
using Unity.Cinemachine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Cinemachine
    {
        public const string CinemachineBrainEnsureToolId = "cinemachine-brain-ensure";

        [AiTool
        (
            CinemachineBrainEnsureToolId,
            Title = "Cinemachine / Ensure Brain",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Ensure a `CinemachineBrain` exists on a `Camera`. Targets the referenced Camera " +
            "GameObject, or `Camera.main` when none is given. Returns the brain + camera info. A CinemachineBrain " +
            "is required for any CinemachineCamera to actually drive the rendering Camera.")]
        [AiSkillBody("Ensure a `CinemachineBrain` component exists on a `Camera`. A CinemachineBrain is the bridge " +
            "between Cinemachine virtual cameras and a real Unity `Camera`; without it, `CinemachineCamera` " +
            "components have no effect on the rendered view.\n\n" +
            "## Inputs\n\n" +
            "- `cameraRef` — optional GameObject hosting a `Camera`. When omitted, `Camera.main` (a Camera tagged " +
            "`MainCamera`) is used.\n\n" +
            "## Behavior\n\n" +
            "Resolves the Camera, adds a `CinemachineBrain` if one is missing (idempotent — an existing brain is " +
            "reused), marks the scene dirty, repaints the Editor, and returns the brain instanceId plus the camera " +
            "name. The whole call runs on the Unity main thread.")]
        [Description("Ensures a CinemachineBrain component exists on a Camera GameObject. " +
            "If no camera is provided, uses Camera.main. Required so CinemachineCameras can drive the view.")]
        public BrainEnsureResponse EnsureBrain
        (
            [Description("Optional reference to the GameObject hosting the Camera. If omitted, Camera.main is used.")]
            GameObjectRef? cameraRef = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                Camera? camera = null;

                if (cameraRef != null && cameraRef.IsValid(out _))
                {
                    var go = cameraRef.FindGameObject(out var error);
                    if (error != null)
                        throw new Exception(error);
                    if (go == null)
                        throw new Exception(Error.GameObjectNotFound());

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

                var cameraGo = camera.gameObject;
                var brain = cameraGo.GetComponent<CinemachineBrain>();
                var created = false;
                if (brain == null)
                {
                    brain = cameraGo.AddComponent<CinemachineBrain>();
                    created = true;
                }

                EditorUtility.SetDirty(cameraGo);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(cameraGo.scene);
                EditorUtils.RepaintAllEditorWindows();

                return new BrainEnsureResponse
                {
                    cameraRef = new GameObjectRef(cameraGo),
                    brainRef = new ComponentRef(brain),
                    cameraName = cameraGo.name,
                    created = created,
                    defaultBlendStyle = brain.DefaultBlend.Style.ToString(),
                    defaultBlendTime = brain.DefaultBlend.Time
                };
            });
        }

        public class BrainEnsureResponse
        {
            [Description("Reference to the Camera GameObject hosting the CinemachineBrain.")]
            public GameObjectRef? cameraRef;

            [Description("Reference to the CinemachineBrain component.")]
            public ComponentRef? brainRef;

            [Description("Name of the Camera GameObject.")]
            public string cameraName = string.Empty;

            [Description("True if a new CinemachineBrain was created; false if one already existed.")]
            public bool created;

            [Description("The current default blend style of the brain.")]
            public string defaultBlendStyle = string.Empty;

            [Description("The current default blend time (seconds) of the brain.")]
            public float defaultBlendTime;
        }
    }
}
