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
                new DatabaseBuilder(provider, serializer, db).Build();

                var formula = new FormulaService();
                var save = new SaveService();
                save.Load(out _);

                var factory = new RuntimeFactory(new DefaultInstanceIdGenerator());
                var itemService = new ItemService(factory, db);
                var inventory = new InventoryService(save, formula, itemService, db);

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
                if (chr != null) chr.Initialize(save);

                if (popup != null) _ui.RegisterDialogScreen(popup);

                _ui.ShowScreen(UIScreenId.MainHUD);
                Debug.Log($"[UIRuntimeBootstrap] Wired {screens.Length} screen(s); MainHUD shown.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UIRuntimeBootstrap] Failed to wire UI: {ex.Message}\n{ex.StackTrace}");
            }
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
