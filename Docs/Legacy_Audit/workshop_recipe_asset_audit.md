# Workshop Recipe Icon Audit

Scope: Workshop → Recipes screen only. Investigation of grey placeholder icons on recipe
cards ("Absolutezero", "Abyssalcompendium", "Abyssalcutlass", etc.), plus their "Unavailable" /
"No ingredients defined" status. No screenshots were captured or generated for this audit
(per constraint) — findings are derived from static code/data analysis and Unity Editor
compile/test runs only.

## 1. Current symptom

`WorkshopRecipePanel` renders one card per `RecipeDefinition` returned by
`GameDatabase.GetAll<RecipeDefinition>()`. Each card's icon comes from
`LegacySpriteRegistry.GetItemSprite(recipe.OutputItemId)`. For the vast majority of recipes this
returned `null` before the fix in this audit, producing the grey/transparent placeholder seen in
the screenshot. Separately, most recipe cards also show "Unavailable" and "No ingredients
defined" — a **different, unrelated** symptom caused by the recipe's `Ingredients` list being
genuinely empty in the shipped data (see §7).

## 2. Total recipe statistics (exact counts)

| Metric | Count | % |
|---|---|---|
| Total recipes (`Assets/StreamingAssets/GameData/recipes.json`) | 321 | 100% |
| Recipes with a non-empty `OutputItemId` | 321 | 100% |
| Recipe `OutputItemId` values that exactly equal a real `ItemDefinition.id` | 12 | 3.7% |
| Recipes with a non-empty `Ingredients` list | 78 | 24.3% |
| Recipes with an **empty** `Ingredients` list (`manualRuleRequired: true`) | 243 | 75.7% |
| Recipes with `parseStatus: "partial"` (upstream decompile-parse flagged incomplete) | 320 | 99.7% |
| Total items (`Assets/StreamingAssets/GameData/items.json`) | 607 | — |
| Items with an `idImage` field populated in raw JSON | 607 | 100% |
| `ItemDefinition` C# class fields for icon/sprite | **0 — no such field exists** | — |
| Item ids that resolve to a real sprite **exactly** (`item.id` == catalog key) | 596 / 607 | 98.2% |
| Recipe icons resolving to a real sprite **before** this audit's fix (exact match only) | 12 / 321 | 3.7% |
| Recipe icons resolving to a real sprite **after** this audit's fix | 320 / 321 | 99.7% |
| Recipes still on the grey fallback after the fix | 1 / 321 | 0.3% |

## 3. Lookup pipeline (exact file:line trace)

1. `Assets/_Game/Scripts/Runtime/UI/Headquarters/WorkshopRecipePanel.cs:82` —
   `WorkshopRowBuilder.CreateRow(_listContent, _rowBorderSprite, recipe.OutputItemId, 1, ...)`
   → the raw `RecipeDefinition.OutputItemId` string is passed straight through as the row's
   `definitionId`. Nothing resolves it to an `ItemDefinition` first.
2. `Assets/_Game/Scripts/Runtime/UI/Headquarters/WorkshopRowBuilder.cs:44` —
   `Sprite iconSprite = !string.IsNullOrEmpty(definitionId) ? LegacySpriteRegistry.GetItemSprite(definitionId) : null;`
   → `definitionId` here is literally `recipe.OutputItemId`.
