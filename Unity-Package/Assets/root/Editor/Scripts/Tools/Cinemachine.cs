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
using com.IvanMurzak.McpPlugin;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    [AiToolType]
    public partial class Tool_Cinemachine
    {
        public static class Error
        {
            public static string GameObjectNotFound()
                => "[Error] GameObject not found. Provide a valid reference to an existing GameObject.";

            public static string CinemachineCameraNotFound()
                => "[Error] CinemachineCamera component not found on the target GameObject. " +
                   "Make sure the GameObject has a CinemachineCamera component attached.";

            public static string CinemachineBrainNotFound()
                => "[Error] CinemachineBrain component not found. Provide a Camera that has (or can host) a CinemachineBrain.";

            public static string NoMainCamera()
                => "[Error] No Camera found. Provide a 'cameraRef' or make sure a Camera tagged 'MainCamera' exists in the scene.";

            public static string ComponentNotFound(string componentTypeName)
                => $"[Error] Component '{componentTypeName}' not found on the target GameObject.";

            public static string TypeNotFound(string typeName)
                => $"[Error] Type '{typeName}' could not be resolved. Provide a full type name (e.g. 'Unity.Cinemachine.CinemachineFollow').";

            public static string NotACinemachineExtension(string typeName)
                => $"[Error] Type '{typeName}' does not derive from Unity.Cinemachine.CinemachineExtension.";

            public static string ReflectorNotAvailable()
                => "[Error] ReflectorNet reflector is not available.";
        }
    }
}
