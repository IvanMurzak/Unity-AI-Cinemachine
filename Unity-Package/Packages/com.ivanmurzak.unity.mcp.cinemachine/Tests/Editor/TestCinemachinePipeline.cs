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
    public class TestCinemachinePipeline : BaseTest
    {
        [UnityTest]
        public IEnumerator SetBody_AddsFollow()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);

            var tool = new Tool_Cinemachine();
            var result = tool.SetBody(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                bodyType: Tool_Cinemachine.BodyType.Follow,
                followOffset: new Vector3(0, 3, -8),
                damping: 1.5f);

            Assert.IsTrue(result.success, "Should succeed");
            var follow = go.GetComponent<CinemachineFollow>();
            Assert.IsNotNull(follow, "CinemachineFollow should be attached");
            Assert.AreEqual(new Vector3(0, 3, -8), follow!.FollowOffset, "FollowOffset should be applied");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetBody_ReplacesPreviousBody()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);
            var tool = new Tool_Cinemachine();

            tool.SetBody(new GameObjectRef(go.GetInstanceID()), Tool_Cinemachine.BodyType.Follow);
            tool.SetBody(new GameObjectRef(go.GetInstanceID()), Tool_Cinemachine.BodyType.ThirdPersonFollow, cameraDistance: 5f);

            Assert.IsNull(go.GetComponent<CinemachineFollow>(), "Previous Follow body should be removed");
            Assert.IsNotNull(go.GetComponent<CinemachineThirdPersonFollow>(), "ThirdPersonFollow should be attached");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetBody_NoneRemovesBody()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);
            var tool = new Tool_Cinemachine();

            tool.SetBody(new GameObjectRef(go.GetInstanceID()), Tool_Cinemachine.BodyType.Follow);
            tool.SetBody(new GameObjectRef(go.GetInstanceID()), Tool_Cinemachine.BodyType.None);

            Assert.IsNull(go.GetComponent<CinemachineFollow>(), "Body should be removed");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetAim_AddsRotationComposer()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);

            var tool = new Tool_Cinemachine();
            var result = tool.SetAim(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                aimType: Tool_Cinemachine.AimType.RotationComposer,
                screenX: 0.1f,
                screenY: -0.2f,
                damping: 0.5f);

            Assert.IsTrue(result.success, "Should succeed");
            var composer = go.GetComponent<CinemachineRotationComposer>();
            Assert.IsNotNull(composer, "RotationComposer should be attached");
            Assert.AreEqual(0.1f, composer!.Composition.ScreenPosition.x, 0.001f, "Screen X should be applied");
            Assert.AreEqual(-0.2f, composer.Composition.ScreenPosition.y, 0.001f, "Screen Y should be applied");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetAim_HardLookAt()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);
            var tool = new Tool_Cinemachine();
            tool.SetAim(new GameObjectRef(go.GetInstanceID()), Tool_Cinemachine.AimType.HardLookAt);
            Assert.IsNotNull(go.GetComponent<CinemachineHardLookAt>(), "HardLookAt should be attached");
            yield return null;
        }

        [UnityTest]
        public IEnumerator SetNoise_AddsPerlin()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);

            var tool = new Tool_Cinemachine();
            var result = tool.SetNoise(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                amplitudeGain: 2f,
                frequencyGain: 3f);

            Assert.IsTrue(result.success, "Should succeed");
            var noise = go.GetComponent<CinemachineBasicMultiChannelPerlin>();
            Assert.IsNotNull(noise, "Perlin noise should be attached");
            Assert.AreEqual(2f, noise!.AmplitudeGain, 0.001f, "AmplitudeGain should be applied");
            Assert.AreEqual(3f, noise.FrequencyGain, 0.001f, "FrequencyGain should be applied");

            yield return null;
        }

        [UnityTest]
        public IEnumerator AddExtension_AddsDeoccluder()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);

            var tool = new Tool_Cinemachine();
            var result = tool.AddExtension(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                extensionTypeName: "CinemachineDeoccluder");

            Assert.IsTrue(result.success, "Should succeed");
            Assert.IsNotNull(go.GetComponent<CinemachineDeoccluder>(), "Deoccluder extension should be attached");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetDefaultBlend_SetsBrainBlend()
        {
            var go = CreateGameObjectWithCamera(GO_BrainCameraName);
            go.AddComponent<CinemachineBrain>();

            var tool = new Tool_Cinemachine();
            var result = tool.SetDefaultBlend(
                style: CinemachineBlendDefinition.Styles.Linear,
                time: 1.25f,
                cameraRef: new GameObjectRef(go.GetInstanceID()));

            Assert.IsTrue(result.success, "Should succeed");
            var brain = go.GetComponent<CinemachineBrain>();
            Assert.AreEqual(CinemachineBlendDefinition.Styles.Linear, brain.DefaultBlend.Style, "Style should be applied");
            Assert.AreEqual(1.25f, brain.DefaultBlend.Time, 0.001f, "Time should be applied");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetBodyJson_Dispatch()
        {
            var go = CreateGameObjectWithCinemachineCamera(GO_CameraName);

            var json = $@"{{
                ""gameObjectRef"": {{ ""instanceID"": {go.GetInstanceID()} }},
                ""bodyType"": ""Follow""
            }}";

            var result = RunToolAllowWarnings(Tool_Cinemachine.CinemachineSetBodyToolId, json);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsNotNull(go.GetComponent<CinemachineFollow>(), "Follow body should be attached");

            yield return null;
        }
    }
}
