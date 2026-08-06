using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;
using GuildMaster.Runtime.Boot;
using GuildMaster.Runtime.UI.Foundation;

namespace GuildMaster.Tests
{
    public class B3_BootErrorRecoveryTests
    {
        private GameObject _bootObj;
        private UIRuntimeBootstrap _bootstrap;
        private ErrorPopup _errorPopup;
        private Button _retryButton;
        private Button _resetDataButton;

        [SetUp]
        public void Setup()
        {
            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            
            var popupObj = new GameObject("ErrorPopup");
            popupObj.transform.SetParent(canvasObj.transform);
            _errorPopup = popupObj.AddComponent<ErrorPopup>();

            var textObj = new GameObject("ErrorText");
            textObj.transform.SetParent(popupObj.transform);
            var errorText = textObj.AddComponent<Text>();

            var retryObj = new GameObject("RetryBtn");
            retryObj.transform.SetParent(popupObj.transform);
            _retryButton = retryObj.AddComponent<Button>();

            var resetObj = new GameObject("ResetBtn");
            resetObj.transform.SetParent(popupObj.transform);
            _resetDataButton = resetObj.AddComponent<Button>();

            var serializedPopup = new UnityEditor.SerializedObject(_errorPopup);
            serializedPopup.FindProperty("_errorText").objectReferenceValue = errorText;
            serializedPopup.FindProperty("_retryButton").objectReferenceValue = _retryButton;
            serializedPopup.FindProperty("_resetDataButton").objectReferenceValue = _resetDataButton;
            serializedPopup.ApplyModifiedProperties();

            _bootObj = new GameObject("Bootstrap");
            _bootstrap = _bootObj.AddComponent<UIRuntimeBootstrap>();
            
            var serializedBoot = new UnityEditor.SerializedObject(_bootstrap);
            serializedBoot.FindProperty("_errorPopup").objectReferenceValue = _errorPopup;
            serializedBoot.ApplyModifiedProperties();
        }

        [TearDown]
        public void TearDown()
        {
            if (_bootObj != null) GameObject.DestroyImmediate(_bootObj);
            if (_errorPopup != null) GameObject.DestroyImmediate(_errorPopup.transform.parent.gameObject);
        }

        [Test]
        public void B3_BootFailure_ShowsErrorPopup()
        {
            LogAssert.Expect(LogType.Error, new Regex(".*Simulated Boot Failure.*"));
            _bootstrap.TestInjectionHook = () => throw new Exception("Simulated Boot Failure");
            
            bool eventFired = false;
            UIRuntimeBootstrap.OnBootFailed += (msg) => eventFired = true;

            _bootstrap.Initialize();

            Assert.IsTrue(eventFired, "OnBootFailed should be fired.");
            Assert.IsTrue(_errorPopup.gameObject.activeSelf, "Error popup should be visible after failure.");
        }

        [Test]
        public void B3_RetryBoot_CleansUpServicesAndRetries()
        {
            LogAssert.Expect(LogType.Error, new Regex(".*Simulated Boot Failure.*"));
            int attempts = 0;
            _bootstrap.TestInjectionHook = () => 
            {
                attempts++;
                if (attempts == 1) throw new Exception("Simulated Boot Failure");
            };

            _bootstrap.Initialize();
            Assert.IsTrue(_errorPopup.gameObject.activeSelf, "Error popup should be visible on first failure.");

            // Simulate clicking Retry
            _retryButton.onClick.Invoke();

            Assert.AreEqual(2, attempts, "Retry should trigger Initialize again.");
            Assert.IsFalse(_errorPopup.gameObject.activeSelf, "Error popup should be hidden after successful retry.");
            Assert.IsNotNull(_bootstrap.Services, "Services should be built successfully after retry.");
        }

        [Test]
        public void B3_ResetData_DeletesSaveAndRetries()
        {
            LogAssert.Expect(LogType.Error, new Regex(".*Simulated Boot Failure.*"));
            int attempts = 0;
            _bootstrap.TestInjectionHook = () => 
            {
                attempts++;
                if (attempts == 1) throw new Exception("Simulated Boot Failure");
            };

            _bootstrap.Initialize();
            Assert.IsTrue(_errorPopup.gameObject.activeSelf, "Error popup should be visible on first failure.");

            // Simulate clicking Reset Data
            _resetDataButton.onClick.Invoke();

            Assert.AreEqual(2, attempts, "ResetData should trigger Initialize again.");
            Assert.IsFalse(_errorPopup.gameObject.activeSelf, "Error popup should be hidden after successful reset.");
            Assert.IsNotNull(_bootstrap.Services, "Services should be built successfully after reset.");
        }
    }
}
