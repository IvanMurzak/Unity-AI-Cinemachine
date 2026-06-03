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
        public const string CinemachineSetLensToolId = "cinemachine-set-lens";

        [AiTool
        (
            CinemachineSetLensToolId,
            Title = "Cinemachine / Set Lens",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Set lens fields of a `CinemachineCamera`'s `LensSettings` — field of view (perspective) " +
            "or orthographic size, near / far clip planes, and Dutch (roll) angle. Only the fields you pass are changed.")]
        [AiSkillBody("Set lens fields on a `CinemachineCamera`. The `LensSettings` controls how the resulting Unity " +
            "Camera is configured while this virtual camera is live.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `CinemachineCamera` (required).\n" +
            "- `fieldOfView` — optional vertical FOV in degrees (perspective cameras).\n" +
            "- `orthographicSize` — optional orthographic half-height (orthographic cameras).\n" +
            "- `nearClipPlane`, `farClipPlane` — optional clip plane distances.\n" +
            "- `dutch` — optional Dutch (roll) angle in degrees.\n\n" +
            "## Behavior\n\n" +
            "Reads the current `LensSettings`, applies only the provided fields, writes it back, marks the scene " +
            "dirty, and repaints the Editor. The whole call runs on the Unity main thread.")]
        [Description("Sets lens fields (FOV/ortho size, near/far clip, dutch) of a CinemachineCamera. Only provided fields change.")]
        public SetLensResponse SetLens
        (
            [Description("Reference to the GameObject containing the CinemachineCamera component.")]
            GameObjectRef gameObjectRef,
            [Description("Vertical field of view in degrees (perspective).")]
            float? fieldOfView = null,
            [Description("Orthographic size (half-height) for orthographic cameras.")]
            float? orthographicSize = null,
            [Description("Near clip plane distance.")]
            float? nearClipPlane = null,
            [Description("Far clip plane distance.")]
            float? farClipPlane = null,
            [Description("Dutch (roll) angle in degrees.")]
            float? dutch = null
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var cam = ResolveCinemachineCamera(gameObjectRef, nameof(gameObjectRef));

                var lens = cam.Lens;
                if (fieldOfView.HasValue) lens.FieldOfView = fieldOfView.Value;
                if (orthographicSize.HasValue) lens.OrthographicSize = orthographicSize.Value;
                if (nearClipPlane.HasValue) lens.NearClipPlane = nearClipPlane.Value;
                if (farClipPlane.HasValue) lens.FarClipPlane = farClipPlane.Value;
                if (dutch.HasValue) lens.Dutch = dutch.Value;
                cam.Lens = lens;

                MarkDirtyAndRepaint(cam, cam.gameObject.scene);

                return new SetLensResponse
                {
                    gameObjectRef = new GameObjectRef(cam.gameObject),
                    cameraRef = new ComponentRef(cam),
                    fieldOfView = cam.Lens.FieldOfView,
                    orthographicSize = cam.Lens.OrthographicSize,
                    nearClipPlane = cam.Lens.NearClipPlane,
                    farClipPlane = cam.Lens.FarClipPlane,
                    dutch = cam.Lens.Dutch,
                    success = true
                };
            });
        }

        public class SetLensResponse
        {
            [Description("Reference to the CinemachineCamera GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the CinemachineCamera component.")]
            public ComponentRef? cameraRef;

            [Description("Resulting field of view.")]
            public float fieldOfView;

            [Description("Resulting orthographic size.")]
            public float orthographicSize;

            [Description("Resulting near clip plane.")]
            public float nearClipPlane;

            [Description("Resulting far clip plane.")]
            public float farClipPlane;

            [Description("Resulting Dutch (roll) angle.")]
            public float dutch;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
