using UnityEngine;
using GuildMaster.Runtime.UI.Dungeon;
using UnityEditor;
using System.Reflection;
using GuildMaster.Runtime.Services;
using GuildMaster.Definitions;

public class TestDungeonUI2
{
    [MenuItem("Tools/Test Dungeon UI 2")]
    static void Check()
    {
        var screens = Object.FindObjectsByType<DungeonScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"Found {screens.Length} DungeonScreens!");
        foreach(var screen in screens)
        {
            var t = screen.GetType();
            var field = t.GetField("_dungeonService", BindingFlags.NonPublic | BindingFlags.Instance);
            var svc = field.GetValue(screen) as IDungeonService;
            Debug.Log($"[{screen.name}] Service is null? {svc == null}");
            
            if (svc != null)
            {
                var isUnlocked = svc.IsDungeonUnlocked("barren_wastelands");
                Debug.Log($"[{screen.name}] barren_wastelands unlocked? {isUnlocked}");
            }
        }
    }
}
