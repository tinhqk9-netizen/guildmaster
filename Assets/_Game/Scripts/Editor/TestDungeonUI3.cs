using UnityEngine;
using GuildMaster.Runtime.Boot;
using UnityEditor;

public class TestDungeonUI3
{
    [MenuItem("Tools/Test Dungeon UI 3")]
    static void Check()
    {
        var boot = Object.FindFirstObjectByType<UIRuntimeBootstrap>();
        if (boot != null && boot.Services != null)
        {
            Debug.Log($"boot.Services.Dungeon == null? {boot.Services.Dungeon == null}");
            if (boot.Services.Dungeon != null) {
                Debug.Log($"DungeonService type: {boot.Services.Dungeon.GetType().Name}");
            }
        }
        else
        {
            Debug.Log("boot or Services is null");
        }
    }
}
