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
using System.ComponentModel;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Cinemachine
    {
        /// <summary>
        /// The position-control ("Body") component of a CinemachineCamera pipeline (Cinemachine 3.x).
        /// </summary>
        public enum BodyType
        {
            [Description("Remove the position-control (Body) component, leaving the camera at its own transform position.")]
            None,

            [Description("CinemachineFollow — keeps a fixed offset from the Follow target.")]
            Follow,

            [Description("CinemachineOrbitalFollow — orbits around the Follow target with horizontal/vertical input axes.")]
            OrbitalFollow,

            [Description("CinemachineThirdPersonFollow — over-the-shoulder rig anchored to the Follow target.")]
            ThirdPersonFollow,

            [Description("CinemachinePositionComposer — frames the Follow target on screen at a fixed camera distance.")]
            PositionComposer,

            [Description("CinemachineHardLockToTarget — snaps the camera exactly onto the Follow target's position.")]
            HardLockToTarget
        }

        /// <summary>
        /// The rotation-control ("Aim") component of a CinemachineCamera pipeline (Cinemachine 3.x).
        /// </summary>
        public enum AimType
        {
            [Description("Remove the rotation-control (Aim) component, leaving the camera at its own transform rotation.")]
            None,

            [Description("CinemachineRotationComposer — rotates to keep the LookAt target at a screen position.")]
            RotationComposer,

            [Description("CinemachineHardLookAt — points the camera directly at the LookAt target.")]
            HardLookAt,

            [Description("CinemachinePanTilt — pan/tilt rotation driven by input axes, independent of a LookAt target.")]
            PanTilt
        }
    }
}
