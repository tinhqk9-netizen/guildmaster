# RESTORE_4 — DESIGNED SYSTEMS

**Goal:** Implement all designed features from Java spec that are currently missing.
**Effort:** ~3–5 days
**Dependencies:** RESTORE_1, RESTORE_2, RESTORE_3
**Risk:** 🔴 HIGH — largest implementation phase, 4 new systems
**Gate:** All systems functional individually + basic integration PASS

---

## Scope

This is the IMPLEMENTATION phase — where code is written, not just verified.

| System | Status | Effort |
|--------|--------|--------|
| Pets (G01) | 🔴 Missing entire system | ~2–3 days |
| Doctrine UI (G04) | 🔴 Missing screen | ~0.5 day |
| Promotion (G03) | 🔴 Missing system | ~1–2 days |
| Ascension (G02) | 🟠 Minimal (bool only) | ~0.5 day |
| Shelter fields (G13-G15) | 🟢 Decision needed | ~0.25 day |

**Total: ~3–5 days**

---

## Tasks

### T4-1: Pet System (G01 — FULL IMPLEMENTATION)

**Step 1 — Field schema:**
- Read Java source pets.json → document ALL fields per pet
- Define `PetDefinition.cs` fields:
  ```csharp
  public class PetDefinition : DefinitionBase
  {
      public string Description;
      public PetRarity Rarity; // Common, Rare, Epic, Legendary
      public int BaseAttack, BaseDefense, BaseSpeed, BaseHp;
      public float AttackMult, DefenseMult, SpeedMult, HpMult; // per level
      public int MaxLevel;
      public string SkillId;
      public string EvolutionPetId;
      public int EvolutionLevel;
      // other fields from pets.json
  }
  ```

**Step 2 — SaveData:**
  ```csharp
  [Serializable]
  public class PetSaveData
  {
      public string InstanceId;
      public string DefinitionId;
      public int Level;
      public int Exp;
      public bool IsEquipped;
      public string OwnerCharacterInstanceId;
  }
  ```
  Add `public List<PetSaveData> Pets = new List<PetSaveData>();` to SaveData + NormalizeAfterLoad guard

**Step 3 — Service:**
  `PetService` with: CreatePet, AddExp, LevelUp, Equip/Unequip, GetStats

**Step 4 — Combat integration:**
  Pet stats added to `CharacterService.GetTotalStat()` when pet equipped

**Step 5 — Loot integration:**
  Pet eggs/parts added to LootService drop tables

**Step 6 — UI:**
  PetScreen with grid of owned pets + detail + equip/unequip + stats

### T4-2: DoctrineScreen (G04 — NEW UI)

```csharp
public class DoctrineScreen : UIScreen
{
    // Show all 8 doctrines: level + progress bar
    // Uses existing DoctrineService
    
    public override void Show()
    {
        var ds = ServiceLocator.Get<IDoctrineService>();
        // For each of 8 doctrines:
        doctrineLabels[i].text = $"{name}: Lv{ds.GetLevel(type)}";
        doctrineBars[i].fill = ds.GetProgress(type);
    }
}
```

**Prefab:** 8 rows, each with:
- Doctrine name
- Level text
- Progress bar (Slider or Image.fillAmount)
- Maxed indicator when level = max

**Registration:** Add to `UIScreenId.Doctrine`, register in UIService, add button to HUD/Menu.

### T4-3: Promotion System (G03 — FULL IMPLEMENTATION)

**Step 1 — Design (from Java source or decide):**
- How many tiers? (e.g., 5: Bronze/Silver/Gold/Legend/Myth)
- Requirements per tier? (level, gold, items, quest)
- Stat multipliers per tier?
- Does promoting reset level? (common in gacha)
- Do bonuses stack?

**Step 2 — Data model:**
  ```csharp
  [Serializable]
  public class PromotionDefinition : DefinitionBase
  {
      public int Tier;
      public int RequiredLevel;
      public int GoldCost;
      public List<ItemCost> RequiredItems;
      public float StatMultiplier;
      public string Title;
      public string RequiredQuestId;
  }
  ```

