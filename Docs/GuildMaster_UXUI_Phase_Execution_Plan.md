# Guild Master â€” UX/UI Redesign Phase Execution Plan

> **Project:** `D:\Tinh\Rebuild_GuildMaster`  
> **Backup root:** `D:\Tinh\Backups\`  
> **Target resolution:** **1080Ã—1920 portrait only**  
> **Build policy:** KhÃ´ng build APK trong Phases Bâ€“E8.  
> **Approval policy:** Anti triá»ƒn khai vÃ  ghi evidence. User + ChatGPT nghiá»‡m thu tá»«ng phase.  
> **Source of truth:** Project rebuild hiá»‡n táº¡i. Decode chá»‰ tham kháº£o tinh tháº§n tutorial.

---

## 1. Quy táº¯c chung

1. Chá»‰ lÃ m **má»™t phase táº¡i má»™t thá»i Ä‘iá»ƒm**.
2. TrÆ°á»›c khi sá»­a, táº¡o backup riÃªng cho phase trong `D:\Tinh\Backups\`.
3. KhÃ´ng dÃ¹ng:
   - `git reset --hard`
   - `git checkout -- .`
   - `git clean`
4. KhÃ´ng sá»­a gameplay formula, economy formula, save core, item definitions, quest reward logic, crafting recipes, guest generation hoáº·c combat formulas náº¿u chÆ°a cÃ³ scope riÃªng Ä‘Æ°á»£c duyá»‡t.
5. UI dÃ¹ng Inter Regular / Inter SemiBold TMP.
6. KhÃ´ng dÃ¹ng Unicode emoji trong production UI.
7. ChÆ°a cÃ³ art thÃ¬ dÃ¹ng `Image` placeholder sprite-ready.
8. Má»i screenshot runtime chá»‰ chá»¥p táº¡i **1080Ã—1920**.
9. Anti khÃ´ng Ä‘Æ°á»£c tá»± ghi visual approved.
10. Sau má»—i phase:
    - Recompile.
    - Cháº¡y test liÃªn quan.
    - VÃ o Play Mode.
    - Chá»¥p screenshot.
    - Ghi file Ä‘Ã£ sá»­a vÃ  evidence.
    - Dá»«ng láº¡i chá» User + ChatGPT nghiá»‡m thu.
11. Chá»‰ Ä‘Æ°á»£c sang phase sau khi status thÃ nh:
    - `USER + CHATGPT APPROVED`

---

## 2. Tráº¡ng thÃ¡i phase

| Status | Ã nghÄ©a |
|---|---|
| `NOT STARTED` | ChÆ°a báº¯t Ä‘áº§u. |
| `IN PROGRESS` | Anti Ä‘ang lÃ m phase. |
| `TECHNICAL PASS` | Compile/test/runtime Ä‘Ã£ qua, chá» duyá»‡t visual. |
| `CHANGES REQUESTED` | User hoáº·c ChatGPT yÃªu cáº§u sá»­a. |
| `USER + CHATGPT APPROVED` | Phase Ä‘Æ°á»£c cháº¥p thuáº­n, cÃ³ thá»ƒ sang phase káº¿ tiáº¿p. |
| `BLOCKED` | CÃ³ dependency ká»¹ thuáº­t Ä‘Ã£ Ä‘Æ°á»£c verify. |

---

## 3. Máº«u Completion Note báº¯t buá»™c

Anti pháº£i append block nÃ y vÃ o Ä‘Ãºng phase sau khi hoÃ n thÃ nh:

```md
### Completion Note

