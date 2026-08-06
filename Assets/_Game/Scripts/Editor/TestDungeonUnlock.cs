using UnityEngine;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Boot;
using UnityEditor;

public class TestDungeonUnlock
{
    [MenuItem("Tools/Test Dungeon Unlock")]
    static void Check()
    {
        var boot = Object.FindAnyObjectByType<UIRuntimeBootstrap>();
        if (boot != null && boot.Services != null)
        {
            foreach(var def in boot.Services.Database.GetAll<DungeonDefinition>())
            {
                Debug.Log($"Dungeon {def.id}: req='{def.RequiredClearDungeonId}'");
            }
        }
        else
        {
            Debug.Log("Play mode not active or boot not found");
        }
    }
}
