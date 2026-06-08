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
using System.Collections;
using AIGD;
using com.IvanMurzak.Unity.MCP.Editor.API;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Cinemachine;

namespace com.IvanMurzak.Unity.MCP.Cinemachine.Editor.Tests
{
    public class TestCinemachineConfig : BaseTest
    {
        [UnityTest]
        public IEnumerator SetTargets_SetsFollowAndLookAt()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);
            var followGo = new GameObject("FollowTarget");
            var lookAtGo = new GameObject("LookAtTarget");

            var tool = new Tool_Cinemachine();
            var result = tool.SetTargets(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                followRef: new GameObjectRef(followGo.GetInstanceID()),
                lookAtRef: new GameObjectRef(lookAtGo.GetInstanceID()));

            Assert.IsTrue(result.success, "Should succeed");
            var cam = go.GetComponent<CinemachineCamera>();
            Assert.AreEqual(followGo.transform, cam.Follow, "Follow should be set");
            Assert.AreEqual(lookAtGo.transform, cam.LookAt, "LookAt should be set");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetTargets_ClearFollow()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);
            var followGo = new GameObject("FollowTarget");
            var cam = go.GetComponent<CinemachineCamera>();
            cam.Follow = followGo.transform;

            var tool = new Tool_Cinemachine();
            tool.SetTargets(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                clearFollow: true);

            Assert.IsNull(cam.Follow, "Follow should be cleared");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetPriority_SetsValue()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);

            var tool = new Tool_Cinemachine();
            var result = tool.SetPriority(new GameObjectRef(go.GetInstanceID()), 99);

            Assert.IsTrue(result.success, "Should succeed");
            Assert.AreEqual(99, go.GetComponent<CinemachineCamera>().Priority.Value, "Priority should be applied");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetLens_SetsFields()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);

            var tool = new Tool_Cinemachine();
            var result = tool.SetLens(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                fieldOfView: 75f,
                nearClipPlane: 0.05f,
                farClipPlane: 500f,
                dutch: 12f);

            Assert.IsTrue(result.success, "Should succeed");
            var lens = go.GetComponent<CinemachineCamera>().Lens;
            Assert.AreEqual(75f, lens.FieldOfView, 0.001f, "FOV should be applied");
            Assert.AreEqual(0.05f, lens.NearClipPlane, 0.001f, "Near clip should be applied");
            Assert.AreEqual(500f, lens.FarClipPlane, 0.001f, "Far clip should be applied");
            Assert.AreEqual(12f, lens.Dutch, 0.001f, "Dutch should be applied");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetPriorityJson_Dispatch()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);

            var json = $@"{{
                ""gameObjectRef"": {{ ""instanceID"": {go.GetInstanceID()} }},
                ""priority"": 33
            }}";

            var result = RunToolAllowWarnings(Tool_Cinemachine.CinemachineSetPriorityToolId, json);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(33, go.GetComponent<CinemachineCamera>().Priority.Value, "Priority should be applied");

            yield return null;
        }
    }
}
