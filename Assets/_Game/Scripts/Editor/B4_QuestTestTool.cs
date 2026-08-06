using System;
using UnityEditor;
using UnityEngine;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.Boot;

namespace GuildMaster.Editor.Tools
{
    public static class B4_QuestTestTool
    {
        [MenuItem("Tools/B4 Fix/Force Trigger Weekly Quests")]
        public static void ForceTriggerQuests()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[B4_QuestTestTool] Please enter Play Mode to force trigger weekly quests.");
                return;
            }

            var boot = UnityEngine.Object.FindFirstObjectByType<UIRuntimeBootstrap>();
            if (boot == null || boot.Services == null)
            {
                Debug.LogError("[B4_QuestTestTool] Cannot find UIRuntimeBootstrap or Services are not initialized.");
                return;
            }

            var questService = boot.Services.Quest as QuestService;
            if (questService == null)
            {
                Debug.LogError("[B4_QuestTestTool] QuestService is not available in the container.");
                return;
            }

            long currentUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 604801; 
            
            bool result = questService.CheckAndTriggerWeeklyQuests(currentUnix);
            
            if (result)
            {
                Debug.Log("[B4_QuestTestTool] Successfully forced weekly quests generation!");
                foreach (var q in questService.GetActiveQuests())
                {
                    Debug.Log($"- {q.Definition.id} (Rarity {q.Rarity})");
                }
            }
            else
            {
                Debug.LogWarning("[B4_QuestTestTool] Weekly quest generation failed or did not trigger.");
            }
        }
    }
}
