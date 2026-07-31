# 10 — Screen Design Specifications

Each spec: Section | Specification table, plus a short ASCII wireframe reflecting that screen's real data/actions. Placeholder visual language only (flat colors/primitives per `08_Global_UI_Design_System.md`), no final art.

---
## Headquarters / HUD
| Section | Specification |
|---|---|
| Purpose | Always-visible currency + navigation root; also becomes the new "home" landing view |
| Backend systems | SaveData.Money/Gems (currency service in Section is de-facto SaveData itself) |
| Data required | Money, Gems, badges for: tavern visitor ready, craft/sell ready, quest claimable |
| Primary action | Navigate to a system |
| Secondary actions | none |
| Layout regions | Fixed top currency bar, fixed bottom nav bar (converted from current absolute-position stack), center "home" summary cards (new) |
| Component list | Currency chip ×2, Nav button ×9-11 (adds Pets, omits Raid/Shelter until backend ready) |
| Nav entry/exit | Root — no Back |
| State variants | normal; boot-failed (new — currently silent) |
| Empty state | n/a |
| Locked state | Pets/Promotion nav hidden until ready, per `07` |
| Loading state | New — boot currently has none |
| Success feedback | n/a |
| Failure feedback | New boot-error banner (today: silent `Debug.LogError` only, `UIRuntimeBootstrap.cs:113-116`) |
| Confirmation | n/a |
| Refresh triggers | Event-driven on any currency mutation (today: Show()-only, stale) |
| Save/offline implications | Shows Offline Summary popup once per session before first HUD paint (new) |
| Mobile behavior | Bottom-anchored nav, safe-area padded |
| Backend gaps | None blocking; badges depend on quest-creation fix to be meaningful |
| Acceptance criteria | Currency never stale by more than one mutation cycle; boot failure is visibly communicated, not silent |

```
+--------------------------------+
| Gold: 500      Gems: 0         |
+--------------------------------+
|   [Tavern●] [Roster] [Bag]     |
|   [Dungeon] [Workshop][Market] |
|   [Quest]   [Pets]   [Settings]|
+--------------------------------+
```

---
## Tavern
| Section | Specification |
|---|---|
| Purpose | Recruit adventurers, manage Quarters capacity |
| Backend | TavernService, CharacterService |
| Data required | Guest list (id/level), NextTavernVisit countdown, TavernCapacity, QuartersCapacity |
| Primary action | Recruit Selected |
| Secondary actions | Upgrade Quarters / Capacity / Time |
| Layout | Header, summary row (population, timer), tab bar (Recruit / Quarters — new split), card list, detail panel, action bar |
| Components | Guest card, upgrade row (3), live countdown bar (new — wires existing unused CreateProgressBar) |
| Nav | from HUD; no deep entry |
| State variants | guests available; tavern full (existing "Tavern full" text); roster full (recruit disabled) |
| Empty state | "No visitors right now — next in MM:SS" |
| Locked | n/a |
| Loading | n/a |
| Success feedback | typed text on recruit/upgrade |
| Failure feedback | reason-typed text (currently generic — fix in Phase 1) |
| Confirmation | Upgrade spend confirm (new, gated by `confirmupgrade` setting) |
| Refresh | live countdown (new — currently snapshot only) |
| Save/offline | Visitor generation already ticks offline via GameLoopService |
| Mobile | standard scroll list |
| Backend gaps | none blocking |
| Acceptance | Countdown always accurate to within 1s while screen open |

```
+--------------------------------+
| < Back      TAVERN             |
| Pop: 3/5   Next: 04:12 [====  ]|
| [Recruit] [Quarters]           |
+--------------------------------+
| [Guest: Footman Lv1]           |
| [Guest: Ranger Lv1]            |
+--------------------------------+
| Detail: Footman, trait: Brute  |
| [Recruit Selected]             |
+--------------------------------+
```

---
## Quarters (folded into Tavern tab, per IA)
Same screen as Tavern; separate tab shows QuartersCapacity, LevelQuarters, UpgradeQuarters price/level and roster headcount vs capacity. No separate spec table — governed by Tavern's spec above.

