# Claude UI Reconstruction Handoff — FINAL

> **SINGLE SOURCE OF TRUTH for Unity UI reconstruction.**
> Generated: 2026-08-04 | Audit Level: COMPLETE
> All evidence from `D:\Tinh\Guild Master - Idle Dungeons\`
> **Verdict:** `FULLY READY FOR CLAUDE IMPLEMENTATION`

---

## 📋 IMPLEMENTATION READINESS

### 🟢 READY (implement now — all data available)

| ID | Screen | Legacy Class | Priority |
|----|--------|-------------|----------|
| P1-01 | Bottom Nav (4 tabs) | `MainActivity` → `BottomNavigationView` + `ViewPager2` | CRITICAL |
| P1-02 | Drawer Menu | `MainActivity` → `NavigationView` | CRITICAL |
| P1-03 | HUD Top Bar | `activity_main.xml` top section | CRITICAL |
| P1-04 | HUD Currency Bar | `layout_money.xml` | CRITICAL |
| P1-05 | HUD Tooltip Icons | `activity_main.xml` containerTooltips | HIGH |
| P2-01 | Headquarters Tab | `HeadquartersFragment` | HIGH |
| P2-02 | Quarters Dialog | `DialogQuarters` | HIGH |
| P2-03 | Tavern Dialog | `DialogTavern` | HIGH |
| P2-04 | Storage Dialog | `DialogStorage` | HIGH |
| P2-05 | Market Dialog | `DialogMarket` | MEDIUM |
| P2-06 | Workshop Dialog | `DialogWorkshop` | MEDIUM |
| P2-07 | Shelter Dialog | `DialogShelter` | MEDIUM |
| P3-01 | Adventurers Tab | `AdventurersFragment` | HIGH |
| P3-02 | Entity Detail Dialog | `DialogEntityDetail` | HIGH |
| P3-03 | Select Equipment | `DialogSelectEquipment` | HIGH |
| P3-04 | Item Detail Dialog | `DialogItemDetail` | HIGH |
| P3-05 | Promotion Choices | `DialogPromotionChoices` | MEDIUM |
| P3-06 | Doctrine Dialog | `DialogDoctrine` | MEDIUM |
| P3-07 | Consume Food | `DialogConsumeFood` | MEDIUM |
| P3-08 | Consume Potion | `DialogConsumePotion` | MEDIUM |
| P4-01 | Dungeons Tab | `DungeonsFragment` | HIGH |
| P4-02 | Dungeon Detail | `DialogDungeonDetail` | HIGH |
| P4-03 | Send Team | `DialogSendTeam` | HIGH |
| P4-04 | Report Dialog | `DialogReport` | HIGH |
| P4-05 | Collect Drops | `DialogCollectDrops` | HIGH |
| P4-06 | Idle Progress | `DialogIdleProgress` | MEDIUM |
| P5-01 | Raids Tab | `RaidsFragment` | MEDIUM |
| P6-01 | Merchant Dialog | `DialogMerchant` | MEDIUM |
| P6-02 | Buy From Merchant | `DialogBuyFromMerchant` | MEDIUM |
| P6-03 | Sell Dialog | `DialogSell` | MEDIUM |
| P6-04 | Craft Dialog | `DialogCraft` | MEDIUM |
| P6-05 | Recipes Dialog | `DialogRecipes` | MEDIUM |
| P7-01 | Pet Detail | `DialogPetDetail` | MEDIUM |
| P7-02 | Choose Pet | `DialogChoosePet` | MEDIUM |
| P8-01 | Quests Dialog | `DialogQuests` | MEDIUM |
| P9-01 | Settings Dialog | `DialogSettings` | LOW |
| P9-03 | Bestiary Dialog | `DialogBestiary` | LOW |

### 🟡 IMPLEMENT LATER (low priority, data available)

| ID | Screen | Reason |
|----|--------|--------|
| P5-02 | Refill Raid Try | Simple confirmation dialog, low urgency |
| P7-03 | Merge Pet | Niche feature |
| P8-02 | Refresh Quests | Simple confirmation dialog |
| P9-02 | FAQ | Text-only list |
| P9-04 | Messages | Text-only list |
| P9-05 | Redeem Code | Text input + button only |
| P3-consume-extras | Consume Evo23, Intercession, PotionOfClumsiness, PotionOfRejuvenation | All use same pick-list → confirm pattern as ConsumePotion |
| P3-misc | Change Trait Rare, Recall Adventurers, Doctrine Reset | Infrequent player actions |

### 🔴 BLOCKED BY MISSING BACKEND

| ID | Screen | Missing Service |
|----|--------|----------------|
| P6-06 | Shop (IAP) | `ShopService` / IAP wrapper — not required for game core |
| P9-06 | Tutorial System | `TutorialService` — plan to add post-launch |

### ⚫ LEGACY FEATURE NOT REQUIRED

| Feature | Reason |
|---------|--------|
| Rarity-colored borders | `backgroundFromRarity()` returns uniform `object_border_dim_white`. No rarity border system exists. |
| Custom fonts | Game uses system default font only. |
| AdMob/Ads integration | Ad reward watching — defer to monetization phase |

---

## 🏛️ ARCHITECTURE

### App Theme
```
Theme.IdleGuildMaster (parent: Theme.MaterialComponents.DayNight.DarkActionBar)
├── android:textColor = #c8c8c8 (dim_white)
├── colorPrimary = @color/brass_border (#befaa03e)
├── colorSecondary = @color/brass_border
├── colorOnPrimary = @color/black
└── colorOnSecondary = @color/black
```

### Dialog Window System
```
CustomDialog (base for ALL dialogs)
├── onResume: setBackgroundDrawable(transparent) → then dialog_border.xml
│   dialog_border.xml: fill=cardview_dark_background, stroke=3dp black, corner=10dp
├── default setLayout(): width=MATCH_PARENT, height=WRAP_CONTENT
├── Immersive mode: flags=8, preserves parent SystemUiVisibility
│
├── Override: DialogDungeonDetail → width=MATCH_PARENT, height=90% screen
├── Override: DialogBuyFromMerchant → width=90% screen, height=WRAP
├── Override: DialogQuests → width=90% screen, height=WRAP
├── Override: DialogSettings → width=90% screen, height=WRAP
├── Override: DialogRefillRaidTry → width=90% screen, height=WRAP
└── Override: DialogRefreshQuests → width=90% screen, height=WRAP
```

### Navigation Architecture
```
MainActivity
├── ViewPager2 (4 fragments, offscreenPageLimit=4, all kept alive)
│   ├── [0] HeadquartersFragment
│   ├── [1] AdventurersFragment
│   ├── [2] DungeonsFragment
│   └── [3] RaidsFragment
├── BottomNavigationView (synced with ViewPager2, color: bottom_nav_color_selector)
├── DrawerLayout + NavigationView (left drawer: settings, FAQ, bestiary, etc.)
└── Top Bar (fragment name + gems + currency + tooltip icons)
```

---

## 🔧 ADAPTER / DYNAMIC RENDERING REFERENCE

### 9 Adapters in UIUtils.java

| Adapter | Layout | Used By | Item Rendering | Click Action |
|---------|--------|---------|----------------|--------------|
| **GridAdapter** | `layout_item` or `layout_item_big` | Storage, CollectDrops, Merchant grids | `image` → item sprite via `getIdImage()`, `stack` → quantity. Border: always `object_border_dim_white` | `onItemClick` → `openItemDetail()` |
| **GridAdapterEnemies** | `layout_item_big_grid` | Report dialog enemy grid | `image` → enemy sprite via `Enemy.getInstance(id).getImageId()`, `stack` → times slain. Border: `object_border_dim_white` | No click action |
| **RecipesAdapter** | `layout_craft_named` | Recipes dialog | `name` → recipe result name. `result.image` → result item (rarity-bordered). Up to 3 ingredients, each with image + stack count. Stack color: `dim_white` if enough, `failure` color if not. Plus signs between ingredients, hidden if < 3 ingredients. | Click result/ingredient → `openItemDetail()` |
| **BestiaryAdapter** | `layout_bestiary_element` | Bestiary dialog (area list) | `areaName` → area name text. Nested `enemiesGrid` → `BestiaryGridAdapter` for enemies in that area. | Click enemy → `getEnemyDetailDialog()` (only if enemy seen) |
| **BestiaryGridAdapter** | `layout_bestiary_enemy` | Nested inside BestiaryAdapter | `image` → enemy sprite if seen, `unknown` drawable if not seen | Click handled by parent BestiaryAdapter |
| **KingMessagesAdapter** | `layout_king_message` | Messages Received dialog | `message_text` → message title | Click → AlertDialog with title + body (singleton guarded) |
| **DoctrinesAdapter** | `layout_doctrine` | Choose Doctrine dialog | `doctrine_name` → name text. `doctrine_description` → short description. `doctrine_image` → doctrine icon. | Click → `openDoctrineDialog()` |
| **FaqAdapter** | `layout_king_message` (reused!) | FAQ dialog | `message_text` → FAQ title | Click → AlertDialog with question + answer |
| **PetsGridAdapter** | `layout_pet_grid` | Shelter dialog, Choose Pet dialog | `image` → pet sprite via `getIdImage()`. `level` → pet level text. `autofeed` → heart icon, visible only if `isFavourite()` | `onItemClick` → `openPetDetail()` or select pet |

### Dynamic View Creation (addView/removeAllViews patterns)

| Dialog | Pattern | What's Added | Layout |
|--------|---------|-------------|--------|
| **DialogTavern** | `removeAllViews()` → loop `addView(inflate(layout_tavern_adventurer))` | Tavern adventurer cards (portrait, name, class, level, hire button) | `layout_tavern_adventurer` |
| **DialogWorkshop** | `removeAllViews()` → loop `addView(inflate(layout_workshop_item))` | Crafting queue items (image, name, progress, time) | `layout_workshop_item` |
| **DialogMarket** | `removeAllViews()` → loop `addView(inflate(layout_market_item))` | Market sell items (image, name, price) | `layout_market_item` |
| **DialogQuests** | `removeAllViews()` → loop `addView(inflate(layout_quest))` per quest group | Quest entries (name, description, reward, progress) | `layout_quest` |
| **DialogEntityDetail** | `addView` to `loot_content` | Loot drop icons horizontally | `layout_item` |
| **DialogItemDetail** | `removeAllViews()` → `addView(inflate(layout_craft))` | Craft ingredients + result | `layout_craft` |
| **DialogSelectEquipment** | `removeAllViews()` → loop `addView(inflate(layout_select_equipment))` | Equipment options (portrait, stats, equipped state) | `layout_select_equipment` |
| **DialogChooseAdventurer** | `addView(inflate(layout_adventurer_summary))` per adventurer | Compact adventurer cards for team selection | `layout_adventurer_summary` |
| **DialogConsumeFood** | `removeAllViews()` → loop `addView(inflate(layout_item))` | Food items to pick from | `layout_item` |
| **DialogConsumePotion** | `removeAllViews()` → loop `addView(inflate(layout_item))` | Potion items to pick from | `layout_item` |
| **DialogMergePet** | `removeAllViews()` → loop `addView(inflate(layout_pet_feeding))` | Pet feeding cards | `layout_pet_feeding` |
| **DialogDoctrine** | `removeAllViews()` on 3 containers → loop `addView(inflate(layout_doctrine_ability))` | Doctrine ability tree rows | `layout_doctrine_ability` |
| **DialogRecallAdventurers** | `removeAllViews()` → loop `addView(inflate(layout_adventurer_summary_no_margin))` | Adventurer cards for recall | `layout_adventurer_summary_no_margin` |
| **DialogChangeTraitRare** | `removeAllViews()` → loop `addView(inflate(layout_adventurer_change_trait))` | Trait selection cards | `layout_adventurer_change_trait` |
| **DialogPromotionChoices** | loop `addView(inflate(layout_promote_adventurer))` | Promotion class option cards | `layout_promote_adventurer` |
| **AdventurersFragment** | `removeAllViews()` → loop `addView(inflate(layout_adventurer))` per adventurer | Full adventurer cards (portrait, name, class, level, stats, equipment) | `layout_adventurer` |

### Storage Dialog — Filter + Sort System

```
Filter RadioGroup (category):
├── All → show all items
├── Materials → item.isCraftingMaterial()
├── Weapons → item.isWeapon()
├── Armors → item.isArmor()
├── Accessories → item.isAccessory()
└── Consumables → item.isConsumable()

Sort RadioGroup (order):
├── Type → Utils.itemsByTypeComparator
├── Quantity → comparingInt(stack)
├── Alphabetical → comparing(name)
├── Price/Unit → comparingLong(price per unit)
└── Price/Total → comparingLong(price * stack)

Collapsible: filtersArrow toggles visibility (menu_lift ↔ menu_drop icons)
Empty state: noItemsTooltip visible, itemGrid invisible
```

### Ascended Unit Visual Override
```java
applyAscendedPalette(binding):
  containerAdventurer.bg → object_border_ascended (fill: ascended_background, stroke: ascended_unit)
  weapon/armor/accessory.bg → object_border_ascended
  image.bg → object_border_rounded_left_ascended (left corners only)
  name.textColor → ascended_unit (#ffdb7f)
```

---

## 🏗️ ALL SCREEN HIERARCHIES

### P1-04: Currency Bar
```
ConstraintLayout (padding 8dp)
  [platinum_icon] → [platinum_text] → 8dp → [gold_icon] → [gold_text] → 8dp → [silver_icon] → [silver_text] → 8dp → [copper_icon] → [copper_text]
  Right-aligned chain. Icon 2dp before text, text 8dp before next icon.
```
Assets: `coin_platinum`, `coin_gold`, `coin_silver`, `coin_copper`

### P2-01: Headquarters Tab
```
ScrollView → ConstraintLayout
  6 building cards (quarters, tavern, storage, market, workshop, shelter)
  Each: ConstraintLayout bg:object_border_dim_white, pad 12dp
    Title (20sp bold centered)
    Description ("{count}/{capacity}" centered)
    Optional "NEW" dot + label
  6 sign icons overlaid from left: sign_quarters..sign_shelter (42×54dp, 24dp marginStart)
```
Click: each container → open dialog (singleton guarded)

### P2-02: Quarters Dialog
```
ConstraintLayout
  description → description_help text
  upgrade_description → upgrade cost details
  layout_money → current balance
  button_upgrade → upgrade action
  exit3 → "Close"
```
Backend: `CharacterService` (capacity upgrade)

### P2-03: Tavern Dialog
```
ConstraintLayout
  ScrollView → LinearLayout (vertical, dynamic addView)
    Per adventurer: layout_tavern_adventurer (63 lines)
      portrait (90×90, border), name (bold), class/level, traits
      hire_button (if affordable → dim_white, else → failure color)
  refresh_countdown → timer
  close → "Close"
```
Backend: `TavernService`

### P2-04: Storage Dialog
```
ConstraintLayout
  Filter controls (collapsible RadioGroup):
    Category: All|Materials|Weapons|Armors|Accessories|Consumables
    Sort: Type|Quantity|Alphabetical|PriceUnit|PriceTotal
    Arrow toggle: menu_lift ↔ menu_drop
  NonScrollableGridView #itemGrid → GridAdapter (numColumns=5)
    Per cell: layout_item (ImageView#image + TextView#stack)
  noItemsTooltip → "No items" (visible when empty)
  buttonUpgradeSpaces → upgrade capacity (hidden at level ≥ 80)
  close → "Close"
```
Backend: `InventoryService`

### P2-05: Market Dialog
```
ConstraintLayout
  ScrollView → LinearLayout (dynamic addView)
    Per item: layout_market_item (87 lines)
      image, name, price, sell/buy button
  close → "Close"
```
Backend: needs `MarketService`

### P2-06: Workshop Dialog
```
ConstraintLayout
  ScrollView → ConstraintLayout
    craft_queue → dynamic addView(layout_workshop_item) per queue slot
    recipes_button → opens DialogRecipes
  close → "Close"
```
Backend: `CraftService`

### P2-07: Shelter Dialog
```
ConstraintLayout
  NonScrollableGridView #petsGrid → PetsGridAdapter (per pet: image, level, autofeed heart)
  description → capacity info
  empty_pets → shown when no pets
  upgrade_capacity → capacity upgrade
  upgrade_autofeed → autofeed upgrade
  layout_money × 2 (for each upgrade cost)
  close → "Close"
```
Backend: `PetService`

### P3-01: Adventurers Tab
```
ConstraintLayout (padding 16dp)
  ScrollView → LinearLayout (vertical)
    dynamic addView(layout_adventurer) per adventurer (163 lines each)
    layout_adventurer:
      ImageView#image (90×90, left-rounded border)
      TextView#name (bold 16sp)
      TextView#class_level
      TextView#traits (brass color)
      ImageView#weapon, #armor, #accessory (32×32 each)
      Notification dot #dot (12sp, brass)
    Ascended units: applyAscendedPalette() overrides borders + name color
```
Click: adventurer card → opens DialogEntityDetail

### P3-02: Entity Detail (713 lines, 4 pages)
See detailed hierarchy in previous handoff section.

### P3-03: Select Equipment
```
ConstraintLayout
  ScrollView → LinearLayout (vertical)
    dynamic addView(layout_select_equipment) per equipment option (63 lines)
      image (44×44, dim_white border)
      name, stats text
      equipped_label (if currently equipped)
  unequip → unequip current
  close → "Close"
```

### P3-04: Item Detail
```
ConstraintLayout
  item_image (64×64, dim_white border)
  item_name (bold 18sp)
  item_description
  item_stats → dynamic text
  ScrollView → ingredients (if craftable): dynamic addView(layout_craft)
  sell_button + sell_price
  close → "Close"
```

### P4-01: Dungeons Tab
```
ScrollView → ConstraintLayout (padding 16dp)
  N × include @layout/layout_dungeon (each 206 lines)
  Dungeons: enchanted_forest, the_desert, eternal_battlefield, the_golden_city,
            blackwater_port, frostbite_peaks, obsidian_mines, + more
  Each layout_dungeon card:
    name, level_range, team_status
    progress_bar, reward_indicator
    Click → DialogDungeonDetail
```

### P4-02: Dungeon Detail (313 lines)
```
ConstraintLayout (fullscreen: MATCH_PARENT × 90% height)
  ScrollView
    20 × include layout_entity_fighting (for each enemy in dungeon)
      entity image, name, HP bar, level
    dungeon_name, dungeon_description
    progress bars × 2
    send_team_button → opens DialogSendTeam
    recall_button → opens DialogRecallAdventurers
    report_button → opens DialogReport
  ImageView animated_icon_damaged → danger indicator
  close → "Close"
```

### P4-03: Send Team
```
ScrollView → ConstraintLayout
  14 × include layout_adventurer (adventurer slots)
    Empty slot: "+" icon, dim border
    Filled: adventurer portrait, name, class/level
  pet_container:
    image_pet_plus (empty) / image_pet + level_pet (filled)
  Action buttons: send, load, save, clear, close
```

### P4-04: Report
```
ConstraintLayout
  duration_label/value
  areas_cleared_label/value
  team_wiped_label/value
  exp_earned/lost/per_hour labels/values
  enemies_slain_label
  NonScrollableGridView #item_grid → GridAdapterEnemies
  close
```

### P5-01: Raids Tab
Same as Dungeons Tab + bottom bar with raid refresh timer.

### P6-01: Merchant
```
ConstraintLayout
  container_regular → regular items header + countdown + NonScrollableGridView#regular_item_grid
  container_special → special items header + countdown + NonScrollableGridView#special_item_grid
  new_regular/new_special indicators
  no_regular_items/no_special_items empty states
  help → merchant help text
  close
```

### P6-04: Craft
```
ConstraintLayout
  include layout_craft_big → result item preview
  builds_from → ingredient list
  number (quantity) with button_minus/button_plus and seekBar
  warning_no_ingredients / warning_full_queue
  time → crafting time text
  craft → craft button
  close
```

### P6-05: Recipes
```
ConstraintLayout
  8 filter buttons (tab-like): All, Weapons, Armors, etc.
  CheckBox → show only craftable
  ListView → RecipesAdapter
    Per recipe: layout_craft_named (result + up to 3 ingredients with plus signs)
  close
```

### P7-01: Pet Detail (229 lines)
```
ConstraintLayout
  detail_image (90×90, dim_white border)
  detail_level, detail_experience, detail_experience_bar
  detail_traits
  description → pet description
  container_ability1..4:
    ability_name, ability_description
    lock icon (abilities 2-4 locked until level)
  pet_type → type label
  dismiss → dismiss pet button
  exit2 → "Close"
```

### P8-01: Quests (477 lines)
```
ConstraintLayout
  ScrollView
    8 quest group containers (kings, affliction, control, fortitude, grace, illusion, knowledge, ruin, war):
      title_*_quests → group header
      *_lp_bonus → LP bonus text
      *_progress → progress text
      *_quests_list → dynamic addView(layout_quest)
    no_quests_message → "All completed"
    new_quests_description + time → next refresh
  refresh → refresh quests button
  close → "Close"
```

### P9-01: Settings (283 lines)
```
ScrollView (width=90% screen)
  21 TextViews as setting labels/controls:
    Sound toggle, Music toggle
    Colorblind mode toggle
    Notifications toggle
    Speed settings
    Cloud save, Export, Import
    Credits, Version
  NonScrollableGridView (for any grid content)
  close → "Close"
```

### P9-03: Bestiary
```
ConstraintLayout
  ListView → BestiaryAdapter
    Per area: layout_bestiary_element (area name + enemy grid)
      NonScrollableGridView → BestiaryGridAdapter
        Per enemy: layout_bestiary_enemy (image only — unknown if not seen)
  button_sort → toggle sort order
  close → "Close"
```

---

## 🎨 COMPLETE VISUAL SYSTEM

### Game Theme
| Property | Value |
|----------|-------|
| Parent | `Theme.MaterialComponents.DayNight.DarkActionBar` |
| textColor | `#c8c8c8` (dim_white) |
| colorPrimary | `#befaa03e` (brass_border) |
| colorSecondary | `#befaa03e` (brass_border) |
| colorOnPrimary | `#000000` (black) |
| statusBarColor | matches colorPrimaryVariant |

### Core XML Drawables (Unity shapes needed)

| Drawable | Fill | Stroke | Corners | Usage |
|----------|------|--------|---------|-------|
| `dialog_border` | `cardview_dark_background` (#1e1e1e) | 3dp black | 10dp all | Dialog window bg |
| `object_border_dim_white` | `standard_background` (#1fffffff) | 1dp dim_white (#c8c8c8) | 10dp all | Cards, buttons, item borders |
| `object_border_brass` | `standard_background` | 1dp brass_border (#befaa03e) | 10dp all | Highlighted items, level-up |
| `object_border_no_background` | transparent | 1dp dim_white | 10dp all | Help text boxes |
| `object_border_ascended` | `ascended_background` | 1dp ascended_unit (#ffdb7f) | 10dp all | Ascended unit cards |
| `object_border_rounded_left_ascended` | `ascended_background` | 1dp ascended_unit | 10dp LEFT only | Ascended portrait border |

### Core Colors (game-specific, ordered by importance)

| Name | Hex | Alpha | Usage |
|------|-----|-------|-------|
| `dim_white` | `#c8c8c8` | 100% | Primary text, borders |
| `brass_border` | `#faa03e` | 75% (`#be`) | Highlights, selected tabs, primary |
| `brass_filler` | `#faa03e` | 100% | "NEW" labels, solid accents |
| `standard_background` | `#ffffff` | 12% (`#1f`) | Card/section fill |
| `standard_background_unavailable` | `#ff0000` | 12% (`#1f`) | Unavailable cards |
| `lower_drawer_body` | `#282828` | 100% | Bottom bar bg |
| `lower_drawer_header` | `#505050` | 100% | Drawer header bg |
| `ascended_unit` | `#ffdb7f` | 100% | Ascended character highlight |
| `ascended_background` | (see colors.json) | | Ascended card fill |
| `hp_bar` | `#ff5a5a` | 100% | HP bar fill |
| `failure` | (see colors.json) | | Error/not-enough text |
| `failure_colorblind` | (see colors.json) | | Colorblind mode error text |
| `extra_opaque_background` | (see colors.json) | | Dialog section bg |
| `cardview_dark_background` | `#1e1e1e` | 100% | Dialog window fill |
| `unselected_nav_view_element` | (see colors.json) | | Inactive tab icon |
| `merchant_special_header` | (see colors.json) | | Merchant special section |

### Typography (system font only)

| Size | Style | Usage |
|------|-------|-------|
| 20sp | Bold | Building names, section headers |
| 18sp | Bold | Dialog titles, item names |
| 16sp | Bold | Currency amounts, entity names, sub-headers |
| 14sp | Regular | Default body text |
| 12sp | Bold | Notification dots, small badges |
| — | Bold + Italic | "NEW" labels |

### Vector Icons (XML → Unity sprite equivalents)

| Icon | Size | Tint | Alpha | Path Description |
|------|------|------|-------|------------------|
| `swap_equipment` | 24dp | brass_border | 0.75 | Circular arrows (refresh) |
| `plus_white_half_alpha` | 48dp | white | 0.50 | Plus sign |
| `lock_close_transparent` | 40dp | white | 0.65 | Padlock |
| `menu_lift` | 36w×24h | white | 0.75 | Double chevron up |
| `menu_drop` | 36w×24h | white | 0.75 | Double chevron down |
| `vector_menu` | 24dp | white | — | Hamburger menu |

---

## 🔄 NAVIGATION + STATE RULES

### Dialog Singleton Pattern
Every dialog in the game uses this pattern:
```java
if (shownDialogX == null) {
    shownDialogX = new DialogX();
    shownDialogX.show(fragmentManager, "tag");
}
// On dismiss → shownDialogX = null
```
**Unity:** Check if popup panel already active before opening.

### Back/Close Behavior
- All dialogs: "Close" text button (dim_white, bold) → dismiss
- No nested back stack (dialogs 1 level deep from tabs)
- Drawer: close on item select or outside tap
- Tabs: BottomNav tap or ViewPager2 swipe

### Tab State Preservation
- `offscreenPageLimit = 4` → all 4 fragments kept alive
- Tab switch does NOT recreate fragment content
- **Unity:** Keep all tab panels instantiated, toggle visibility

### NonScrollableGridView
Custom `GridView` that expands height to fit all items (no internal scrolling). Used inside ScrollViews.
**Unity:** Use `GridLayoutGroup` with content size fitter.

---

## ⚡ DYNAMIC RESOURCE LOADING PATTERNS

| Pattern | Count | Key Usage |
|---------|-------|-----------|
| `R.drawable.*` static | ~900 | Known asset at compile time → use `Resources.Load<Sprite>(path)` |
| `getIdentifier("unit_" + id)` | ~30 | Character/enemy portraits → `SpriteAtlas.GetSprite(id)` |
| `getIdentifier("doctrine_" + id)` | ~5 | Doctrine icons → same sprite atlas approach |
| `item.getIdImage()` | ~80 | Item sprites → sprite name mapping from data |
| String concat `"unit_" +` | ~50 | Same as getIdentifier pattern |

---

## 🔗 BACKEND MAPPING

| Legacy Feature | Unity Service | Status |
|---------------|--------------|--------|
| Adventurer CRUD | `CharacterService` | ✅ |
| Equipment management | `EquipmentService` | ✅ |
| Inventory | `InventoryService` | ✅ |
| Tavern recruitment | `TavernService` | ✅ |
| Crafting | `CraftService` | ✅ |
| Dungeons/Expeditions | `DungeonService` | ✅ |
| Combat | `CombatService` | ✅ |
| Party management | `PartyService` | ✅ |
| Quests | `QuestService` | ✅ |
| Pets | `PetService` | ✅ |
| Doctrines | `DoctrineService` | ✅ |
| Promotions | `PromotionService` | ✅ |
| Merchant | `MerchantService` | ✅ |
| Offline progress | `OfflineProgressService` | ✅ |
| Loot | `LootService` | ✅ |
| Settings | `SettingsService` | ✅ |
| Items | `ItemService` | ✅ |
| Enemies | `EnemyService` | ✅ |
| Skills | `SkillService` | ✅ |
| Status effects | `StatusEffectService` | ✅ |
| Save/Load | `SaveService` | ✅ |
| Target selection | `TargetSelectionService` | ✅ |
| Market listing/selling | — | 🔴 needs `MarketService` |
| IAP Shop | — | 🔴 needs IAP wrapper |
| Tutorial | — | 🔴 needs `TutorialService` |
| King Messages | — | 🟡 simple, can be added quickly |
| Redeem Code | — | 🟡 simple API call |

---

## 📁 SUPPLEMENTARY DATA FILES

| File | Content |
|------|---------|
| [legacy_asset_inventory_v2.csv](file:///D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/legacy_asset_inventory_v2.csv) | 1,036 assets reclassified into 19 categories |
| [deep_layout_hierarchy.csv](file:///D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/deep_layout_hierarchy.csv) | 1,043 UI nodes across 79 layouts |
| [deep_dynamic_loading.csv](file:///D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/deep_dynamic_loading.csv) | 1,193 dynamic resource loading patterns |
| [deep_xml_drawables.csv](file:///D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/deep_xml_drawables.csv) | 199 XML drawables (117 game-specific) |
| [deep_game_colors.json](file:///D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/deep_game_colors.json) | 189 game-specific colors |
| [deep_game_dimens.json](file:///D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/deep_game_dimens.json) | 197 game-specific dimensions |
| [Asset_Gallery/](file:///D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery) | 11 visual contact sheets |
| [unresolved_questions.md](file:///D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/unresolved_questions.md) | 0 blockers remaining |
