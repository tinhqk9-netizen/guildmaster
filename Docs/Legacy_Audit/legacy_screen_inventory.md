# Legacy Screen Inventory

> Complete inventory of all screens in the legacy game.
> Evidence: Java source + XML layout analysis.

---

## Screen Types

| Type | Count | Description |
|------|-------|-------------|
| Activity | 1 | `MainActivity` — single activity, hosts everything |
| Fragment (Tab) | 4 | Bottom navigation tabs |
| Dialog (Feature) | 46 | Full-screen or modal dialogs for every feature |
| AlertDialog (System) | ~6 | Cloud, Ad, AdFree, FullStorage, KingMessage, IndividualFaq |
| **Total Unique Screens** | **~57** | |

---

## Tab Fragments (Bottom Navigation)

### Tab 0: Headquarters
| Field | Value |
|-------|-------|
| **Class** | `HeadquartersFragment` |
| **Layout** | `fragment_headquarters.xml` (273 lines) |
| **Source** | `ui/headquarters/HeadquartersFragment.java` |
| **Title** | `@string/fragment_name_headquarters` |
| **Icon** | `@drawable/bottom_nav_castle` |
| **Start Dest** | YES (default tab) |
| **Scrollable** | YES (ScrollView wrapping ConstraintLayout) |
| **Children** | 6 building cards (Quarters, Tavern, Storage, Market, Workshop, Shelter) |
| **Each Card** | Sign icon (42×54dp) + Name (bold 20sp) + Description (count/capacity) |
| **Background** | `@drawable/object_border_dim_white` on each card |
| **Padding** | 16dp container, 12dp cards, 16dp gap between cards |
| **Opens** | Click each card → respective Dialog |
| **Evidence** | `HeadquartersFragment.java` lines 62-99, `fragment_headquarters.xml` |

**Card → Dialog Mapping:**

| Card | Dialog Opened | Guard |
|------|--------------|-------|
| Quarters | `DialogQuarters` | `shownDialogQuarters == null` |
| Tavern | `DialogTavern` | `shownDialogTavern == null` |
| Storage | `DialogStorage` | `shownDialogStorage == null` |
| Market | `DialogMarket` | `shownDialogMarket == null` |
| Workshop | `DialogWorkshop` | `shownDialogWorkshop == null` |
| Shelter | `DialogShelter` | `shownDialogShelter == null` |

### Tab 1: Adventurers
| Field | Value |
|-------|-------|
| **Class** | `AdventurersFragment` |
| **Layout** | `fragment_adventurers.xml` |
| **Source** | `ui/adventurers/AdventurersFragment.java` |
| **Title** | `@string/fragment_name_adventurers` |
| **Icon** | `@drawable/bottom_nav_adventurers` |
| **Evidence** | `mobile_navigation.xml` line 8-9 |

### Tab 2: Dungeons
| Field | Value |
|-------|-------|
| **Class** | `DungeonsFragment` |
| **Layout** | `fragment_dungeons.xml` |
| **Source** | `ui/dungeons/DungeonsFragment.java` |
| **Title** | `@string/fragment_name_dungeons` |
| **Icon** | `@drawable/bottom_nav_dungeons` |
| **Evidence** | `mobile_navigation.xml` line 11-12 |

### Tab 3: Raids
| Field | Value |
|-------|-------|
| **Class** | `RaidsFragment` |
| **Layout** | `fragment_raids.xml` |
| **Source** | `ui/raids/RaidsFragment.java` |
| **Title** | `@string/fragment_name_raids` |
| **Icon** | `@drawable/bottom_nav_raids` |
| **Visibility** | Hidden until at least 1 raid is unlocked |
| **Evidence** | `MainActivity.java` `refreshRaidsFragmentVisibility()` lines 401-421 |

---

## HUD Bar (Always Visible — in activity_main.xml)

### Top Bar (constraintLayout)
| Element | ID | Type | Details |
|---------|-----|------|---------|
| Menu (hamburger) | `menu_button` | ImageView | `@drawable/vector_menu` |
| Screen title | `fragment_name` | TextView | Bold 20sp, set per fragment |
| Gems display | `container_gems` | ConstraintLayout | `@drawable/object_border_dim_white` bg |
| Gem icon | `image_gems` | ImageView | `@drawable/gem` |
| Gem count | `amount_gems` | TextView | Bold 16sp |

