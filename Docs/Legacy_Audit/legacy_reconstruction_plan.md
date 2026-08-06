# Legacy-Faithful Reconstruction Plan

> Strategy: Replicate legacy game UI/UX first, then gradually replace assets.
> Based on audit findings from 2026-08-04.

---

## Strategy: 3 Stages

### Stage 1: Legacy-Faithful (Current Focus)
- Rebuild all 55 screens to match legacy layout, navigation, and behavior
- Use legacy PNG assets directly (copy from decode folder)
- Match colors, spacing, and component patterns exactly
- Target: "screenshot comparison should be nearly identical"

### Stage 2: Asset Replacement (Later)
- Replace legacy PNGs with new artwork (user-created)
- Keep layout structure identical
- One-for-one swap per asset

### Stage 3: UX Modernization (Later)
- Polish animations, transitions
- Improve touch feedback
- Potential layout refinements (but keep feature parity)

---

## Reconstruction Phases (Stage 1)

### Phase 1: Core Shell & Navigation (Priority: CRITICAL)

**Goal:** Match legacy's single-activity, tab+drawer+dialog architecture.

| Task | Description | Legacy Reference |
|------|-------------|-----------------|
| 1.1 | Implement 4-tab bottom navigation (HQ, Adventurers, Dungeons, Raids) | `mobile_navigation.xml`, `bottom_nav_menu.xml` |
| 1.2 | Implement drawer menu (10 items) | `drawer_nav_menu.xml` |
| 1.3 | Implement HUD: top bar (menu + title + gems) | `activity_main.xml` lines 176-233 |
| 1.4 | Implement HUD: currency bar | `layout_money.xml` |
| 1.5 | Implement HUD: tooltip icons row | `activity_main.xml` lines 29-127 |
| 1.6 | Implement dialog show/dismiss singleton pattern | `MainActivity.java` guard pattern |
| 1.7 | Conditional Raids tab visibility | `refreshRaidsFragmentVisibility()` |

**Dependencies:** None (foundational)
**Estimated Effort:** High

---

### Phase 2: Headquarters Screen (Priority: HIGH)

**Goal:** Match the 6-building-card layout.

| Task | Description | Legacy Reference |
|------|-------------|-----------------|
| 2.1 | HQ fragment: 6 scrollable building cards | `fragment_headquarters.xml` |
| 2.2 | Card pattern: sign icon (42×54dp) + name (bold 20sp) + description | Same XML |
| 2.3 | DialogQuarters: adventurer list with capacity | `dialog_quarters.xml` |
| 2.4 | DialogTavern: guest list, timer, recruit | `dialog_tavern.xml` |
| 2.5 | DialogStorage: item grid | `dialog_storage.xml` |
| 2.6 | DialogMarket: sell/listing UI | `dialog_market.xml` |
| 2.7 | DialogWorkshop: craft queue + recipes | `dialog_workshop.xml` |
| 2.8 | DialogShelter: pet list | `dialog_shelter.xml` |

**Dependencies:** Phase 1 (navigation shell)

---

### Phase 3: Adventurer/Character System (Priority: HIGH)

| Task | Description |
|------|-------------|
| 3.1 | AdventurersFragment: adventurer grid/list |
| 3.2 | DialogEntityDetail: stats, equipment, actions |
| 3.3 | DialogSelectEquipment: slot-based equip UI |
| 3.4 | DialogItemDetail: item stats, rarity border |
| 3.5 | DialogPromotionChoices: class selection |
| 3.6 | DialogChangeTraitRare: trait swap |
| 3.7 | Consumption dialogs (6): food/potion/intercession/evo |
| 3.8 | Doctrine system (3 dialogs) |
| 3.9 | DialogRecallAdventurers: dismissed list |

**Dependencies:** Phase 1

---

### Phase 4: Combat/Dungeon System (Priority: HIGH)

| Task | Description |
|------|-------------|
| 4.1 | DungeonsFragment: dungeon list |
| 4.2 | DialogDungeonDetail: dungeon info, team |
| 4.3 | DialogSendTeam: adventurer + pet selection |
| 4.4 | DialogChooseAdventurer: picker grid |
| 4.5 | DialogChoosePet: pet picker |
| 4.6 | DialogCollectDrops: loot screen |
| 4.7 | DialogReport: combat log |
| 4.8 | DialogIdleProgress: offline rewards |

**Dependencies:** Phase 1, Phase 3 (adventurer data)

