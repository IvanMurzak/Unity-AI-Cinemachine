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
using System.Text.Json;
using com.IvanMurzak.McpPlugin.Common.Model;
using com.IvanMurzak.ReflectorNet;
using NUnit.Framework;
using UnityEngine;
using Unity.Cinemachine;

namespace com.IvanMurzak.Unity.MCP.Cinemachine.Editor.Tests
{
    public class BaseTest : com.IvanMurzak.Unity.MCP.Editor.Tests.BaseTest
    {
        protected const string GO_CameraName = "TestCinemachineCamera";
        protected const string GO_BrainCameraName = "TestBrainCamera";

        protected virtual ResponseData<ResponseCallTool> RunToolAllowWarnings(string toolName, string json)
        {
            var reflector = UnityMcpPluginEditor.Instance.Reflector ?? throw new Exception("Reflector not available.");

            Debug.Log($"{toolName} Started with JSON:\n{json}");

            var parameters = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            var request = new RequestCallTool(toolName, parameters!);
            var task = UnityMcpPluginEditor.Instance.Tools!.RunCallTool(request);
            var result = task.Result;

            Debug.Log($"{toolName} Completed");

            var jsonResult = result.ToJson(reflector);
            Debug.Log($"{toolName} Result:\n{jsonResult}");

            Assert.IsFalse(result.Status == ResponseStatus.Error, $"Tool call failed with error status: {result.Message}");
            Assert.IsNotNull(result.Message, $"Tool call returned null message");
            Assert.IsFalse(result.Message!.Contains("[Error]"), $"Tool call failed with error: {result.Message}");
            Assert.IsNotNull(result.Value, $"Tool call returned null value");
            Assert.IsFalse(result.Value!.Status == ResponseStatus.Error, $"Tool call failed");
            Assert.IsFalse(jsonResult!.Contains("[Error]"), $"Tool call failed with error in JSON: {jsonResult}");

            return result;
        }

        /// <summary>Create a GameObject with a CinemachineCamera in the active scene.</summary>
        protected static GameObject CreateGameObjectWithCinemachineCamera(string name = "TestCinemachineCamera")
        {
            var go = new GameObject(name);
            go.AddComponent<CinemachineCamera>();
            return go;
        }

        /// <summary>Create a GameObject with a Camera (no brain yet) in the active scene.</summary>
        protected static GameObject CreateGameObjectWithCamera(string name = "TestBrainCamera")
        {
            var go = new GameObject(name);
            go.AddComponent<Camera>();
            return go;
        }
    }
}
