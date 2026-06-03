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
    public class TestCinemachineLifecycle : BaseTest
    {
        [UnityTest]
        public IEnumerator CreateCamera_AddsCinemachineCamera()
        {
            var tool = new Tool_Cinemachine();
            var result = tool.CreateCamera(
                name: GO_CameraName,
                position: new Vector3(1, 2, 3),
                rotation: new Vector3(10, 20, 30),
                priority: 42);

            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(GO_CameraName, result.gameObjectName, "Name should match");
            Assert.AreEqual(42, result.priority, "Priority should match");

            var go = GameObject.Find(GO_CameraName);
            Assert.IsNotNull(go, "GameObject should exist in scene");
            var cam = go!.GetComponent<CinemachineCamera>();
            Assert.IsNotNull(cam, "CinemachineCamera should be attached");
            Assert.AreEqual(42, cam!.Priority.Value, "Priority value should be applied");
            Assert.AreEqual(new Vector3(1, 2, 3), go.transform.position, "Position should be applied");

            yield return null;
        }

        [UnityTest]
        public IEnumerator CreateCamera_WithFollowAndLookAt()
        {
            var followGo = new GameObject("FollowTarget");
            var lookAtGo = new GameObject("LookAtTarget");

            var tool = new Tool_Cinemachine();
            var result = tool.CreateCamera(
                name: GO_CameraName,
                followRef: new GameObjectRef(followGo.GetInstanceID()),
                lookAtRef: new GameObjectRef(lookAtGo.GetInstanceID()));

            var go = GameObject.Find(GO_CameraName);
            var cam = go!.GetComponent<CinemachineCamera>();
            Assert.AreEqual(followGo.transform, cam!.Follow, "Follow target should be set");
            Assert.AreEqual(lookAtGo.transform, cam.LookAt, "LookAt target should be set");
            Assert.AreEqual("FollowTarget", result.follow, "Follow name should be reported");

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetCamera_ReturnsConfig()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);
            var cam = go.GetComponent<CinemachineCamera>();
            cam.Priority = new PrioritySettings { Value = 7 };

            var tool = new Tool_Cinemachine();
            var result = tool.GetCamera(new GameObjectRef(go.GetInstanceID()));

            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(7, result.priority, "Priority should be reported");
            Assert.IsNotNull(result.lens, "Lens should be serialized");
            Assert.IsNotNull(result.pipelineComponents, "Pipeline component list should not be null");

            yield return null;
        }

        [UnityTest]
        public IEnumerator ListCameras_FindsCreatedCameras()
        {
            CreateGameObjectWithCinemachineCamera("CamA");
            CreateGameObjectWithCinemachineCamera("CamB");

            var tool = new Tool_Cinemachine();
            var result = tool.ListCameras();

            Assert.IsNotNull(result, "Result should not be null");
            Assert.GreaterOrEqual(result.count, 2, "Should find at least the two created cameras");

            yield return null;
        }

        [UnityTest]
        public IEnumerator EnsureBrain_AddsBrainToCamera()
        {
            var go = CreateGameObjectWithCamera(GO_BrainCameraName);

            var tool = new Tool_Cinemachine();
            var result = tool.EnsureBrain(new GameObjectRef(go.GetInstanceID()));

            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.created, "Brain should have been created");
            Assert.IsNotNull(go.GetComponent<CinemachineBrain>(), "Brain should be attached");

            // Idempotent: second call reuses the existing brain.
            var result2 = tool.EnsureBrain(new GameObjectRef(go.GetInstanceID()));
            Assert.IsFalse(result2.created, "Second call should reuse the existing brain");

            yield return null;
        }

        [UnityTest]
        public IEnumerator CreateCameraJson_Dispatch()
        {
            var json = $@"{{
                ""name"": ""{GO_CameraName}"",
                ""priority"": 15
            }}";

            var result = RunToolAllowWarnings(Tool_Cinemachine.CinemachineCameraCreateToolId, json);
            Assert.IsNotNull(result, "Result should not be null");

            var go = GameObject.Find(GO_CameraName);
            Assert.IsNotNull(go, "Camera GameObject should exist");
            Assert.AreEqual(15, go!.GetComponent<CinemachineCamera>().Priority.Value, "Priority should be applied");

            yield return null;
        }
    }
}
