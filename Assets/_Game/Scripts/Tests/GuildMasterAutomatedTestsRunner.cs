using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using System.IO;
using System.Text;

namespace GuildMaster.Editor.Verification
{
    public class GuildMasterAutomatedTestsRunner : ScriptableObject, ICallbacks
    {
        private static GuildMasterAutomatedTestsRunner _instance;
        private bool _isRunningPlayMode;

        [MenuItem("GuildMaster/Verification/Run Full Automated Verification", priority = 100)]
        public static void RunTests()
        {
            if (_instance == null)
            {
                _instance = CreateInstance<GuildMasterAutomatedTestsRunner>();
            }

            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(_instance);

            Debug.Log("[Verification] Starting EditMode Tests...");
            
            var filter = new Filter
            {
                testMode = TestMode.EditMode
            };
            
            _instance._isRunningPlayMode = false;
            api.Execute(new ExecutionSettings(filter));
        }

        public void RunPlayModeTests()
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(this);

            Debug.Log("[Verification] Starting PlayMode Tests...");
            
            var filter = new Filter
            {
                testMode = TestMode.PlayMode
            };
            
            _isRunningPlayMode = true;
            api.Execute(new ExecutionSettings(filter));
        }

        public void RunStarted(ITestAdaptor testsToRun) { }

        public void RunFinished(ITestResultAdaptor result)
        {
            string type = _isRunningPlayMode ? "PlayMode" : "EditMode";
            string path = Path.Combine(Application.dataPath, $"../Reports/Completion/{type}_Test_Result.xml");

            // Format simple XML result
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine($"<testrun name=\"{type}\" total=\"{result.PassCount + result.FailCount + result.SkipCount}\" passed=\"{result.PassCount}\" failed=\"{result.FailCount}\" skipped=\"{result.SkipCount}\" duration=\"{result.Duration}\">");
            
            FormatResult(result, sb);
            
            sb.AppendLine("</testrun>");
            
            File.WriteAllText(path, sb.ToString());
            Debug.Log($"[Verification] {type} tests finished. Results saved to {path}");

            if (!_isRunningPlayMode)
            {
                // Trigger PlayMode tests after EditMode finishes
                RunPlayModeTests();
            }
        }

        private void FormatResult(ITestResultAdaptor result, StringBuilder sb)
        {
            if (!result.HasChildren)
            {
                sb.AppendLine($"  <testcase name=\"{result.Name}\" status=\"{result.TestStatus}\" duration=\"{result.Duration}\">");
                if (result.TestStatus == TestStatus.Failed)
                {
                    sb.AppendLine($"    <failure message=\"{System.Security.SecurityElement.Escape(result.Message ?? "")}\">");
                    sb.AppendLine($"      <stacktrace>{System.Security.SecurityElement.Escape(result.StackTrace ?? "")}</stacktrace>");
                    sb.AppendLine("    </failure>");
                }
                sb.AppendLine("  </testcase>");
            }
            else
            {
                foreach (var child in result.Children)
                {
                    FormatResult(child, sb);
                }
            }
        }

        public void TestStarted(ITestAdaptor test) { }
        public void TestFinished(ITestResultAdaptor result) { }
    }
}