---
## Party
| Section | Specification |
|---|---|
| Purpose | Assign up to N adventurers to the active dungeon-run party |
| Backend | **New PartyService required** (currently UI-local only, CharacterScreen.cs:36) |
| Data required | Roster, current party slots, party size cap (new concept) |
| Primary action | Add/Remove from party |
| Secondary actions | Reorder (optional) |
| Layout | Roster list + party slot tray (persistent strip) |
| Components | Roster card, party slot chip |
| Nav | Tab within Adventurers screen |
| State variants | party full; party empty (blocks Dungeon start) |
| Empty | "No party assigned — Dungeon requires at least 1 member" |
| Locked | n/a |
| Loading | n/a |
| Success feedback | slot fills/empties visibly |
| Failure | "Party full" reason text |
| Confirmation | none needed (reversible) |
| Refresh | immediate on tap |
| Save/offline | **must persist** — today it does not |
| Mobile | drag-optional, tap-to-toggle sufficient |
| Backend gaps | BG (new PartyService + SaveData.Party field) — see `11` |
| Acceptance | Party membership survives app restart |

```
+--------------------------------+
| PARTY (2/4)                    |
| [Footman] [Ranger] [ + ] [ + ] |
+--------------------------------+
| ROSTER                         |
| [Footman Lv3] (in party)       |
| [Mage Lv1]    [Add to Party]   |
+--------------------------------+
```

