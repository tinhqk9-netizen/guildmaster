using System;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Database;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Core;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.UI.HUD;
using GuildMaster.Runtime.UI.Inventory;
using GuildMaster.Runtime.UI.Character;
using GuildMaster.Runtime.UI.Popup;

namespace GuildMaster.Runtime.Boot
{
    /// <summary>
    /// Minimal composition root for the Main scene (S5 scope). Builds the services the
    /// existing UI screens depend on, registers every UIScreen with a UIService, initializes
    /// the HUD / Inventory / Character / Popup controllers, wires Back buttons, and shows the
    /// HUD. This is GLUE ONLY — it wires existing systems together. It adds no gameplay rules
    /// and no fake data; empty save/database simply produce empty screens.
    ///
    /// Full boot orchestration (Boot→Main transition, DI container) remains S6 work.
    /// </summary>
    public class UIRuntimeBootstrap : MonoBehaviour
    {
        private UIService _ui;
        private ISaveService _save;

        private void Start()
        {
            try
            {
                // --- backend composition (mirrors Runtime/Boot/Bootstrapper) ---
#if UNITY_EDITOR
                IGameDataProvider provider = new EditorExternalGameDataProvider();
#else
                IGameDataProvider provider = new StreamingAssetsGameDataProvider();
#endif
                var serializer = new UnityJsonSerializer();
                var db = new GameDatabase();
                LogDatabaseBuild(new DatabaseBuilder(provider, serializer, db).Build(), provider);

                var formula = new FormulaService();
                var save = new SaveService();
                save.Load(out _);
                _save = save;

                var factory = new RuntimeFactory(new DefaultInstanceIdGenerator());
                var itemService = new ItemService(factory, db);
                var inventory = new InventoryService(save, formula, itemService, db);
                var characterService = new CharacterService(save, formula, db, factory, inventory);

                // --- UI wiring ---
                _ui = new UIService();

                UIScreen[] screens = FindObjectsByType<UIScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                PopupScreen popup = null;

                foreach (UIScreen screen in screens)
                {
                    if (screen is PopupScreen p && screen.IsPopup)
                    {
                        popup = p;
                        continue;
                    }

                    _ui.RegisterScreen(screen);
                    WireBackButton(screen);
                }

                var hud = FindFirstObjectByType<HUDController>(FindObjectsInactive.Include);
                if (hud != null) hud.Initialize(save, _ui);

                var inv = FindFirstObjectByType<InventoryScreen>(FindObjectsInactive.Include);
                if (inv != null) inv.Initialize(inventory);

                var chr = FindFirstObjectByType<CharacterScreen>(FindObjectsInactive.Include);
                if (chr != null) chr.Initialize(characterService);

                if (popup != null) _ui.RegisterDialogScreen(popup);

                _ui.ShowScreen(UIScreenId.MainHUD);
                Debug.Log($"[UIRuntimeBootstrap] Wired {screens.Length} screen(s); MainHUD shown.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UIRuntimeBootstrap] Failed to wire UI: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Surfaces the database build result. Without this the build silently produces an empty
        /// database when data cannot be read (e.g. missing StreamingAssets in a player build),
        /// leaving the UI up but blank with nothing in the log to diagnose it.
        /// </summary>
        private static void LogDatabaseBuild(DatabaseBuildReport report, IGameDataProvider provider)
        {
            int totalRecords = 0;
            foreach (var kvp in report.loadedRecordsByCategory) totalRecords += kvp.Value;

            if (report.hasFatalErrors)
            {
                Debug.LogError(
                    $"[UIRuntimeBootstrap] Database build FAILED via {provider.ProviderName}. " +
                    $"manifestLoaded={report.manifestLoaded}, files={report.loadedFiles}/{report.expectedFiles}. " +
                    $"Errors: {string.Join(" | ", report.errors)}");
                return;
            }

            Debug.Log(
                $"[UIRuntimeBootstrap] Database built via {provider.ProviderName}: " +
                $"{report.loadedFiles}/{report.expectedFiles} file(s), {totalRecords} record(s).");

            if (report.recordCountMismatches.Count > 0)
            {
                Debug.LogWarning($"[UIRuntimeBootstrap] Record count mismatches: {string.Join(" | ", report.recordCountMismatches)}");
            }
        }

        /// <summary>Persists the current save state. No-op until <see cref="_save"/> is set by Start().</summary>
        private void PersistSave(string reason)
        {
            if (_save == null) return;

            if (_save.Save(out Exception error))
            {
                Debug.Log($"[UIRuntimeBootstrap] Save written ({reason}).");
            }
            else
            {
                Debug.LogError($"[UIRuntimeBootstrap] Save failed ({reason}): {error?.Message}");
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) PersistSave("OnApplicationPause");
        }

        private void OnApplicationQuit()
        {
            PersistSave("OnApplicationQuit");
        }

        /// <summary>Any non-HUD screen with a child "Btn_Back" returns to the previous screen.</summary>
        private void WireBackButton(UIScreen screen)
        {
            if (screen.ScreenId == UIScreenId.MainHUD) return;

            Transform backT = screen.transform.Find("Btn_Back");
            if (backT != null && backT.TryGetComponent(out Button backBtn))
            {
                backBtn.onClick.RemoveAllListeners();
                backBtn.onClick.AddListener(() => _ui.Back());
            }
        }
    }
}
