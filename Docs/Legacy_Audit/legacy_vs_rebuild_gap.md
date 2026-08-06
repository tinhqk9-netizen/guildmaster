# Legacy vs Rebuild — Gap Analysis

> Compares legacy screen inventory against Unity rebuild project.
> Status as of 2026-08-04.

---

## Rebuild UIScreenId Enum (Current)

```csharp
None, Loading, MainHUD, MainMenu, Inventory, Character,
Dungeon, Craft, Merchant, Settings, Tavern, Quest
```

Source: `Assets/_Game/Scripts/Runtime/UI/UIScreenId.cs`

---

## Screen Mapping

| # | Legacy Screen | Rebuild Screen | Status | Notes |
|---|--------------|----------------|--------|-------|
| **Tab Fragments** | | | | |
| 1 | `HeadquartersFragment` | `MainHUD` (partial) | 🟡 PARTIAL | Rebuild has building cards but layout/navigation differs |
| 2 | `AdventurersFragment` | `Character` (partial) | 🟡 PARTIAL | Rebuild has character list but missing adventurer detail |
| 3 | `DungeonsFragment` | `Dungeon` (partial) | 🟡 PARTIAL | Rebuild has dungeon screen |
| 4 | `RaidsFragment` | — | 🔴 MISSING | No raids tab in rebuild |
| **HUD** | | | | |
| 5 | Top bar (menu, title, gems) | `MainHUD` | 🟡 PARTIAL | Rebuild has HUD but layout structure differs |
| 6 | Currency bar (gold/silver/copper) | `MainHUD` | 🟡 PARTIAL | Rebuild has currency display |
| 7 | Tooltip icons row | — | 🔴 MISSING | Shop/merchant/quest/ad icons not in rebuild HUD |
| 8 | Tutorial section | — | 🔴 MISSING | No tutorial system |
| 9 | Bottom navigation (4 tabs) | Sidebar/tabs | 🟡 PARTIAL | Different navigation pattern |
| 10 | Drawer menu (10 items) | — | 🔴 MISSING | No drawer/hamburger menu |
| **HQ Sub-Dialogs** | | | | |
| 11 | `DialogQuarters` | — | 🔴 MISSING | |
| 12 | `DialogTavern` | `Tavern` | 🟢 EXISTS | Rebuild has tavern screen |
| 13 | `DialogStorage` | `Inventory` | 🟡 PARTIAL | Rebuild has inventory but may differ |
| 14 | `DialogMarket` | — | 🔴 MISSING | |
| 15 | `DialogWorkshop` | `Craft` | 🟡 PARTIAL | Rebuild has craft screen |
| 16 | `DialogShelter` | — | 🔴 MISSING | |
| **Character Dialogs** | | | | |
| 17 | `DialogEntityDetail` | `Character` | 🟡 PARTIAL | Rebuild has character view |
| 18 | `DialogSelectEquipment` | — | 🔴 MISSING | |
| 19 | `DialogItemDetail` | — | 🔴 MISSING | |
| 20 | `DialogPromotionChoices` | — | 🔴 MISSING | |
| 21 | `DialogChooseAdventurer` | — | 🔴 MISSING | |
| 22 | `DialogChangeTraitRare` | — | 🔴 MISSING | |
| 23 | `DialogRecallAdventurers` | — | 🔴 MISSING | |
| **Combat Dialogs** | | | | |
| 24 | `DialogDungeonDetail` | — | 🔴 MISSING | |
| 25 | `DialogSendTeam` | — | 🔴 MISSING | |
| 26 | `DialogCollectDrops` | — | 🔴 MISSING | |
| 27 | `DialogReport` | — | 🔴 MISSING | |
| 28 | `DialogIdleProgress` | — | 🔴 MISSING | |
| **Pet Dialogs** | | | | |
| 29 | `DialogPetDetail` | — | 🔴 MISSING | |
| 30 | `DialogChoosePet` | — | 🔴 MISSING | |
| 31 | `DialogMergePet` | — | 🔴 MISSING | |
| **Consumption Dialogs** | | | | |
| 32-37 | 6× Consume dialogs | — | 🔴 MISSING | |
| **Commerce Dialogs** | | | | |
| 38 | `DialogMerchant` | `Merchant` | 🟢 EXISTS | |
| 39 | `DialogBuyFromMerchant` | — | 🔴 MISSING | |
| 40 | `DialogSell` | — | 🔴 MISSING | |
| 41 | `DialogShop` | — | 🔴 MISSING | |
| **Crafting Dialogs** | | | | |
| 42 | `DialogCraft` | `Craft` | 🟡 PARTIAL | |
| 43 | `DialogRecipes` | — | 🔴 MISSING | |
| **Doctrine Dialogs** | | | | |
| 44-46 | 3× Doctrine dialogs | — | 🔴 MISSING | |
| **Quest Dialogs** | | | | |
| 47 | `DialogQuests` | `Quest` | 🟢 EXISTS | |
| 48-49 | Quest refresh/refill | — | 🔴 MISSING | |
| **System Dialogs** | | | | |
| 50 | `DialogSettings` | `Settings` | 🟢 EXISTS | |
| 51-55 | FAQ, Bestiary, Messages, Redeem, etc. | — | 🔴 MISSING | |

---

## Summary

| Status | Count | % |
|--------|-------|---|
| 🟢 EXISTS | 4 | 7% |
| 🟡 PARTIAL | 9 | 16% |
| 🔴 MISSING | 42 | 76% |
| **Total** | **55** | **100%** |

### Rebuild-Only (Not in Legacy)
| Rebuild Screen | Notes |
|---------------|-------|
| `Loading` | Not a separate screen in legacy |
| `MainMenu` | Legacy goes straight to Headquarters |

---

## Critical Gaps

1. **Raids tab** — entire feature missing
2. **Pet system UI** — 3 dialogs, no rebuild equivalent
3. **Doctrine/class system** — 3 dialogs, no rebuild equivalent
4. **Consumption system** — 6 dialogs for potions/food
5. **HUD tooltip icons** — shop/merchant/quest shortcuts
6. **Drawer menu** — settings/FAQ/bestiary/achievements access
7. **Tutorial system** — 7-step guided tutorial
8. **Idle progress dialog** — shows what happened while player was away
