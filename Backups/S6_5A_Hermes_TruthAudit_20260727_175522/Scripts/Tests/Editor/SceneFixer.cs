using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using GuildMaster.Runtime.UI.Tavern;
using GuildMaster.Runtime.UI.Quest;
using GuildMaster.Runtime.UI.Character;
using GuildMaster.Runtime.UI.Craft;
using GuildMaster.Runtime.UI.Merchant;
using GuildMaster.Runtime.UI.Dungeon;
using GuildMaster.Runtime.UI.Inventory;
using GuildMaster.Runtime.UI.Settings;

public static class SceneFixer
{
    public static void FixMainScene()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/Main.unity");
        var screenRoot = GameObject.Find("ScreenRoot");
        
        EnsureComponent<TavernScreen>(screenRoot);
        EnsureComponent<QuestScreen>(screenRoot);
        EnsureComponent<CharacterScreen>(screenRoot);
        EnsureComponent<CraftScreen>(screenRoot);
        EnsureComponent<MerchantScreen>(screenRoot);
        EnsureComponent<DungeonScreen>(screenRoot);
        EnsureComponent<InventoryScreen>(screenRoot);
        EnsureComponent<SettingsScreen>(screenRoot);

        EditorSceneManager.SaveScene(scene);
        Debug.Log("Scene fixed!");
        EditorApplication.Exit(0);
    }

    private static void EnsureComponent<T>(GameObject root) where T : Component
    {
        var comp = Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
        if (comp == null)
        {
            string name = typeof(T).Name;
            var obj = root.transform.Find(name)?.gameObject;
            if (obj == null)
            {
                obj = new GameObject(name);
                obj.transform.SetParent(root.transform);
            }
            obj.AddComponent<T>();
            Debug.Log($"Added {name} component to {obj.name}");
        }
    }
}
