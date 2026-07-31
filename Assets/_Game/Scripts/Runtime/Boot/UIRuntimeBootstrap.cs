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
using GuildMaster.Runtime.UI.Foundation;
using GuildMaster.Runtime.UI.HUD;
using GuildMaster.Runtime.UI.Inventory;
using GuildMaster.Runtime.UI.Character;
using GuildMaster.Runtime.UI.Popup;
using GuildMaster.Runtime.UI.Tavern;
using GuildMaster.Runtime.UI.Craft;
using GuildMaster.Runtime.UI.Merchant;
using GuildMaster.Runtime.UI.Dungeon;
using GuildMaster.Runtime.UI.Quest;
using GuildMaster.Runtime.UI.Settings;

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
        public ServiceContainer Services { get; private set; }

        public static event Action<string> OnBootFailed;
        private bool _isInitializing;
        
        [SerializeField] private ErrorPopup _errorPopup;

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_isInitializing) return;
            _isInitializing = true;

            // Cleanup if retrying
            if (Services != null)
            {
                Services = null;
            }

            try
            {
                if (_errorPopup != null) _errorPopup.Hide();
                
                // --- backend composition ---
#if UNITY_EDITOR
                IGameDataProvider provider = new EditorExternalGameDataProvider();
#else
                IGameDataProvider provider = new StreamingAssetsGameDataProvider();
#endif
                var serializer = new UnityJsonSerializer();
                var db = new GameDatabase();
                LogDatabaseBuild(new DatabaseBuilder(provider, serializer, db).Build(), provider);

                Services = new ServiceContainer(db);
                _save = Services.Save;

                var gameLoopRunner = gameObject.AddComponent<GameLoopRunner>();
                
                var offlineBuilder = new OfflineProgressSummaryBuilder(Services.Save);
                OfflineStateDiffSummary offlineSummary = offlineBuilder.BuildSummary(() => 
                {
                    gameLoopRunner.Initialize(Services.GameLoop, Services.Save);
                });

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
                if (hud != null) hud.Initialize(Services.Save, _ui, offlineSummary);

                // Inventory and Character cross-reference each other's current selection
                // (equip pulls the selected item, use-consumable targets the selected character),
                // so both must exist before either is initialized.
                var inv = FindFirstObjectByType<InventoryScreen>(FindObjectsInactive.Include);
                var chr = FindFirstObjectByType<CharacterScreen>(FindObjectsInactive.Include);
                if (inv != null) inv.Initialize(Services, chr);
                if (chr != null) chr.Initialize(Services, inv);

                var tav = FindFirstObjectByType<TavernScreen>(FindObjectsInactive.Include);
                if (tav != null) tav.Initialize(Services);

                var crf = FindFirstObjectByType<CraftScreen>(FindObjectsInactive.Include);
                if (crf != null) crf.Initialize(Services);

                var mer = FindFirstObjectByType<MerchantScreen>(FindObjectsInactive.Include);
                if (mer != null) mer.Initialize(Services, inv);

                var dun = FindFirstObjectByType<DungeonScreen>(FindObjectsInactive.Include);
                if (dun != null) dun.Initialize(Services, chr);

                var que = FindFirstObjectByType<QuestScreen>(FindObjectsInactive.Include);
                if (que != null) que.Initialize(Services);

                var set = FindFirstObjectByType<SettingsScreen>(FindObjectsInactive.Include);
                if (set != null) set.Initialize(Services);

                if (popup != null) _ui.RegisterDialogScreen(popup);

                _ui.ShowScreen(UIScreenId.MainHUD);
                Debug.Log($"[UIRuntimeBootstrap] Wired {screens.Length} screen(s); MainHUD shown.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UIRuntimeBootstrap] Failed to wire UI: {ex.Message}\n{ex.StackTrace}");
                if (_errorPopup != null)
                {
                    _errorPopup.Initialize(RetryBoot, ResetData);
                    _errorPopup.ShowError("Startup Failed:\n" + ex.Message);
                }
                OnBootFailed?.Invoke(ex.Message);
            }
            finally
            {
                _isInitializing = false;
            }
        }

        public void RetryBoot()
        {
            Initialize();
        }

        public void ResetData()
        {
            // The actual UI should show a confirmation popup BEFORE calling this method.
            // When called, this deletes the save safely, creates a fresh one, and restarts.
            try
            {
                if (_save != null)
                {
                    _save.DeleteSave();
                }
                else
                {
                    // Fallback if Services failed to build
                    var tempSave = new SaveService();
                    tempSave.DeleteSave();
                }
                Initialize();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UIRuntimeBootstrap] Failed to reset data: {ex.Message}");
                OnBootFailed?.Invoke("Failed to reset data. Please restart the game.");
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
            int dropTables = 0;
            foreach (var kvp in report.loadedRecordsByCategory)
            {
                // Drop tables are a diagnostic counter, not a data file — keep them out of the total.
                if (kvp.Key == "enemy_drop_tables") { dropTables = kvp.Value; continue; }
                totalRecords += kvp.Value;
            }

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
                $"{report.loadedFiles}/{report.expectedFiles} file(s), {totalRecords} record(s). " +
                $"Enemy drop tables loaded: {dropTables}.");

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

        /// <summary>Any non-HUD screen with a descendant named "Btn_Back" (direct child, or inside a Header row) returns to the previous screen.</summary>
        private void WireBackButton(UIScreen screen)
        {
            if (screen.ScreenId == UIScreenId.MainHUD) return;

            Button backBtn = FindButtonDeep(screen.transform, "Btn_Back");
            if (backBtn != null)
            {
                backBtn.onClick.RemoveAllListeners();
                backBtn.onClick.AddListener(() => _ui.Back());
            }
        }

        private Button FindButtonDeep(Transform parent, string name)
        {
            if (parent.name == name && parent.TryGetComponent<Button>(out var btn)) return btn;
            for (int i = 0; i < parent.childCount; i++)
            {
                var res = FindButtonDeep(parent.GetChild(i), name);
                if (res != null) return res;
            }
            return null;
        }
    }
}
