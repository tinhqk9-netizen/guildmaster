using UnityEngine;
using GuildMaster.Runtime.UI.Dungeon;
using UnityEditor;
using System.Reflection;
using GuildMaster.Runtime.Services;
using GuildMaster.Definitions;

public class TestDungeonUI
{
    [MenuItem("Tools/Test Dungeon UI")]
    static void Check()
    {
        var screen = Object.FindAnyObjectByType<DungeonScreen>(FindObjectsInactive.Include);
        if (screen != null)
        {
            Debug.Log("Found DungeonScreen!");
            var t = screen.GetType();
            var field = t.GetField("_dungeonService", BindingFlags.NonPublic | BindingFlags.Instance);
            var svc = field.GetValue(screen) as IDungeonService;
            Debug.Log($"Service is null? {svc == null}");
            
            if (svc != null)
            {
                var dungeons = new string[] { "barren_wastelands", "the_southern_grove", "enchanted_forest" };
                foreach (var d in dungeons)
                {
                    Debug.Log($"Dungeon {d} unlocked? {svc.IsDungeonUnlocked(d)}");
                }
            }
        }
        else
        {
            Debug.Log("DungeonScreen not found");
        }
    }
}