---
## Adventurer Detail
| Section | Specification |
|---|---|
| Purpose | Full stat/equipment/trait view + equip actions for one adventurer |
| Backend | CharacterService.GetTotalStat, EquipmentService |
| Data required | Level, Exp, HP/maxHP, 6 stats w/ bonus breakdown, Weapon/Armor/Accessory, Trait (+effect if backend fixed), IsAscended/AscensionLevel |
| Primary action | Equip selected item |
| Secondary actions | Unequip ×3, Promote (once backend fixed) |
| Layout | Portrait header, stat block, equipment row, trait row, action bar |
| Components | Stat row w/ tooltip breakdown (new), equipment slot icon ×3 |
| Nav | from Roster or Party or Dungeon party card |
| State variants | fully-equipped; missing gear (shows "(none)" — keep) |
| Empty | n/a (always has a selected character to show this screen) |
| Locked | Promote button hidden until Promotion backend fixed |
| Loading | n/a |
| Success | typed text |
| Failure | typed text (fix Unequip's currently-discarded return value) |
| Confirmation | none for equip/unequip (reversible, low-risk) |
| Refresh | on any equip/unequip/level event |
| Save/offline | persists via CharacterSaveData |
| Mobile | single-column stacked |
| Backend gaps | trait-effect data for 7 inert traits; equipped-badge stub in Inventory |
| Acceptance | Every stat shown reflects GetTotalStat exactly (no separate hand-computed display) |

```
+--------------------------------+
| < Back   Footman  Lv 3         |
| HP 42/60  XP 120/300           |
| CON 12  DEX 8  DEF 10          |
| INT 3   MDEF 4  Immune 0       |
| Weapon: Iron Sword  [Unequip]  |
| Armor: (none)                  |
| Trait: Brute (+15% CON)        |
| [Equip Selected Item]          |
+--------------------------------+
```

---
## Inventory
| Section | Specification |
|---|---|
| Purpose | Browse/manage carried items |
| Backend | InventoryService |
| Data required | item id, stack, locked, equipped(fix stub), capacity |
| Primary action | Use / Equip(fix binding) |
| Secondary actions | Lock, Sell(redirect or real) |
| Layout | capacity bar, filter/category tabs (new), card list, detail, action bar |
| Components | item card w/ rarity badge, equipped pip (fix) |
| Nav | from HUD |
| State variants | full capacity (block add, explicit message) |
| Empty | "Inventory empty" + CTA to Dungeon/Merchant |
| Locked | n/a |
| Loading | n/a |
| Success | typed text |
| Failure | "Inventory full" explicit (today: exception thrown, not surfaced anywhere) |
| Confirmation | Sell confirmation if high rarity (new, `confirmswap`-style toggle) |
| Refresh | on any mutation; already synced from Merchant sell (good, keep) |
| Save/offline | via ItemSaveData |
| Mobile | virtualized list recommended (607 max items) |
| Backend gaps | Equip button unbound; Sell button unbound/placeholder; equipped-state stub |
| Acceptance | Equip reachable in 1 tap from this screen without switching to Character first |

```
+--------------------------------+
| < Back  INVENTORY   58/100     |
| [All][Weapon][Armor][Material] |
+--------------------------------+
| [Iron Sword x1] (equipped)     |
| [Health Potion x4]             |
+--------------------------------+
| Detail: Iron Sword, +5 ATK     |
| [Use] [Equip] [Lock] [Sell]    |
+--------------------------------+
```

---
## Storage (folded presentation inside Inventory, per IA — capacity strip only)
No separate screen; capacity number + upgrade entry point added to Inventory's header per spec above.

---
## Dungeon Selection
| Section | Specification |
|---|---|
| Purpose | Pick a dungeon + party, start a run |
| Backend | DungeonService, DungeonDefinition |
| Data required | dungeon id, ClearCount, BestTimeSeconds, lock reason (RequiredClearDungeonId) |
| Primary action | Start |
| Secondary actions | Prev/Next |
| Layout | grid of dungeon cards (existing GridLayoutGroup — keep), selected detail, party summary strip |
| Components | dungeon card w/ lock badge, party strip (from Party screen) |
| Nav | from HUD |
| State variants | locked (chain-gate; currently unfixable until BG dungeon-completion write lands) |
| Empty | n/a (11 dungeons always present) |
| Locked | show lock icon + "Clear {required} first" (currently would never unlock — flag) |
| Loading | n/a |
| Success | transitions to Active panel |
| Failure | n/a |
| Confirmation | **Start confirmation popup (new)** — party gets committed with zero warning today |
| Refresh | on selection change |
| Save/offline | new run creates ActiveDungeonSaveData |
| Mobile | grid scroll |
| Backend gaps | dungeon-completion write missing (blocks all chain-gating) |
| Acceptance | Player cannot start with an empty party (already gated) and sees a confirm step before committing party |

```
+--------------------------------+
| < Back   DUNGEONS               |
| [Forest] [Cave] [Ruins*locked] |
| [Desert] [Swamp] ...           |
+--------------------------------+
| Selected: Forest  Clears: 3    |
| Party: Footman, Ranger         |
| [Start Dungeon]                |
+--------------------------------+
```

---
## Active Dungeon / Combat Presentation
| Section | Specification |
|---|---|
| Purpose | Show live run state, allow Continue/Auto-Battle |
| Backend | DungeonService.Tick, CombatService |
| Data required | Turn/action text, enemy HP, party HP, progress (MaxProgress placeholder=0 — cosmetic-only until backend defines a real max) |
| Primary action | Auto-Battle toggle |
| Secondary actions | Continue (manual step) |
| Layout | enemy row, party row, action/turn banner, control bar |
| Components | HP bars (wire existing unused CreateProgressBar), non-interactive enemy/party cards (keep non-interactive, but style distinctly from real buttons) |
| Nav | replaces Select panel in place |
| State variants | fighting; searching; looting; defeated/respawning; fled |
| Empty | n/a |
| Locked | n/a |
| Loading | n/a |
| Success | n/a (continuous) |
| Failure | **Defeat/Flee summary (new)** — currently silent state transition |
| Confirmation | Auto-Battle-off before leaving screen mid-fight (new, optional) |
| Refresh | live via existing 0.5s coroutine (keep, it's the app's only good live-refresh precedent) |
| Save/offline | round-trips via SaveDungeonState/LoadDungeonState (confirmed solid) |
| Mobile | fits single screen, no scroll needed |
| Backend gaps | Status effects never applied (StatusEffectService orphaned) — combat outcomes are simpler than intended |
| Acceptance | HP always shown as current/max, not current-only |

```
+--------------------------------+
| Forest — Turn 4  [FIGHTING]    |
| Enemies: Wolf HP 12/20         |
| Party:  Footman HP 40/60       |
|         Ranger  HP 22/30       |
| [Continue] [Auto-Battle: ON]   |
+--------------------------------+
```

---
## Loot Chest
| Section | Specification |
|---|---|
| Purpose | Show pending drops, collect into inventory |
| Backend | LootService, DungeonService.CollectDrops |
| Data required | PendingDrops list |
| Primary action | Collect All |
| Secondary actions | none |
| Layout | reward card grid, collect button |
| Components | reward card w/ rarity badge |
| Nav | auto-shown when loot pending |
| State variants | chest full (2000/3000 cap) |
| Empty | n/a (only shown when drops exist) |
| Locked | n/a |
| Loading | n/a |
| Success | "Collected N items" |
| Failure | **"M items lost — inventory full" (new)** — today silently drops overflow |
| Confirmation | none |
| Refresh | n/a (one-shot) |
| Save/offline | drops persisted in ActiveDungeonSaveData until collected |
| Mobile | grid, scrollable if many drops |
| Backend gaps | CollectDrops doesn't report per-item failure detail |
| Acceptance | Every dropped item is either shown in inventory or explicitly reported as lost |

---
## Quest
| Section | Specification |
|---|---|
| Purpose | Track quest progress, claim rewards |
| Backend | QuestService |
| Data required | Progress/Target/State per quest, doctrine target (full 8, not 3) |
| Primary action | Claim Reward |
| Secondary actions | pick doctrine target |
| Layout | list, detail w/ real progress bar (wire existing component), doctrine picker (new, dropdown-style for 8 options) |
| Components | quest card, progress bar |
| Nav | from HUD |
| State variants | in-progress; completed(claimable); (Locked/RewardClaimed states exist in enum but are dead code today — do not build UI for unreachable states) |
| Empty | **likely permanent empty state today** since no quest-creation path exists — must show an honest "No active quests" rather than an infinite blank list, and this is flagged as BLOCKED on backend (`11`) |
| Locked | n/a |
| Loading | n/a |
| Success | real result-gated feedback (fix discarded return value) |
| Failure | typed |
| Confirmation | none |
| Refresh | on claim |
| Save/offline | via QuestSaveData |
| Mobile | list+detail |
| Backend gaps | **BLOCKER**: no quest-creation method exists anywhere — this screen may be permanently empty until fixed |
| Acceptance | Claim feedback always reflects actual ClaimReward result |

---
## Doctrine (new overview screen)
| Section | Specification |
|---|---|
| Purpose | Show all 8 doctrine levels/progress |
| Backend | DoctrineService |
| Data required | 8×(Level,Progress), stars-to-next-level formula |
| Primary action | none (view-only; progress comes from quest claims) |
| Secondary actions | none |
| Layout | 8-row list, each with a progress bar |
| Components | doctrine row |
| Nav | tab within Quest screen |
| State variants | maxed (DoctrineMaxed flag — currently dead, never set true; fix before showing a "MAXED" badge) |
| Empty | n/a |
| Locked | n/a |
| Loading | n/a |
| Success/Failure | n/a (no actions) |
| Confirmation | n/a |
| Refresh | on quest claim |
| Save/offline | already persisted |
| Mobile | simple list |
| Backend gaps | DoctrineMaxed never set — fix before shipping a maxed-state badge |
| Acceptance | All 8 doctrines visible, not just the 3 currently exposed via QuestScreen's cycler |

---
## Workshop / Craft
| Section | Specification |
|---|---|
| Purpose | Craft items from recipes, manage queue |
| Backend | CraftService |
| Data required | Recipe list w/ **real** ingredient availability (fix hardcoded 999), queue w/ live timer, completed list |
| Primary action | Craft Selected |
| Secondary actions | Claim, Upgrade Queue |
| Layout | existing 3-tab structure (keep) |
| Components | recipe card w/ accurate checklist, queue card w/ live progress bar |
| Nav | from HUD |
| State variants | recipe stub (empty ingredients — many of 321 recipes) must show "Recipe data incomplete" rather than a craftable-looking card |
| Empty | "No recipes available" |
| Locked | n/a |
| Loading | n/a |
| Success | typed (already handles CraftResult.FailureReason well — keep) |
| Failure | typed (keep) |
| Confirmation | Upgrade spend confirm (new) |
| Refresh | live queue timer (new — currently manual only) |
| Save/offline | offline-ticks via GameLoopService |
| Mobile | list, 321-item scroll — virtualize |
| Backend gaps | fake ingredient-availability stub is the top priority fix in this screen; craft duration ignores GetSecondsToCraft formula |
| Acceptance | Ingredient checklist matches actual inventory contents 1:1, always |

---
## Recipe Detail
Sub-panel of Workshop (existing detail-panel pattern) — same screen, no separate class needed. Spec covered above.

---
## Merchant
| Section | Specification |
|---|---|
| Purpose | Buy from rotating stock |
| Backend | MerchantService |
| Data required | Regular/Special stock (currently always empty — BG) |
| Primary action | Buy Selected |
| Secondary actions | none on this tab |
| Layout | existing Buy tab (keep) |
| Components | offer card |
| Nav | from HUD |
| State variants | **stock empty is the default/only reachable state today** — must show an honest "Merchant has nothing today" rather than looking broken |
| Empty | as above |
| Locked | n/a |
| Loading | n/a |
| Success | typed |
| Failure | typed |
| Confirmation | none (reversible only via re-buy, low risk) |
| Refresh | on buy |
| Save/offline | n/a |
| Mobile | list |
| Backend gaps | **BLOCKER**: stock lists never populated by any writer |
| Acceptance | Buy tab never silently looks broken — empty state is explicit |

---
## Market (Merchant → Listings tab)
| Section | Specification |
|---|---|
| Purpose | Sell items, track pending sales, claim proceeds |
| Backend | MerchantService.SellItem/ProgressMarket/ClaimSoldItem |
| Data required | active + sold listings, live sell timer |
| Primary action | Claim Sold |
| Secondary actions | Sell (from Inventory selection) |
| Layout | existing Listings tab (keep) |
| Components | listing card w/ live countdown (new) |
| Nav | tab within Merchant |
| State variants | pending; sold-ready |
| Empty | "No active listings" |
| Locked | n/a |
| Loading | n/a |
| Success | typed (keep, already good — explicitly refreshes Inventory on sell) |
| Failure | typed |
| Confirmation | Sell confirm for high-value items (new) |
| Refresh | live countdown (new) |
| Save/offline | offline-ticks via GameLoopService |
| Mobile | list |
| Backend gaps | sell duration ignores GetSecondsToSell formula |
| Acceptance | Countdown accurate; claim always credits the exact computed price |

---
## Shop
No separate screen — Merchant IS Shop per backend folding (`01`). No spec needed beyond Merchant above.

---
## Raid Selection / Active Raid
| Section | Specification |
|---|---|
| Purpose | N/A until backend exists |
| Backend | **None** — RaidDefinition is an empty data-only class, no RaidService |
| Recommendation | **Do not build this screen in the UI phases below.** Design placeholder only; implementation blocked on backend work outside this plan's scope. |

---
## Pets
| Section | Specification |
|---|---|
| Purpose | View, acquire, assign, release pets |
| Backend | PetService (fully implemented — highest-readiness net-new screen) |
| Data required | Level/Exp, EquippedToCharacterId, stat bonuses (Attack/Defense/Hp/Speed) |
| Primary action | Equip to character |
| Secondary actions | Unequip, view bonus breakdown |
| Layout | pet roster list, detail w/ bonus breakdown, character-assignment picker |
| Components | pet card, assignment picker (reuses Roster card component) |
| Nav | new primary from HUD |
| State variants | unassigned; assigned; (no "acquire" method found in PetService beyond CreatePet — verify calling convention before wiring) |
| Empty | "No pets yet" |
| Locked | n/a |
| Loading | n/a |
| Success | typed |
| Failure | typed (add exclusivity guard first — see BG on double-equip) |
| Confirmation | none |
| Refresh | on equip/level |
| Save/offline | via PetSaveData |
| Mobile | list+detail |
| Backend gaps | no exclusivity guard preventing one pet equipped to two characters simultaneously |
| Acceptance | Equip/unequip always reflected immediately in both Pets screen and the target Adventurer Detail screen |

---
## Shelter
| Section | Specification |
|---|---|
| Purpose | N/A until backend exists |
| Backend | **None** — only dead SaveData fields + unused formulas |
| Recommendation | **Do not build.** Blocked on backend. |

---
## Promotion / Ascension
| Section | Specification |
|---|---|
| Purpose | Preview and confirm tier promotion for an adventurer |
| Backend | PromotionService (code complete, but **data-registration gap makes it unreachable** — must fix `DatabaseBuilder` + add `promotions.json` first) |
| Data required | Available promotions (RequiredLevel, RequiredItemId/Count, StatMultiplier, TierName) |
| Primary action | Promote |
| Secondary actions | none |
| Layout | eligibility list, confirm panel |
| Components | promotion tier card |
| Nav | secondary, from Adventurer Detail |
| State variants | ineligible (level/item gate) |
| Empty | "No promotions available for this tier" |
| Locked | show item/level requirement explicitly |
| Loading | n/a |
| Success | typed, shows new stat preview |
| Failure | typed (missing item/level) |
| Confirmation | **required (irreversible resets Level/Exp to 1/0)** |
| Refresh | on promote |
| Save/offline | via CharacterSaveData.AscensionLevel |
| Mobile | list+confirm |
| Backend gaps | **BLOCKER**: not registered in DatabaseBuilder; also the double-multiplier stacking risk noted in `01` needs a design decision before shipping |
| Acceptance | Promote is unreachable in UI until backend gap is fixed — do not ship a dead-end screen |

---
## Settings
| Section | Specification |
|---|---|
| Purpose | Toggle preferences, reset save |
| Backend | SettingsService (best-behaved existing screen — keep pattern) |
| Data required | 5 toggles, language, version |
| Primary action | Save |
| Secondary actions | Toggle ×5 (add Cloud button — currently display-only), Reset |
| Layout | existing (keep, no card list) |
| Components | toggle row ×5, 2-step reset confirm (already exists — reuse as the ConfirmPopup template) |
| Nav | from HUD |
| State variants | pending-unsaved (new — surface "unsaved changes" since toggles aren't auto-persisted) |
| Empty | n/a |
| Locked | n/a |
| Loading | n/a |
| Success | typed (keep) |
| Failure | n/a |
| Confirmation | Reset already has 2-step (keep as the reference implementation for the new global ConfirmPopup) |
| Refresh | n/a |
| Save/offline | manual Save required — add unsaved-indicator |
| Mobile | simple list |
| Backend gaps | Cloud toggle has no control; Reset-vs-fresh-save default divergence (Notifications/Cloud) |
| Acceptance | Every visible toggle is interactive; unsaved state is visible |

---
## Offline Summary (new)
| Section | Specification |
|---|---|
| Purpose | Report what happened while the player was away |
| Backend | Requires resolving the GameLoopService vs OfflineProgressService ambiguity first (`11`) |
| Data required | DeltaSeconds, resources gained (craft/sell/dungeon results) |
| Primary action | Dismiss/Collect |
| Secondary actions | none |
| Layout | full-screen popup shown once at session start before HUD |
| Components | summary rows (currency gained, items crafted/sold, dungeon progress) |
| Nav | auto-shown, dismiss returns to HUD |
| State variants | no offline time (skip popup); capped at 12h (show "capped" note) |
| Empty | skip popup entirely if DeltaSeconds below a minimum threshold |
| Locked | n/a |
| Loading | n/a |
| Success | n/a |
| Failure | n/a |
| Confirmation | n/a (dismiss only) |
| Refresh | one-shot |
| Save/offline | consumes OfflineProgressResult |
| Mobile | full-screen modal |
| Backend gaps | **BLOCKER**: must first resolve which offline-progress implementation is canonical |
| Acceptance | Never shown when DeltaSeconds is trivially small; never shown twice for the same session |

---
## Generic Confirm / Error / Reward Popup
| Section | Specification |
|---|---|
| Purpose | Reusable Yes/No confirm, error toast, and reward-reveal components |
| Backend | none directly — UI infrastructure |
| Data required | title, message, 2 callbacks (confirm) |
| Primary action | Confirm / Dismiss |
| Secondary actions | Cancel |
| Layout | centered modal, single popup slot (keep existing `UIService._currentPopup` single-slot design) |
| Components | ConfirmPopup (new, extends beyond today's OK-only PopupScreen), ErrorToast (distinct color/icon from info, today identical), RewardPopup (new) |
| Nav | invoked from any screen |
| State variants | confirm/error/reward all share the modal shell, differ by content+button count |
| Empty | n/a |
| Locked | n/a |
| Loading | n/a |
| Success/Failure | n/a (these ARE the feedback mechanism) |
| Confirmation | ConfirmPopup itself has no further nesting |
| Refresh | n/a |
| Save/offline | n/a |
| Mobile | centered, safe-area padded |
| Backend gaps | none |
| Acceptance | Every irreversible/spend action identified across `03`/`06` routes through ConfirmPopup, gated by existing-but-unused Settings toggles `confirmswap`/`confirmupgrade`/`confirmretreat` |