### Currency Bar (constraintLayout2)
| Element | Details |
|---------|---------|
| Include | `@layout/layout_money` |
| Background | `@color/standard_background` |
| Padding | 16dp L/R, 2dp top, 4dp bottom |

### Tooltip Icons Row (containerTooltips)
| ID | Icon | Click Action | Visibility |
|----|------|-------------|------------|
| `shop` | `@drawable/shop` | → `DialogShop` | If IAP initialized |
| `king_message` | `@drawable/king_message` | → KingMessage AlertDialog | If messages > 0 |
| `merchant` | `@drawable/merchant` | → `DialogMerchant` | Always |
| `new_items` | `@drawable/brass_circle` | Notification dot on merchant | If new merchant items |
| `quests` | `@drawable/quest_marker` | → `DialogQuests` | If quests seen |
| `quests_notification` | `@drawable/brass_circle` | Notification dot on quests | If active quests |
| `ad` | `@drawable/advertisement` | → Ad dialog | If ads available |
| `adfree` | `@drawable/advertisement` | → AdFree dialog | If starter pack purchased |

All tooltip icons have: `@drawable/object_border_dim_white` background, 8dp padding, 16dp top margin.

### Tutorial Section (containerTutorial)
| Element | Details |
|---------|---------|
| Icon | `@drawable/tutorial_icon` with `@drawable/object_border_brass` bg |
| Title | Bold, `@string/tutorial_title` |
| Step | Bold, `@color/brass_border` color |
| Body | 16sp, HTML content |
| Visibility | Hidden when `tutorialStep >= 7` |

---

## Feature Dialogs (46 total)

### Headquarters Sub-Dialogs

| Dialog | Layout XML | Source Class | Opens From |
|--------|-----------|-------------|------------|
| DialogQuarters | `dialog_quarters.xml` | `DialogQuarters.java` | HQ → Quarters card |
| DialogTavern | `dialog_tavern.xml` | `DialogTavern.java` | HQ → Tavern card |
| DialogStorage | `dialog_storage.xml` | `DialogStorage.java` | HQ → Storage card |
| DialogMarket | `dialog_market.xml` | `DialogMarket.java` | HQ → Market card |
| DialogWorkshop | `dialog_workshop.xml` | `DialogWorkshop.java` | HQ → Workshop card |
| DialogShelter | `dialog_shelter.xml` | `DialogShelter.java` | HQ → Shelter card |

### Character/Adventurer Dialogs

| Dialog | Layout XML | Opens From |
|--------|-----------|------------|
| DialogEntityDetail | `dialog_entity_detail.xml` | Adventurer list item click |
| DialogSelectEquipment | `dialog_select_equipment.xml` | Entity detail → equip slot |
| DialogItemDetail | `dialog_item_detail.xml` | Any item click |
| DialogPromotionChoices | `dialog_promotion_choices.xml` | Entity detail → promote |
| DialogChooseAdventurer | `dialog_choose_adventurer.xml` | Various team selection |
| DialogChangeTraitRare | `dialog_change_trait_rare.xml` | Entity detail → change trait |
| DialogRecallAdventurers | `dialog_recall_adventurers.xml` | Drawer → Recall |

### Combat/Dungeon Dialogs

| Dialog | Layout XML | Opens From |
|--------|-----------|------------|
| DialogDungeonDetail | `dialog_dungeon_detail.xml` | Dungeon list item |
| DialogSendTeam | `dialog_send_team.xml` | Dungeon detail → send |
| DialogCollectDrops | `dialog_collect_drops.xml` | Post-combat loot |
| DialogReport | `dialog_report.xml` | Post-combat report |
| DialogIdleProgress | `dialog_idle_progress.xml` | App resume after idle |

### Pet Dialogs

| Dialog | Layout XML | Opens From |
|--------|-----------|------------|
| DialogPetDetail | `dialog_pet_detail.xml` | Pet list item |
| DialogChoosePet | `dialog_choose_pet.xml` | Team → choose pet |
| DialogMergePet | `dialog_merge_pet.xml` | Pet detail → merge |

### Consumption Dialogs