---

### Phase 5: Raids System (Priority: MEDIUM)

| Task | Description |
|------|-------------|
| 5.1 | RaidsFragment: raid list (conditional visibility) |
| 5.2 | Reuse dungeon detail/send team |
| 5.3 | DialogRefillRaidTry: gem spend |

**Dependencies:** Phase 4

---

### Phase 6: Commerce System (Priority: MEDIUM)

| Task | Description |
|------|-------------|
| 6.1 | DialogMerchant: regular + premium items |
| 6.2 | DialogBuyFromMerchant: purchase confirm |
| 6.3 | DialogSell: sell UI with price |
| 6.4 | DialogShop: IAP store |
| 6.5 | Craft system (reuse from Phase 2.7) |
| 6.6 | DialogRecipes: recipe list |

**Dependencies:** Phase 2 (market/workshop), Phase 3 (items)

---

### Phase 7: Pets System (Priority: MEDIUM)

| Task | Description |
|------|-------------|
| 7.1 | DialogPetDetail: stats, feeding |
| 7.2 | DialogMergePet: merge UI |

**Dependencies:** Phase 2 (shelter)

---

### Phase 8: Quest System (Priority: MEDIUM)

| Task | Description |
|------|-------------|
| 8.1 | DialogQuests: quest list (9 quest types) |
| 8.2 | DialogRefreshQuests: refresh timer |

**Dependencies:** Phase 1

---

### Phase 9: System/Meta Screens (Priority: LOW)

| Task | Description |
|------|-------------|
| 9.1 | DialogSettings: language, sound, credits |
| 9.2 | DialogFaq: FAQ list |
| 9.3 | DialogBestiary: enemy catalog |
| 9.4 | DialogMessagesReceived: king message history |
| 9.5 | DialogRedeemCode: input field |
| 9.6 | Tutorial system (7 steps) |
| 9.7 | Cloud save (Google Play Games integration) |

**Dependencies:** Phase 1

---

## Asset Import Strategy

### Step 1: Copy Referenced Assets
- Import all 1,023 referenced PNGs from `res/drawable/` into Unity `Assets/_Game/Sprites/Legacy/`
- Maintain exact filenames
- Set import settings: Sprite (2D), no mipmaps, Point filtering for pixel art

### Step 2: Organize by Function
```
Assets/_Game/Sprites/Legacy/
├── Units/          (685 unit_*.png)
├── Items/          (125 item/equipment PNGs)
├── Icons/          (65 icon PNGs)
├── Pets/           (21 pet_*.png)
├── Currency/       (21 currency PNGs)
├── UI/             (90 misc system PNGs)
└── Signs/          (6 sign_*.png)
```

### Step 3: Sprite Atlas
- Create sprite atlases per category for batching
- Each atlas ≤ 2048×2048

---

## Visual Standards (from Legacy)

| Property | Value | Apply To |
|----------|-------|----------|
| Background | Dark theme, `#282828` base | All screens |
| Card bg | Semi-transparent white (`#1fffffff`) | Cards, containers |
| Card border | `object_border_dim_white` (XML drawable) | All interactive cards |
| Title font | System default, bold 20sp | Section headers |
| Body font | System default, regular 14sp | Descriptions |
| Currency font | Bold 16sp | Gold, gems, amounts |
| Icon padding | 8dp (tooltip icons) | HUD icons |
| Card padding | 12dp | Building cards |
| Card gap | 16dp vertical | Between cards |
| Notification dot | 12×12dp brass circle, -3dp offset | Merchant, quests |
| Sign icon | 42×54dp, 24dp from left | HQ building signs |
| Rarity | 6-tier color borders (gray→green→blue→orange→red→gold) | Items, adventurers |

---

## Unresolved Questions

1. **layout_money.xml** — Need to audit this include for exact currency breakdown layout
2. **Dynamic resource lookup** — Some assets may be loaded via `getIdentifier()` (not caught by static reference scan)
3. **Animation details** — 36 anim + 36 animator XMLs need individual audit for transition timing
4. **Strings** — 486KB strings.xml contains all game text; needs selective extraction per screen
5. **Dimens** — 39KB dimens.xml may contain screen-specific dimensions not caught by layout audit
6. **Styles** — 445KB styles.xml needs audit for custom themes applied to dialogs
7. **Runtime UI** — Without emulator, can't verify exact rendered appearance; relying on XML/code analysis only