3. `Assets/_Game/Scripts/Runtime/UI/Legacy/LegacySpriteRegistry.cs:139` —
   `GetItemSprite(itemSpriteName) => GetSprite(itemSpriteName)` — a straight dictionary lookup,
   no normalization (before this audit's fix).
4. `Assets/_Game/Scripts/Runtime/UI/Legacy/LegacySpriteCatalog.cs` — the dictionary backing
   `GetSprite` is keyed by the **original decompiled PNG filename stem** (e.g. `absolute_zero`),
   built once by the Editor tool in §5.

The row's status text (`"No ingredients defined."`) and Craft-button label
(`"Unavailable"`/`"Missing"`) are built independently in `WorkshopRecipePanel.cs:109-135` from
`recipe.Ingredients` and `ICraftService.CanCraft(recipe.id)` — this path never touches sprites and
was **not** changed.

## 4. Parser findings (raw JSON samples)

`RecipeDefinition` (`Assets/_Game/Scripts/Definitions/RecipeDefinition.cs`) has exactly two
data fields: `OutputItemId` (string) and `Ingredients` (`List<IngredientData>`). `ItemDefinition`
(`Assets/_Game/Scripts/Definitions/ItemDefinition.cs`) inherits `id` from `DefinitionBase` but has
**no icon/sprite field at all** — the raw JSON's `idImage` value is never copied into the C# model
anywhere in the runtime pipeline (`ItemFieldsLoader.cs` only copies `constitution`, `dexterity`,
`intelligence`, `defense`, `magicDefense`, `maxHp`). This is fine for the current UI, because
every screen (including Workshop) looks up sprites by `item.id`/`recipe.OutputItemId` directly
rather than by a separate icon field — but it does mean the `idImage` value in the raw data is
completely unused today.

10 concrete samples (raw JSON, `Assets/StreamingAssets/GameData/recipes.json` /
`items.json`):

| Recipe id | `OutputItemId` (raw) | Real `ItemDefinition.id` | Match? | `Ingredients` |
|---|---|---|---|---|
| `recipe_absolutezero` | `absolutezero` | `absolute_zero` | No (underscores stripped) | `[]` (rawArgs truncated: `Item.getInstance("IceCage", 1`) |
| `recipe_abyssalcompendium` | `abyssalcompendium` | `abyssal_compendium` | No | `[{"ItemId":"missingpage","Amount":50}]` |
| `recipe_abyssalcutlass` | `abyssalcutlass` | `abyssal_cutlass` | No | `[]` (rawArgs truncated) |
| `recipe_abyssalgoo` | `abyssalgoo` | `abyssal_goo` | No | `[]` (rawArgs truncated) |
| `recipe_abyssalingot` | `abyssalingot` | `abyssal_ingot` | No | `[]` (rawArgs truncated) |
| `recipe_aegismechanica` | `aegismechanica` | (see items.json) | No | `[]` |
| `recipe_amuletofresurrection` | `amuletofresurrection` | (see items.json) | No | `[]` |
| `recipe_amuletoftheswordsman` | `amuletoftheswordsman` | (see items.json) | No | `[]` |
| `recipe_ancientarmor` | `ancientarmor` | (see items.json) | No | `[]` |
| `recipe_ancientboots` | `ancientboots` | (see items.json) | No | `[]` |

**Root-cause parser located**: `Tools/DecodeConverter/parsers/recipe_parser.py:45` —

```python
out_id = enum_id.lower()
```

The recipe extractor reads Java enum constant names (e.g. `AbsoluteZero`) straight out of the
decompiled `Recipes.java` file and only lower-cases them — it never converts `PascalCase` to
`snake_case`. The item extractor (whichever parser built `items.json`) evidently *does* convert
class names to `snake_case` ids (`AbsoluteZero` → `absolute_zero`). This casing-convention
mismatch between two independent parsers in `Tools/DecodeConverter/parsers/` is why 309/321
(96.3%) recipe `OutputItemId` values don't match any real `ItemDefinition.id`.

Separately, the same parser's ingredient regex
(`Item\.getInstance\(\"([A-Za-z0-9_]+)\"\s*(?:,\s*(\d+))?\)`) combined with the enclosing
non-greedy enum-constant regex (`\((.*?)\)(?:,|\z)`) fails to capture multi-argument /
multi-line `getInstance(...)` calls in the original Java source, leaving `rawArgs` visibly
truncated (e.g. `"Item.getInstance(\"IceCage\", 1"` — no closing paren) and `Ingredients: []`
for 243/321 (75.7%) recipes.

**This is a data-generation bug in the Python conversion pipeline, not in the Unity runtime
code.** Per this audit's constraints, `recipes.json` (recipe output/ingredient data) was **not**
modified.

## 5. Registry/catalog findings

`Assets/_Game/Scripts/Runtime/UI/Legacy/LegacySpriteRegistry.cs` — confirmed `GetItemSprite`
is a thin wrapper over `GetSprite`, a single `Dictionary<string, Sprite>` lookup keyed by exact
catalog entry key (no normalization, before the fix).

`Assets/_Game/Scripts/Editor/LegacyAssetImporter.cs` (`Tools/Guild Master/Legacy UI/Import
Legacy Assets`, `BuildCatalog` at line 153) builds
`Assets/Resources/LegacySpriteCatalog.asset` by scanning **every** `.png` under
`Assets/_Game/Art/Legacy/<Category>/` (all 12 category folders), keying each entry by
`Path.GetFileNameWithoutExtension(path)` — the filename stem, case-preserved, verbatim. Filenames
that collide across two different category folders are deliberately excluded from the catalog
(ambiguous — logged, not fixed automatically). No duplicate stems were found across the full
1,032-file legacy art set at audit time, so this exclusion path currently affects 0 entries.

## 6. Asset filename findings

`Assets/_Game/Art/Legacy/` contains 1,032 PNG files across all 12 category folders (558 of them
under `Items/`, but sprites for craftable items were also found miscategorized under
`Currency/` (`crystal_dagger.png`) and `Navigation/` (`archaic_amulet.png`,
`titanic_might.png`) — the catalog importer scans all folders together so this
miscategorization does not affect lookups, only the folder organization).

Cross-referencing all 321 recipe `OutputItemId` values (normalized: underscores stripped,
lower-cased) against all 1,032 legacy PNG filename stems (same normalization) found **zero
normalization collisions** — every normalized form maps to at most one real file. 320/321
recipe output ids have a unique matching asset once normalized this way; only 1 does not
(`celestialmercy` — the real item is `celestial_mercy`, but its own source PNG was itself
misnamed `celestials_mercy.png`, plural, by the original game — a genuine upstream art-naming
inconsistency, not an id-format issue).

## 7. Root-cause categories (counts)

| Category | Definition | Count | % of 321 |
|---|---|---|---|
| **A** | Asset exists, mapping already correct, sprite resolved before any fix | 12 | 3.7% |
| **B** | Asset exists, but the recipe's `OutputItemId` key didn't match the catalog key due to a missing-underscore casing mismatch — fixed by this audit's normalized-fallback lookup | 308 | 96.0% |
| **C** | Item data valid but never made it into the sprite catalog (catalog-builder gap) | 0 | 0% |
| **D** | Recipe `OutputItemId` itself doesn't correspond to any real `ItemDefinition.id` (upstream parser bug, `Tools/DecodeConverter/parsers/recipe_parser.py:45`) — **icon now resolves via the same normalized fallback that fixes category B, but the id itself remains data-invalid** (see §9) | 309 (overlaps with B for 308 of them) | 96.3% |
| **E** | Genuinely no source asset exists under any name/normalization, even checked against the plain filename set | 1 (`recipe_celestialmercy`) | 0.3% |

Category B and D overlap heavily: 308 of the 309 recipes whose `OutputItemId` doesn't match a
real item id are *also* the ones fixed by the normalized sprite-key fallback (because the
underlying asset filename normalizes to the same string as the malformed id). The 309th
(`celestialmercy`) is both data-invalid **and** has no matching asset (category E).

No recipes fell into category C — every item's PNG made it into the catalog.

## 8. Safe fix applied

**File**: `Assets/_Game/Scripts/Runtime/UI/Legacy/LegacySpriteRegistry.cs`

Added a second, normalized index (`_normalizedCache`, key = catalog key with `_` removed and
lower-cased) built alongside the existing exact-match `_cache` in the `Cache` property getter
(lines 41-95). Entries whose normalized form collides with another catalog entry are excluded
from this fallback index entirely (never guesses between two candidates for the same normalized
key — verified 0 collisions exist across the full legacy art set, see §6).

`GetSprite(string legacyName)` (lines 104-130) now falls back to `_normalizedCache` only when
the exact-key lookup misses. `ClearCache()` was updated to also clear `_normalizedCache`.

This is a general rule applied once in the shared registry — every current and future caller of
`LegacySpriteRegistry.GetSprite`/`GetItemSprite`/etc. benefits automatically. No per-recipe
hardcoding was added. No recipe/item JSON data, crafting formulas, ingredients, or `SaveData`
were touched.

```csharp
// Assets/_Game/Scripts/Runtime/UI/Legacy/LegacySpriteRegistry.cs
private static Dictionary<string, Sprite> _normalizedCache;
...
string normalized = Normalize(entry.key);
if (normalizedSeen.Contains(normalized)) { normalizedAmbiguous.Add(normalized); continue; }
normalizedSeen.Add(normalized);
_normalizedCache[normalized] = entry.sprite;
...
private static string Normalize(string key) => key.Replace("_", string.Empty).ToLowerInvariant();
```

```csharp
public static Sprite GetSprite(string legacyName)
{
    ...
    if (Cache.TryGetValue(legacyName, out Sprite sprite)) { ... return sprite; }
    _ = Cache;
    if (_normalizedCache != null && _normalizedCache.TryGetValue(Normalize(legacyName), out Sprite normalizedSprite))
        return normalizedSprite;
    LogMissingOnce(legacyName, "no catalog entry for this name");
    return null;
}
```

No other files were modified. `WorkshopRecipePanel.cs` and `WorkshopRowBuilder.cs` were read but
left unchanged — the fix lives entirely in the shared sprite registry, which is the correct single
point of ownership for "legacy name → sprite" resolution.

## 9. Remaining true missing assets (exact IDs)

Only one recipe still renders the grey fallback after the fix:

- `recipe_celestialmercy` → `OutputItemId: "celestialmercy"` → real item `celestial_mercy`
  → real source art filename `celestials_mercy.png` (plural — a genuine upstream naming
  inconsistency in the original decompiled art, not fixable by any general id-normalization rule
  without risking false matches elsewhere). Left on fallback as legitimate category E.

## 10. Remaining data gaps (not fixed — out of scope per audit constraints)

1. **309/321 recipe `OutputItemId` values remain data-invalid** relative to `ItemDefinition.id`
   (see §4/§7 category D). The icon now displays correctly for 308 of these thanks to the
   sprite-registry fallback, but the underlying id mismatch is **not** cosmetic-only:
   `CraftService.TryStartCraft` (`Assets/_Game/Scripts/Runtime/Services/CraftService.cs:135`)
   stores `DefinitionId = recipe.OutputItemId` on the crafted `ItemActionSaveData`. For any of
   these 309 recipes that *did* have real ingredients and were craftable, the resulting
   inventory item would carry an id that doesn't match any real `ItemDefinition` — breaking
   stat/price/category lookups for that crafted item elsewhere in the game (Storage, Market,
   etc.). This was not touched because fixing it requires changing `recipes.json` output-id data,
   explicitly out of scope for this audit.
2. **243/321 recipes have empty `Ingredients`** (`manualRuleRequired: true`), which is why most
   cards show "No ingredients defined." and a "Unavailable" Craft button
   (`CraftService.CanCraft` returns `Fail(CraftFailureReason.InvalidIngredients)` for any recipe
   with an empty ingredient list — `CraftService.cs:84-87`). This is the shipped data, faithfully
   reflected by the UI; not a UI bug. Root cause is the same upstream parser's incomplete
   `getInstance(...)` regex match (§4).
3. **11 items** have an `id` that does not directly match any PNG filename, but their `idImage`
   field does (e.g. `avian_egg` → real art `egg_avian.png`; `celestial_mercy` → real art
   `celestials_mercy.png`). These are word-order/pluralization mismatches that the underscore-
   stripping fallback cannot and should not attempt to resolve (risk of wrong-item false
   matches). None of the 12 category-A (already-working) recipe output ids are among these 11, so
   this does not affect the Workshop Recipes screen today, but could affect Storage/Market
   screens if they ever pass these 11 item ids to `LegacySpriteRegistry`. Flagged for awareness,
   not fixed (outside Workshop-recipe scope of this audit).
4. **Locked/undiscovered-recipe display**: `Docs/Legacy_Audit/legacy_screen_asset_map.csv`,
   `deep_dynamic_loading.csv`, and `legacy_visual_system.md` were checked; no evidence of a
   legacy "silhouette/lock" convention specific to undiscovered Workshop recipes was found in
   the existing audit CSVs, and no decompiled Workshop/Recipe layout XML was located under
   `D:\Tinh\Guild Master - Idle Dungeons\resources\res\layout\` (searched for
   `*workshop*`/`*craft*`/`*recipe*` — no matches). The "Unavailable" state observed in the
   screenshot is fully explained by §10.2 (empty ingredients data) rather than an intentional
   discovery-gate mechanic, so no separate "locked recipe" design conclusion could be confirmed
   either way from available legacy sources.

## 11. Exact fallback recipe ID list

After the fix, exactly one recipe id remains on the grey fallback icon:

- `recipe_celestialmercy`

(Full per-recipe detail for all 321 recipes — including which of the 320 resolved via exact vs.
normalized-fallback match — is in `Docs/Legacy_Audit/workshop_recipe_sprite_audit.csv`.)

## 12. Files modified

- `Assets/_Game/Scripts/Runtime/UI/Legacy/LegacySpriteRegistry.cs` — added normalized-key
  fallback lookup (see §8). This is the only production file changed.

Files read/investigated but **not** modified: `WorkshopDialog.cs`, `WorkshopRecipePanel.cs`,
`WorkshopRowBuilder.cs`, `LegacySpriteCatalog.cs`, `LegacyThemeSprites.cs`,
`RecipeDefinition.cs`, `ItemDefinition.cs`, `DefinitionBase.cs`, `ItemFieldsLoader.cs`,
`LegacyAssetImporter.cs`, `CraftService.cs`, `recipes.json`, `items.json`,
`Tools/DecodeConverter/parsers/recipe_parser.py`.

## 13. Runtime verification result

- `mcp__mcp-unity__recompile_scripts`: **0 errors, 0 warnings**.
- `mcp__mcp-unity__run_tests` (EditMode): **171/171 passed, 0 failed, 0 skipped** — matches the
  pre-change baseline exactly, no regressions.

## 14. Rollback steps

If this change needs to be reverted:

```
copy /Y "D:\Tinh\Backups\Legacy_UI_Workshop_Recipe_Asset_Audit\UI\Legacy\LegacySpriteRegistry.cs" ^
        "D:\Tinh\Rebuild_GuildMaster\Assets\_Game\Scripts\Runtime\UI\Legacy\LegacySpriteRegistry.cs"
```

(PowerShell/bash equivalent: copy the single backed-up file back over the current one.) A full
backup of every file this audit touched or read (`WorkshopDialog.cs`, `WorkshopRecipePanel.cs`,
`WorkshopRowBuilder.cs`, `LegacySpriteRegistry.cs`, `LegacySpriteCatalog.cs`,
`LegacyThemeSprites.cs`, `RecipeDefinition.cs`, `ItemDefinition.cs`, and the entire
`Docs/Legacy_Audit/` folder as it stood before this audit) is at
`D:\Tinh\Backups\Legacy_UI_Workshop_Recipe_Asset_Audit\`. Only `LegacySpriteRegistry.cs` was
actually changed, so only that one file needs restoring to fully roll back.