| Dialog | Layout XML | Opens From |
|--------|-----------|------------|
| DialogConsumeFood | `dialog_consume_food.xml` | Item → consume food |
| DialogConsumePotion | `dialog_consume_potion.xml` | Item → consume potion |
| DialogConsumePotionOfClumsiness | `dialog_consume_potion_of_clumsiness.xml` | Special potion |
| DialogConsumePotionOfRejuvenation | `dialog_consume_potion_of_rejuvenation.xml` | Special potion |
| DialogConsumeIntercession | `dialog_consume_intercession.xml` | Intercession item |
| DialogConsumeEvo23 | `dialog_consume_evo23.xml` | Evo item |

### Commerce Dialogs

| Dialog | Layout XML | Opens From |
|--------|-----------|------------|
| DialogMerchant | `dialog_merchant.xml` | HUD → Merchant icon |
| DialogBuyFromMerchant | `dialog_buy_from_merchant.xml` | Merchant → buy |
| DialogSell | `dialog_sell.xml` | Storage → sell item |
| DialogShop | `dialog_shop.xml` | HUD/Drawer → Shop |

### Crafting Dialogs

| Dialog | Layout XML | Opens From |
|--------|-----------|------------|
| DialogCraft | `dialog_craft.xml` | Workshop → craft item |
| DialogRecipes | `dialog_recipes.xml` | Workshop → recipes |

### Doctrine/Class System Dialogs

| Dialog | Layout XML | Opens From |
|--------|-----------|------------|
| DialogDoctrine | `dialog_doctrine.xml` | Entity detail → doctrine |
| DialogChooseDoctrine | `dialog_choose_doctrine.xml` | Doctrine → choose |
| DialogDoctrineReset | `dialog_doctrine_reset.xml` | Doctrine → reset |

### Quest/Progression Dialogs

| Dialog | Layout XML | Opens From |
|--------|-----------|------------|
| DialogQuests | `dialog_quests.xml` | HUD → Quests icon |
| DialogRefreshQuests | `dialog_refresh_quests.xml` | Quests → refresh |
| DialogRefillRaidTry | `dialog_refill_raid_try.xml` | Raids → refill |

### System/Meta Dialogs

| Dialog | Layout XML | Opens From |
|--------|-----------|------------|
| DialogSettings | `dialog_settings.xml` | Drawer → Settings |
| DialogFaq | `dialog_faq.xml` | Drawer → FAQ |
| DialogBestiary | `dialog_bestiary.xml` | Drawer → Bestiary |
| DialogMessagesReceived | `dialog_messages_received.xml` | Drawer → Messages |
| DialogRedeemCode | `dialog_redeem_code.xml` | Drawer → Redeem Code |

### Base Dialog

| Dialog | Layout XML | Purpose |
|--------|-----------|---------|
| CustomDialog | `custom_dialog.xml` | Base class for all game dialogs |

---

## Drawer Menu Items

| # | ID | Title String | Icon | Action |
|---|----|-------------|------|--------|
| 1 | `shop` | `drawer_shop_title` | `shop` | → `DialogShop` |
| 2 | `settings` | `drawer_settings_title` | `drawer_icon_settings` | → `DialogSettings` |
| 3 | `dismissed_adventurers` | `drawer_recall_adventurers_title` | `drawer_icon_recall_adventurers` | → `DialogRecallAdventurers` |
| 4 | `messages_received` | `drawer_messages_received_title` | `drawer_icon_king_message` | → `DialogMessagesReceived` |
| 5 | `faq` | `drawer_faq_title` | `drawer_icon_faq` | → `DialogFaq` |
| 6 | `bestiary` | `drawer_bestiary_title` | `drawer_icon_bestiary` | → `DialogBestiary` |
| 7 | `achievements` | `drawer_achievements_title` | `drawer_icon_achievements` | → Google Play Games |
| 8 | `cloud` | `drawer_cloud_title` | `drawer_icon_cloud` | → Cloud Save dialog |
| 9 | `redeem_code` | `drawer_redeem_code_title` | `drawer_icon_redeem_code` | → `DialogRedeemCode` |
| 10 | `reddit` | `drawer_reddit_title` | `drawer_icon_reddit` | → External browser (Reddit) |
| 11 | `cafe_naver` | `drawer_cafe_naver_title` | `drawer_icon_cafe_naver` | → External browser (Korean only) |

**Evidence:** `drawer_nav_menu.xml`, `MainActivity.java` `attachListeners()` lines 436-501
