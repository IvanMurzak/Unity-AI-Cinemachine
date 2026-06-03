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
using System.Collections;
using com.IvanMurzak.ReflectorNet.Model;
using AIGD;
using com.IvanMurzak.Unity.MCP.Editor.API;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Cinemachine;

namespace com.IvanMurzak.Unity.MCP.Cinemachine.Editor.Tests
{
    public class TestCinemachineGeneric : BaseTest
    {
        [UnityTest]
        public IEnumerator Get_SerializesCinemachineCamera()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);
            var cam = go.GetComponent<CinemachineCamera>();

            var tool = new Tool_Cinemachine();
            var result = tool.GetComponentData(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                componentRef: new ComponentRef(cam.GetInstanceID()));

            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsNotNull(result.data, "Serialized data should not be null");
            StringAssert.Contains("CinemachineCamera", result.componentType, "Component type should be reported");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Get_FirstCinemachineComponent_WhenNoComponentRef()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);

            var tool = new Tool_Cinemachine();
            var result = tool.GetComponentData(new GameObjectRef(go.GetInstanceID()));

            Assert.IsNotNull(result.data, "Should serialize the first Cinemachine component");
            StringAssert.Contains("Cinemachine", result.componentType, "Resolved component should be a Cinemachine type");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Modify_FollowOffset_ViaReflectorNet()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);
            var follow = go.AddComponent<CinemachineFollow>();
            var reflector = UnityMcpPluginEditor.Instance.Reflector ?? throw new Exception("Reflector not available.");

            var newOffset = new Vector3(0, 5, -12);
            var diff = SerializedMember.FromValue(
                    reflector: reflector,
                    name: follow.GetType().Name,
                    type: typeof(CinemachineFollow),
                    value: null)
                .AddProperty(SerializedMember.FromValue(
                    reflector: reflector,
                    name: nameof(follow.FollowOffset),
                    value: newOffset));

            var tool = new Tool_Cinemachine();
            var result = tool.ModifyComponent(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                data: diff,
                componentRef: new ComponentRef(follow.GetInstanceID()));

            Assert.IsTrue(result.success, "Modification should succeed");
            Assert.AreEqual(newOffset, follow.FollowOffset, "FollowOffset should be modified");

            yield return null;
        }

        [UnityTest]
        public IEnumerator ModifyJson_FollowOffset_Dispatch()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);
            var follow = go.AddComponent<CinemachineFollow>();

            var json = $@"{{
                ""gameObjectRef"": {{ ""instanceID"": {go.GetInstanceID()} }},
                ""componentRef"": {{ ""instanceID"": {follow.GetInstanceID()} }},
                ""data"": {{
                    ""typeName"": ""Unity.Cinemachine.CinemachineFollow"",
                    ""props"": [
                        {{
                            ""name"": ""FollowOffset"",
                            ""typeName"": ""UnityEngine.Vector3"",
                            ""value"": {{ ""x"": 1.0, ""y"": 2.0, ""z"": -3.0 }}
                        }}
                    ]
                }}
            }}";

            var result = RunToolAllowWarnings(Tool_Cinemachine.CinemachineModifyToolId, json);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(new Vector3(1, 2, -3), follow.FollowOffset, "FollowOffset should be modified via JSON");

            yield return null;
        }
    }
}