**Step 3 — SaveData:**
  Add `public int PromotionTier;` to `CharacterSaveData`

**Step 4 — Service:**
  `PromotionService` with CanPromote, Promote, GetNextTier

**Step 5 — UI:**
  Promotion button on CharacterScreen, tier indicator, cost display

### T4-4: Ascension Enhancement (G02 — FROM BOOL TO FULL)

**Step 1 — Change `IsAscended` (bool) to `AscensionLevel` (int):**
  ```csharp
  // SaveData.CharacterSaveData
  public int AscensionLevel; // 0 = base, 1-10 = ascended
  ```

**Step 2 — Cost formula:**
  ```csharp
  // FormulaService
  public AscensionCost GetAscensionCost(CharacterRuntime character)
  {
      return new AscensionCost
      {
          GoldCost = (int)(500 * Math.Pow(1.5, character.AscensionLevel)),
          GemsCost = character.Rarity == Rarity.Legendary ? 50 : 100,
          RequiredLevel = 50 + (character.AscensionLevel * 10),
          RequiredItem = "ascension_stone",
          RequiredItemCount = character.AscensionLevel + 1
      };
  }
  ```

**Step 3 — Stat multiplier:**
  ```csharp
  // CharacterService
  double ascMult = 1.0 + (character.AscensionLevel * 0.1); // 1.0x → 2.0x at max
  ```

**Step 4 — UI:**
  Show AscensionLevel on CharacterScreen, "Ascend" button when eligible

### T4-5: Shelter Fields Decision (G13-G15)

**Must decide ONE of:**

| Option | Action | Effort | Pros | Cons |
|--------|--------|--------|------|------|
| **REUSE** | Rename LevelShelter→LevelQuarters (alias), UpgradeShelter→UpgradeQuarters | ~0.5d | No data loss | Confusing dual naming |
| **MIGRATE** | Add migration: v0→v1 copies shelter→quarters | ~1d | Clean schema | Old saves need migration |
| **DEPRECATE** | Mark as [Obsolete] "Use LevelQuarters instead" | ~0.1d | Simple | Dead fields in save |
| **KEEP** (for autofeed) | LevelShelterAutofeed kept for RESTORE_4 autofeed | ~0.1d | Ready for autofeed | Need to verify intent |

**Recommendation:** Keep `LevelShelterAutofeed` (for potential autofeed feature), deprecate `LevelShelter`/`UpgradeShelter` with `[Obsolete]` and use quarters equivalents instead.

---

## Verification Gate — RESTORE_4 PASS Criteria

| Check | Method | Status |
|-------|--------|--------|
| PetDefinition fields match pets.json schema | Code review | ⬜ |
| PetService CreatePet → Save → Load → Pet exists | NOT_RUN (needs editor) | GATE |
| Pet equipped → stats applied to combat | NOT_RUN | GATE |
| PetScreen shows pets, equip/unequip works | NOT_RUN | GATE |
| DoctrineScreen shows all 8 doctrines + progress | NOT_RUN (needs editor) | GATE |
| Doctrine progress updates after quest claim | NOT_RUN | GATE |
| PromotionDefinition matches design spec | Design review | ⬜ |
| Promote → Tier increases → stat multiplier applied | NOT_RUN (needs editor) | GATE |
| Promotion UI shows cost + requirements | NOT_RUN | GATE |
| AscensionLevel (int) replaces IsAscended (bool) | Code review | ⬜ |
| Multiple ascension levels work + cost scaling | NOT_RUN | GATE |
| Stat multiplier with ascension computed correctly | Code review | ⬜ |
| Shelter fields decision documented + applied | Decision doc | ⬜ |
| NormalizeAfterLoad guards all new SaveData fields | Code review | ⬜ |
| Save → Load → Play cycle with new features | NOT_RUN | GATE |
