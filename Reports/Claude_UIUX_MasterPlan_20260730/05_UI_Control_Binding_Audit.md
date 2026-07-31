# 05 — UI Control Binding Audit

Status values restricted to: BOUND_AND_COMPLETE, BOUND_BUT_INCOMPLETE, BOUND_TO_WRONG_METHOD, PRESENT_NOT_BOUND, MISSING, PLACEHOLDER, RUNTIME_PENDING.

| Screen | Control | Field/GameObject | Listener | Backend method | Return value handled | State gating | Refresh | Success feedback | Failure feedback | Status | Evidence |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Tavern | Btn_RecruitSelected | `_recruitButton` | OnClickRecruitSelected | `RecruitGuest` | Yes (bool) | interactable when guest selected & room | Manual | text | text (generic, not reason-typed) | BOUND_BUT_INCOMPLETE | TavernScreen.cs:182-201 |
| Tavern | Btn_UpgradeQuarters/Capacity/Time | 3 buttons | 3 handlers | `Upgrade*` | Yes (bool) | interactable if affordable (assumed, RUNTIME_VERIFICATION_REQUIRED) | Manual | text | text | BOUND_AND_COMPLETE | TavernScreen.cs:203-254 |
| Character | Btn_AddParty/RemoveParty | 2 buttons | 2 handlers | **none — local HashSet only** | n/a | none | Manual | text (misleading — implies persistence) | n/a | BOUND_TO_WRONG_METHOD (bound to UI-only state, not a backend action) | CharacterScreen.cs:200-216 |
| Character | Btn_Equip | `_equipButton` | OnClickEquipSelectedItem | `EquipmentService.Equip` | Yes (bool `done`) | pulls selection from InventoryScreen | Manual | text | text | BOUND_AND_COMPLETE | CharacterScreen.cs:218-231 |
| Character | Btn_UnWpn/UnArm/UnAcc | 3 buttons | 3 handlers | `EquipmentService.Unequip` | **No — return discarded** | none | Manual | unconditional "Unequipped {slot}." even if no-op | none | BOUND_BUT_INCOMPLETE | CharacterScreen.cs:232-242 |
| Character | Btn_Prev/Next (select) | none built | `OnClickSelectNext/Previous` exist in code | n/a | n/a | n/a | n/a | n/a | n/a | PRESENT_NOT_BOUND (method exists, no button created) | GuildMasterUnifiedApply.cs:241-271 |
| Inventory | Btn_Lock | `_lockButton` | toggle handler | `ToggleLockItem` | Yes (bool) | selection required | Manual | label updates | none | BOUND_AND_COMPLETE | InventoryScreen.cs |
| Inventory | Btn_Use | `_useButton` | OnClickUse | `UseConsumable` | Yes (bool) | selection + consumable required | Manual | text | text | BOUND_AND_COMPLETE (though backend logic itself is hardcoded +50 HP — flagged separately) | InventoryService.cs:173-188 |
| Inventory | Btn_Equip | `_equipButton` field | **no method exists, no GameObject built** | — | — | — | — | — | — | MISSING | InventoryScreen.cs:27; GuildMasterUnifiedApply.cs:273-299 |
| Inventory | Btn_Sell | `_sellButton` field | `OnClickSellSelected` exists but does not call any service (redirect text only) and the button itself is never built/bound | — | — | — | — | — | — | PRESENT_NOT_BOUND (method exists but unreachable; even if reachable it's a placeholder redirect, not a real sell) | InventoryScreen.cs:192-198 |
| Craft | Tab_Recipes/Queue/Completed | 3 tab buttons | tab switch handlers | n/a (UI state) | n/a | n/a | Manual | n/a | n/a | BOUND_AND_COMPLETE | CraftScreen.cs |
| Craft | Ingredient checklist rows | detail panel text | n/a (display only) | **`currentWeightOfItem` hardcoded to 999** | n/a | always shows "available" | Manual | n/a | n/a | PLACEHOLDER (materially misleading data) | CraftScreen.cs:214-218 |
| Craft | Btn_Craft | `_craftButton` | OnClickCraftSelected | `TryStartCraft` | Yes (`.Success`/`.FailureReason`) | selection required | Manual | text | typed reason text | BOUND_AND_COMPLETE | CraftScreen.cs:333-350 |
| Craft | Btn_Claim | `_claimButton` | OnClickClaimSelected | `ClaimCompletedCraft` | Yes (bool) | completed item selected | Manual | text | text | BOUND_AND_COMPLETE | CraftScreen.cs:354-370 |
| Craft | Btn_UpgradeQueue | `_upgradeQueueButton` | OnClickUpgradeQueue | `UpgradeQueueCapacity` | Yes (bool) | affordability (RUNTIME_VERIFICATION_REQUIRED) | Manual | text | text | BOUND_AND_COMPLETE | CraftScreen.cs:374-385 |
| Dungeon | Btn_Start | `_startButton` | OnClickStartSelected | `StartDungeon` | **No — success text shown unconditionally once preconditions met, not gated on actual call result** | dungeon+party selected | Manual | unconditional text | none | BOUND_BUT_INCOMPLETE | DungeonScreen.cs:263-278 |
| Dungeon | Btn_Continue | `_continueButton` | OnClickContinue | `Tick` | No return value on interface | active run | Manual | none explicit | none | BOUND_AND_COMPLETE (method has no bool to check, by design) | DungeonScreen.cs:280 |
| Dungeon | Btn_AutoBattle | `_autoBattleButton` | OnClickToggleAutoBattle | starts coroutine calling `Tick()` every 0.5s | n/a | active run | **Live** (only auto-refreshing control in the app) | label toggles ON/OFF | n/a | BOUND_AND_COMPLETE | DungeonScreen.cs:286-328 |
| Dungeon | Btn_Collect | `_collectLootButton` | OnClickCollectLoot | `CollectDrops` | Yes (int collected) | loot pending | Manual | text with count | n/a (no partial-fail detail) | BOUND_BUT_INCOMPLETE | DungeonScreen.cs:340-343 |
| Dungeon | Btn_PrevDungeon/Next | 2 buttons | handlers | n/a (selection) | n/a | n/a | Manual | n/a | n/a | BOUND_AND_COMPLETE | DungeonScreen.cs:250-259; confirmed present Main.unity:1680,8824 |
| Merchant | Btn_BuyRegular/Special | 2 buttons | handlers | `BuyOffer` | Yes (bool) | offer selected, affordable, room | Manual | text | text | BOUND_AND_COMPLETE (contingent on stock ever being populated — backend gap) | MerchantScreen.cs:340-358 |
| Merchant | Btn_Sell | `_sellButton` | OnClickSellSelected | `SellItem` | Yes (`.Success`/`.FailureReason`) | item selected via InventoryScreen | Manual, also refreshes InventoryScreen | text | typed reason text | BOUND_AND_COMPLETE | MerchantScreen.cs:360-380 |
| Merchant | Btn_ClaimSold | `_claimSoldButton` | OnClickClaimSold | `ClaimSoldItem` | Yes (bool) | item sold | Manual | text | text | BOUND_AND_COMPLETE | MerchantScreen.cs:382-401 |
| Quest | Btn_Claim | `_claimButton` | OnClickClaimSelected | `ClaimReward` | **No — return discarded, success shown whenever State==Completed regardless of actual call outcome** | quest completed | Manual | unconditional text | none | BOUND_BUT_INCOMPLETE (clearest ignored-return-value instance in the app) | QuestScreen.cs:146-162 |
| Quest | Btn_CycleDoctrine | `_cycleDoctrineButton` | OnClickCycleDoctrine | n/a (local cycle of 3/8 strings) | n/a | n/a | Manual | label updates | n/a | BOUND_TO_WRONG_METHOD-adjacent (functions, but only exposes 3 of 8 doctrines — incomplete coverage of backend capability) | QuestScreen.cs:29,81-85 |
| Settings | 4 toggle buttons | fields | handlers | `SetToggle` | n/a (no fail path) | n/a | Manual | text | n/a | BOUND_AND_COMPLETE | SettingsScreen.cs:98-132 |
| Settings | Cloud toggle | displayed text only | **no button** | n/a | n/a | n/a | n/a | n/a | n/a | PRESENT_NOT_BOUND (value read and shown, no control to change it) | SettingsScreen.cs:65-75 |
| Settings | Btn_Save | `_saveButton` | OnClickSave | `SaveCurrentState` | implicit | n/a | Manual | text | n/a | BOUND_AND_COMPLETE | SettingsScreen.cs:136-141 |
| Settings | Btn_Reset → Confirm/Cancel | 3 buttons | 2-step handlers | `ResetToDefault` | implicit | pending-reset gate | Manual | warning text, then confirm | n/a | BOUND_AND_COMPLETE (only real confirm flow in the app) | SettingsScreen.cs:144-166 |
| Popup | Btn_OK | `_okButton` | Awake() wiring | `Hide()` only | n/a | n/a | n/a | n/a | n/a | BOUND_AND_COMPLETE (but PLACEHOLDER as a class — no Yes/No variant exists anywhere) | PopupScreen.cs:13-19 |
| HUD | 8 nav buttons | fields | `BindButtons()` | `ShowScreen(X)` | n/a | n/a | n/a | n/a | n/a | BOUND_AND_COMPLETE | HUDController.cs:35-45 |

Cross-cutting binding-layer issues affecting every screen above:
- `UIService.ShowScreen` on an unregistered id: silent `Debug.LogWarning`, no player feedback (`UIService.cs:19-39`).
- `UIService.Back()` at stack depth ≤1: silent no-op (`UIService.cs:49-61`).
- `UIService.ShowInfo/ShowError/ShowDeferred` with no dialog registered: message is lost, only a log warning (`UIService.cs:98-125`).
- Card lists built by `UICardFactory.CreateCard` with `isInteractable:false` render as plain `Button` components that never receive a listener — visually similar to a disabled button but functionally just inert display, confirmed on Dungeon's enemy/party/queue cards and Craft's non-actionable list rows (`UICardFactory.cs:70-139`).

Full raw per-control trace: `evidence/ui_trace_raw.md`.
