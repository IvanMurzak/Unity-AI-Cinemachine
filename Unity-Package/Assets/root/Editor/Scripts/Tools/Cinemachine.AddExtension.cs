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
        public const string CinemachineAddExtensionToolId = "cinemachine-add-extension";

        [AiTool
        (
            CinemachineAddExtensionToolId,
            Title = "Cinemachine / Add Extension",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Add a `CinemachineExtension`-derived component (e.g. `CinemachineDeoccluder`, " +
            "`CinemachineConfiner3D`, `CinemachineFollowZoom`, `CinemachineRecomposer`) to a `CinemachineCamera` " +
            "GameObject, resolved by type name. The type is validated to derive from `CinemachineExtension`.")]
        [AiSkillBody("Add a Cinemachine extension component to a `CinemachineCamera` GameObject. Extensions augment " +
            "the camera pipeline (collision avoidance, framing, post-processing, etc.).\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `CinemachineCamera` (required).\n" +
            "- `extensionTypeName` — the extension type name. A full name like " +
            "`Unity.Cinemachine.CinemachineDeoccluder` is preferred; a bare class name like `CinemachineDeoccluder` " +
            "is also resolved against the `Unity.Cinemachine` namespace as a fallback.\n\n" +
            "## Behavior\n\n" +
            "Resolves the type, verifies it derives from `Unity.Cinemachine.CinemachineExtension`, adds it to the " +
            "GameObject, marks the scene dirty, and repaints. Throws if the type cannot be resolved or is not an " +
            "extension. Runs on the Unity main thread.")]
        [Description("Adds a CinemachineExtension-derived component (by type name) to a CinemachineCamera GameObject. " +
            "The type is validated to derive from CinemachineExtension.")]
        public AddExtensionResponse AddExtension
        (
            [Description("Reference to the GameObject containing the CinemachineCamera component.")]
            GameObjectRef gameObjectRef,
            [Description("Extension type name, e.g. 'Unity.Cinemachine.CinemachineDeoccluder' or 'CinemachineDeoccluder'.")]
            string extensionTypeName
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));
            if (string.IsNullOrWhiteSpace(extensionTypeName))
                throw new ArgumentException("Extension type name must not be empty.", nameof(extensionTypeName));

            return MainThread.Instance.Run(() =>
            {
                var cam = ResolveCinemachineCamera(gameObjectRef, nameof(gameObjectRef));
                var go = cam.gameObject;

                var type = com.IvanMurzak.ReflectorNet.Utils.TypeUtils.GetType(extensionTypeName)
                    ?? com.IvanMurzak.ReflectorNet.Utils.TypeUtils.GetType($"Unity.Cinemachine.{extensionTypeName}");

                if (type == null)
                    throw new Exception(Error.TypeNotFound(extensionTypeName));

                if (!typeof(CinemachineExtension).IsAssignableFrom(type))
                    throw new Exception(Error.NotACinemachineExtension(type.FullName ?? extensionTypeName));

                var added = go.AddComponent(type);

                MarkDirtyAndRepaint(cam, go.scene);

                return new AddExtensionResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    cameraRef = new ComponentRef(cam),
                    extensionRef = added is UnityEngine.Component addedComponent ? new ComponentRef(addedComponent) : null,
                    extensionTypeName = type.FullName ?? type.Name,
                    success = true
                };
            });
        }

        public class AddExtensionResponse
        {
            [Description("Reference to the CinemachineCamera GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the CinemachineCamera component.")]
            public ComponentRef? cameraRef;

            [Description("Reference to the added extension component.")]
            public ComponentRef? extensionRef;

            [Description("Full type name of the added extension.")]
            public string extensionTypeName = string.Empty;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