- **Status:** TECHNICAL PASS / CHANGES REQUESTED / BLOCKED
- **Date:**
- **Backup path:**
- **Files created:**
- **Files modified:**
- **Scene/prefab changes:**
- **Compile result:**
- **Test result:**
- **Play Mode result:**
- **Screenshot paths:**
- **Runtime warnings/errors:**
- **Known visual issues:**
- **Scope deviations:** None / explain
- **Rollback steps:**
- **Anti conclusion:** Technical only; visual approval pending
- **User review:**
- **ChatGPT review:**
- **Final approval:** PENDING / USER + CHATGPT APPROVED
```

Không xóa Completion Note cũ. Nếu sửa lại phase, append revision mới bên dưới.

---

# Phase B — Font Foundation

**Status:** `ROLLED BACK — REMOVED FROM CURRENT ROADMAP`

## Goal

Tạo nền typography TMP ổn định mà chưa thay layout screen.

## Allowed Scope

- `UITemporaryTheme.cs`
- `UICardFactory.cs`
- Inter Regular/SemiBold TTF
- Inter Regular/SemiBold TMP SDF assets
- TMP Essential Resources nếu đang thiếu
- Editor utility riêng cho phase nếu cần tạo asset idempotent

## Required Output

- Inter Regular cho body.
- Inter SemiBold cho title, tab, label, badge, button.
- Runtime-created card text chuyển từ Legacy `Text` sang TMP.
- Một cơ chế font loading duy nhất đã verify.
- Có null check và log lỗi exact path.
- Không thêm emoji glyph.
- Không sửa screen layout.

## Technical Validation

- 0 compile errors.
- Existing EditMode tests pass.
- Regular font load thành công.
- SemiBold font load thành công.
- Không có null font reference.
- Selected/disabled states còn hoạt động.
- TMP import diff và rollback paths được ghi lại.

## Visual Evidence

1080×1920:

- Tavern runtime cards.
- Character runtime cards.
- Inventory runtime cards.
- Normal / selected / disabled card states.

## Approval Gate

User + ChatGPT kiểm tra:

- Font sắc nét.
- Phân cấp Regular/SemiBold thấy rõ.
- Không phát sinh overlap/crop.
- UI cũ không bị khó đọc hơn.

### Rollback Note

- **Status:** ROLLED BACK — REMOVED FROM CURRENT ROADMAP
- **Date:** 2026-08-03
- **Backup source:** `D:\Tinh\Backups\Phase_B_Font_Foundation\`
- **Files restored:**
  - `Assets/_Game/Scripts/Runtime/UI/Core/UITemporaryTheme.cs`
  - `Assets/_Game/Scripts/Runtime/UI/Core/UICardFactory.cs`
  - `Assets/_Game/Scripts/GuildMaster.Runtime.asmdef`
- **Files deleted:**
  - `Assets/_Game/Resources/Fonts/Inter-Regular SDF.asset` & `.meta`
  - `Assets/_Game/Resources/Fonts/Inter-SemiBold SDF.asset` & `.meta`
  - `Assets/_Game/Resources/Fonts/Inter-Regular.ttf` & `.meta`
  - `Assets/_Game/Resources/Fonts/Inter-SemiBold.ttf` & `.meta`
  - `Assets/_Game/Resources/Fonts` & `.meta`
  - `Assets/_Game/Resources.meta`
  - `Assets/Editor/FontGenerator.cs` & `.meta`
  - Temporary test/refresh scripts (`CheckSavePath.cs`, `ForceRefresh.cs`, `ForceRefreshTool2.cs`, `TestPhaseB.cs`, `PhaseBFontSetup.cs`, `migrate_uicardfactory.py`, etc.)
- **Compile result:** 0 errors (Successfully recompiled all scripts)
- **Test result:** EditMode 171/171 Passed (0 Failed, 0 Ignored)
- **Play Mode result:** Baseline UI verified, 0 runtime errors
- **Screenshot paths:**
  - `rollback_phase_b_tavern.png`
  - `rollback_phase_b_inventory.png`
  - `rollback_phase_b_character.png`
- **User review:** `REJECTED`
- **ChatGPT review:** `REJECTED`
- **Reason:**
  - runtime UI rendering broken
  - oversized blank panels
  - click-triggered errors
  - user chose not to continue this phase
- **Final approval:** `NOT APPROVED`

---

# Phase C â€” HUD Visual Prototype

**Initial status:** `NOT STARTED`

## Goal

DÃ¹ng HUD lÃ m prototype cho hÆ°á»›ng Clean Modern Fantasy cá»§a toÃ n game.

## Allowed Scope

- `HUDController.cs`
- HUD nodes trong `Main.unity`
- Shared visual helper cáº§n cho HUD
- Placeholder image slots
- Currency presentation
- Popup blocker verification

## Required Layout

- Background fantasy Ä‘Æ°á»£c giá»¯ hoáº·c cáº£i thiá»‡n.
- Opaque/dark surfaces.
- Currency bar.
- Gold/Gem sprite-ready placeholders.
- Tutorial hint container, máº·c Ä‘á»‹nh áº©n.
- 8 destination:
  - Tavern
  - Character
  - Inventory
  - Craft
  - Merchant
  - Dungeon
  - Quest
  - Settings
- Má»—i nav button cÃ³:
  - Image placeholder
  - English label
  - Normal / pressed / disabled states

## Prohibited

- Persistent bottom navigation.
- More menu.
- Backend changes.
- Redesign screen ngoÃ i HUD.
- APK build.

## Technical Validation

- 8 button má»Ÿ Ä‘Ãºng destination.
- Currency update Ä‘Ãºng.
- HUD áº©n khi má»Ÿ screen.
- Popup block click xuá»‘ng HUD.
- 0 compile errors.
- Relevant tests pass.

## Visual Evidence

1080Ã—1920:

- Main HUD.
- Pressed/selected state.
- Disabled state náº¿u cÃ³.
- Popup over HUD chá»©ng minh raycast blocking.

## Approval Gate

User + ChatGPT quyáº¿t Ä‘á»‹nh visual direction cÃ³ Ä‘á»§ tá»‘t Ä‘á»ƒ Ã¡p dá»¥ng sang cÃ¡c screen khÃ¡c hay khÃ´ng.

---

# Phase D â€” Lightweight Tutorial

**Initial status:** `NOT STARTED`

## Goal

ThÃªm hÆ°á»›ng dáº«n first-session nháº¹, khÃ´ng thay gameplay hoáº·c existing save.

## Allowed Scope

- `HUDController.cs`
- `UIRuntimeBootstrap.cs`
- `DungeonScreen.cs`
- Tutorial-specific UI nodes/modal á»Ÿ popup layer Ä‘Ã£ duyá»‡t

## Flow

1. HUD hint: yÃªu cáº§u má»Ÿ Dungeon.
2. Dungeon hint: hÆ°á»›ng ngÆ°á»i chÆ¡i tá»›i Start Expedition.
3. Completion modal: giáº£i thÃ­ch party tá»± khÃ¡m phÃ¡ vÃ  nháº·t loot.

## Save Safety

- Chá»‰ trigger khi `LastLoadStatus == SaveLoadResult.FreshNewGame`.
- KhÃ´ng migrate existing saves.
- KhÃ´ng dÃ¹ng `TutorialStep 0 â†’ 99`.
- KhÃ´ng táº¡o gameplay service má»›i.
- KhÃ´ng sá»­a gameplay formula.

## Modal Rules

- CÃ³ Close.
- Tá»± áº©n sau 8 giÃ¢y.
- Tap outside khÃ´ng Ä‘Ã³ng.
- Overlay block raycast.
- Coroutine dá»«ng khi Close, Hide, Disable hoáº·c Destroy.
- Rá»i Dungeon thÃ¬ modal pháº£i Ä‘Æ°á»£c dá»n sáº¡ch.

## Technical Validation

- Fresh game cÃ³ tutorial.
- Existing save khÃ´ng cÃ³.
- Steps chá»‰ advance sau Ä‘Ãºng action.
- KhÃ´ng cÃ²n modal sau khi Ä‘á»•i screen.
- 0 compile errors vÃ  relevant tests pass.

## Visual Evidence

1080Ã—1920:

- HUD hint.
- Dungeon Start hint.
- Completion modal.

## Approval Gate

User + ChatGPT duyá»‡t wording, visual weight, timing vÃ  má»©c Ä‘á»™ khÃ´ng gÃ¢y phiá»n.

---

# Phase E1 â€” Character / Party / Equipment

**Initial status:** `NOT STARTED`

## Goal

LÃ m rÃµ roster, party, hero detail vÃ  equipment mÃ  khÃ´ng nhá»“i quÃ¡ nhiá»u thÃ´ng tin.

## Layout Direction

### Sticky Top

- Header.
- Party 1 / Party 2 / Party 3 tabs.
- 4 party member slots.

### Scrollable Body

- Hero roster.
- Portrait placeholder.
- Name.
- Level.
- Party state.
- In Expedition state.

### Selected Hero Detail

- Name, level, class.
- HP vÃ  XP.
- Stats.
- Weapon, armor, accessory.
- Selected state rÃµ.

### Actions

- Add to Party.
- Remove from Party.
- Dismiss.
- Equipment popup qua flow hiá»‡n cÃ³.

## Existing Backend Use

- DÃ¹ng `IDungeonService.IsCharacterOnExpedition(id)`.
- Disable action khÃ´ng an toÃ n cho hero Ä‘ang expedition.
- KhÃ´ng sá»­a party/equipment/dismiss formula.

## Technical Validation

- 3 party tabs hoáº¡t Ä‘á»™ng.
- Chá»n hero refresh detail.
- Equipment popup má»Ÿ Ä‘Ãºng slot.
- Equip/unequip cÃ²n hoáº¡t Ä‘á»™ng.
- In-expedition restrictions Ä‘Ãºng.
- 0 compile errors vÃ  relevant tests pass.

## Visual Evidence

1080Ã—1920:

- Character main.
- Selected hero detail.
- Equipment popup.
- In Expedition state.

## Approval Gate

User + ChatGPT duyá»‡t density, readability, hierarchy vÃ  action clarity.

---

# Phase E2 â€” Dungeon

**Initial status:** `NOT STARTED`

## Goal

TÃ¡ch rÃµ Dungeon Select, Active vÃ  Loot.

## Shared Rules

- Ná»n opaque, khÃ´ng see-through HUD/background.
- Expedition slot bar gá»n.
- Primary info trÆ°á»›c combat log.
- Primary action button lá»›n.
- KhÃ´ng sá»­a combat formula.

## Select State

- Expedition slots.
- Dungeon list.
- Dungeon banner placeholders.
- Difficulty/status.
- Party preview.
- Start Expedition.

## Active State

- Dungeon/room/turn.
- Party HP.
- Enemy HP.
- Progress/current action.
- Combat log secondary, scrollable.
- Recall.

## Loot State

- Completion summary.
- Item placeholders.
- Quantity vÃ  rarity border.
- Collect Loot.

## Technical Validation

- 3 expedition slots Ä‘á»™c láº­p.
- Start/Recall/Collect hoáº¡t Ä‘á»™ng.
- Combat refresh cÃ²n Ä‘Ãºng.
- Loot vÃ o Inventory Ä‘Ãºng.
- KhÃ´ng cÃ²n layering error.
- 0 compile errors vÃ  relevant tests pass.

## Visual Evidence

1080Ã—1920:

- Dungeon Select.
- Dungeon Active.
- Dungeon Loot.

## Approval Gate

User + ChatGPT duyá»‡t separation, HP/progress readability, log density vÃ  action hierarchy.

---

# Phase E3 â€” Tavern

**Initial status:** `NOT STARTED`

## Goal

Biáº¿n Tavern vÃ  Quarters tá»« debug-style thÃ nh fantasy management screen dá»… hiá»ƒu.

## Tavern Tab

- Timer.
- Capacity.
- Guest dossier lá»›n.
- Portrait placeholder.
- Name/class/stats/cost.
- Prev/Next secondary.
- Recruit primary.

## Quarters Tab

CÃ¡c card riÃªng:

- Quarters Capacity.
- Visitor Capacity.
- Visitor Speed.

Má»—i card cÃ³ level, cost, state vÃ  Upgrade.

## Technical Validation

- Guest selection hoáº¡t Ä‘á»™ng.
- Timer update.
- Recruit trá»« Ä‘Ãºng tiá»n vÃ  thÃªm hero.
- Full capacity block recruit.
- Upgrade dÃ¹ng logic cÅ©.
- 0 compile errors vÃ  relevant tests pass.

## Visual Evidence

1080Ã—1920:

- Tavern guest.
- Cannot-afford hoáº·c full-capacity.
- Quarters upgrade.

## Approval Gate

User + ChatGPT duyá»‡t dossier clarity, button hierarchy, spacing vÃ  empty/full states.

---

# Phase E4 â€” Inventory + Item Detail Modal

**Initial status:** `NOT STARTED`

## Goal

Táº¡o inventory icon-only vÃ  detail modal riÃªng.

## Inventory Grid

- 4 cá»™t táº¡i 1080Ã—1920.
- Icon-only card.
- Item placeholder.
- Rarity border.
- Quantity badge.
- Lock overlay.
- KhÃ´ng cÃ³ name/stats/description trong card.

## Item Detail Modal

Planned controller:

`Assets/_Game/Scripts/Runtime/UI/Inventory/ItemDetailModal.cs`

Hiá»ƒn thá»‹:

- Large item image placeholder.
- Name.
- Category.
- Rarity.
- Quantity.
- Stats.
- Description/effect.
- Lock state.
- Existing actions.

Actions:

- Use.
- Lock/Unlock.
- Go to Merchant.

Rules:

- KhÃ´ng sell trá»±c tiáº¿p trong Inventory.
- Modal á»Ÿ `PopupRoot`.
- Full-screen blocker.
- Tap outside khÃ´ng Ä‘Ã³ng.
- Close button only.
- ÄÃ³ng váº«n giá»¯ selected item.
- Lock/Unlock refresh modal vÃ  grid.

## Technical Validation

- Tabs filter Ä‘Ãºng.
- Chá»n Ä‘Ãºng item.
- Modal block raycast.
- Use theo backend cÅ©.
- Lock/Unlock persist vÃ  refresh.
- Go to Merchant theo flow cÅ©.
- 0 compile errors vÃ  relevant tests pass.

## Visual Evidence

1080Ã—1920:

- Inventory grid.
- Selected/locked item.
- Item Detail Modal.
- Empty inventory.
- Full inventory warning.

## Approval Gate

User + ChatGPT duyá»‡t card size, density, modal hierarchy vÃ  touch clarity.

---

# Phase E5 â€” Craft / Workshop

**Initial status:** `NOT STARTED`

## Goal

LÃ m Recipes, Queue vÃ  Completed dá»… Ä‘á»c vÃ  dá»… thao tÃ¡c.

## Layout

- Recipes / Queue / Completed tabs.
- Search bar rÃµ.
- Recipe list 1 hoáº·c 2 cá»™t dá»… Ä‘á»c táº¡i 1080Ã—1920.
- Result image placeholder.
- Ingredient sufficiency states.
- Craft button lá»›n.
- Queue progress.
- Completed Claim.

## Technical Validation

- Search filter realtime.
- Material checks Ä‘Ãºng.
- Start craft Ä‘Ãºng.
- Queue capacity cÃ²n hoáº¡t Ä‘á»™ng.
- Claim Ä‘Æ°a item vÃ o Inventory.
- 0 compile errors vÃ  relevant tests pass.

## Visual Evidence

1080Ã—1920:

- Recipes.
- Missing-material state.
- Queue.
- Completed/Claim.

## Approval Gate

User + ChatGPT duyá»‡t readability, search clarity, ingredient states vÃ  queue hierarchy.

---

# Phase E6 â€” Quest

**Initial status:** `NOT STARTED`

## Goal

Hiá»ƒn thá»‹ quest, progress, doctrine vÃ  reward Ä‘Ãºng backend.

## Layout

Má»—i quest card cÃ³:

- Title.
- Short description.
- Current/target.
- Progress bar.
- Reward preview.
- Active / Ready / Claimed state.

## Verified Rewards

- High rarity: Gems.
- Lower rarity: Doctrine Progress.
- KhÃ´ng Gold.
- KhÃ´ng XP.
- KhÃ´ng fabricated bonus.
- KhÃ´ng emoji trong text.

## Doctrine

- War.
- Economy.
- Growth.

## Technical Validation

- Progress Ä‘Ãºng.
- Doctrine cycle Ä‘Ãºng.
- Reward preview update Ä‘Ãºng.
- Claim grants Gems hoáº·c Doctrine Progress.
- 0 compile errors vÃ  relevant tests pass.

## Visual Evidence

1080Ã—1920:

- Active quest list.
- Ready-to-claim.
- Claimed.
- Doctrine selection.

## Approval Gate

User + ChatGPT duyá»‡t progress readability, reward clarity vÃ  doctrine interaction.

---

# Phase E7 â€” Merchant / Market

**Initial status:** `NOT STARTED`

## Goal

LÃ m rÃµ Buy, Sell vÃ  Listings mÃ  khÃ´ng thay economy behavior.

## Buy

- Regular Stock.
- Special Stock.
- Item placeholder.
- Price.
- Quantity.
- Buy.

Special Stock chá»‰ lÃ  section riÃªng, khÃ´ng pháº£i discount.

## Sell

- Selected Inventory item.
- Quantity.
- Lock status.
- Sell qua merchant backend hiá»‡n cÃ³.

KhÃ´ng cÃ³:

- Tax.
- Discount %.
- Custom price.
- Fabricated offer rules.

## Listings

- Active listings.
- Sold listings.
- Claim Gold.
- Empty state rÃµ.

## Technical Validation

- Buy trá»« Ä‘Ãºng Gold vÃ  nháº­n Ä‘Ãºng item.
- Sell táº¡o listing Ä‘Ãºng.
- Locked item khÃ´ng sell.
- Claim Sold cá»™ng Ä‘Ãºng Gold.
- 0 compile errors vÃ  relevant tests pass.

## Visual Evidence

1080Ã—1920:

- Buy.
- Sell.
- Listings.
- Sold/Claim.
- Empty stock/listing.

## Approval Gate

User + ChatGPT duyá»‡t pricing clarity, tab distinction, empty states vÃ  primary actions.

---

# Phase E8 â€” Settings & System Popups

**Initial status:** `NOT STARTED`

## Goal

HoÃ n thiá»‡n settings, reset safety vÃ  popup consistency.

## Interactive Settings

- Sound.
- Music.
- Vibration.
- Notifications.

## Read-Only

- Cloud Backup.
- Version.
- Language.

KhÃ´ng biáº¿n Cloud Backup thÃ nh toggle thá»© 5 náº¿u chÆ°a cÃ³ scope má»›i Ä‘Æ°á»£c duyá»‡t.

## Reset Data

- Danger zone.
- Reset.
- Confirm.
- Cancel.
- Warning hierarchy rÃµ.

## Popups

- Opaque modal.
- Raycast blocker.
- Header/body/action hierarchy.
- Close behavior.
- Typography thá»‘ng nháº¥t.

## Technical Validation

- 4 toggles save/load.
- Cloud Backup váº«n read-only.
- Reset cáº§n confirm.
- Confirmed reset dÃ¹ng backend hiá»‡n cÃ³.
- Popup block click phÃ­a dÆ°á»›i.
- 0 compile errors vÃ  relevant tests pass.

## Visual Evidence

1080Ã—1920:

- Settings main.
- Toggle states.
- Reset confirmation.
- Standard popup.
- Error/warning popup.

## Approval Gate

User + ChatGPT duyá»‡t safety, readability, toggle clarity, popup consistency vÃ  cohesion.

---

# Final UX/UI Review

**Initial status:** `NOT STARTED`

Chá»‰ báº¯t Ä‘áº§u sau khi E8 Ä‘Æ°á»£c approved.

## Required Review

- Má»Ÿ toÃ n bá»™ screen vÃ  state chÃ­nh.
- Kiá»ƒm tra typography, color, spacing, card, button vÃ  placeholder thá»‘ng nháº¥t.
- KhÃ´ng cÃ²n Unicode emoji.
- KhÃ´ng cÃ²n debug-style layout.
- KhÃ´ng cÃ²n background/layer see-through.
- Screenshot approved khá»›p project hiá»‡n táº¡i.
- Cháº¡y full EditMode regression suite.
- Ghi riÃªng cÃ¡c limitation ngoÃ i UX/UI.

## Final Evidence Set

1080Ã—1920:

- HUD.
- Character.
- Equipment Popup.
- Dungeon Select.
- Dungeon Active.
- Dungeon Loot.
- Tavern.
- Quarters.
- Inventory.
- Item Detail Modal.
- Craft.
- Quest.
- Merchant.
- Settings.
- Tutorial.
- System popup.

## Final Decision

Chá»‰ User + ChatGPT Ä‘Æ°á»£c ghi:

`UX/UI REDESIGN APPROVED`

APK build lÃ  quyáº¿t Ä‘á»‹nh riÃªng cá»§a User.

---

# Phase Tracker

| Phase | Status | Technical Evidence | User Review | ChatGPT Review |
|---|---|---|---|---|
| B â€” Font Foundation | NOT STARTED | Pending | Pending | Pending |
| C â€” HUD Prototype | NOT STARTED | Pending | Pending | Pending |
| D â€” Tutorial Light | NOT STARTED | Pending | Pending | Pending |
| E1 â€” Character/Party | NOT STARTED | Pending | Pending | Pending |
| E2 â€” Dungeon | NOT STARTED | Pending | Pending | Pending |
| E3 â€” Tavern | NOT STARTED | Pending | Pending | Pending |
| E4 â€” Inventory/Modal | NOT STARTED | Pending | Pending | Pending |
| E5 â€” Craft | NOT STARTED | Pending | Pending | Pending |
| E6 â€” Quest | NOT STARTED | Pending | Pending | Pending |
| E7 â€” Merchant | NOT STARTED | Pending | Pending | Pending |
| E8 â€” Settings/Popups | NOT STARTED | Pending | Pending | Pending |
| Final UX/UI Review | NOT STARTED | Pending | Pending | Pending |

