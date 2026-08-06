# Legacy UI Phase 2: Theme + Shape Foundation

> **Date:** 2026-08-04
> **Goal:** Build foundational legacy UI theme variables and procedurally generated reusable borders based on 1080×1920 reference layout.

---

## 1. Scale & Mapping Approach
Since the legacy game ran at approx xxhdpi (where 1dp ≈ 3px), mapping directly 1:1 on a 1080×1920 canvas would result in UI elements that are far too small. 

- **Scale Rule Applied:** `1 dp = 3 px`
- **Result:** The standard 10dp corner radius translates to 30px on our Canvas. A 1dp line stroke translates to a 3px stroke, keeping it crisp and visible.

## 2. Completed Foundation Files

### Scriptable Constants
- `Assets/_Game/Scripts/Runtime/UI/Legacy/LegacyUITheme.cs`
  - Created as static `readonly` constants (safer than ScriptableObject as it cannot be null or go missing).
  - Contains exact Hex to Color mappings (e.g., `#1e1e1e` for Dialog background).

### Procedural Editor Generator
- `Assets/_Game/Scripts/Editor/UI/Legacy/LegacyThemeBuilder.cs`
  - Added menu item: `Tools > Guild Master > Legacy UI > Build Legacy Theme Assets`.
  - Idempotent script: Procedurally draws rounded rects with anti-aliasing via signed distance calculations.
  - Automatically slices and configures ImportSettings (SpriteMeshType.FullRect, SpriteBorder = 32px, Bilinear scaling).

### Generated 9-Slice Assets (Option A)
The script generated these perfectly crisp 9-slice `.png` files directly from code without needing manual texture authoring:
- `dialog_border.png` (30px radius, 9px stroke, dark fill)
- `object_border_dim_white.png` (30px radius, 3px stroke)
- `object_border_brass.png` (30px radius, 3px stroke)
- `object_border_no_background.png` (30px radius, transparent fill)
- `object_border_ascended.png` (30px radius, ascended color)
- `object_border_rounded_left_ascended.png` (left corners only)

### Generated Reusable Component Prefabs
- `Assets/_Game/Prefabs/UI/Legacy/LegacyPanel.prefab`
- `Assets/_Game/Prefabs/UI/Legacy/LegacyDialogFrame.prefab`
- `Assets/_Game/Prefabs/UI/Legacy/LegacyCardFrame.prefab`
- `Assets/_Game/Prefabs/UI/Legacy/LegacyButtonFrame.prefab`
- `Assets/_Game/Prefabs/UI/Legacy/LegacyAscendedFrame.prefab`

*(All prefabs are barebones generic `Image` backgrounds using Image.Type.Sliced, cleanly separated from text/logic as requested).*

## 3. Test Scene & Validation
- **Path:** `Assets/_Game/Scenes/Tests/LegacyShapeTest.unity`
- **Script:** `TestSceneBuilder.cs` (available via `Tools > Guild Master > Legacy UI > Build Test Scene`)
- **Status:** The scene automatically initializes a Canvas (1080×1920, Match Width/Height 0.5) and spawns the 5 core prefabs in two sizes (Card/Button size 150×150, and standard Panel size 400×200) to prove the 9-slicing does not distort.

## 4. Rollback Steps
If this foundation needs to be discarded:
1. Delete `Assets/_Game/Scripts/Runtime/UI/Legacy/`
2. Delete `Assets/_Game/Scripts/Editor/UI/Legacy/`
3. Delete `Assets/_Game/Art/UI/Generated/`
4. Delete `Assets/_Game/Prefabs/UI/Legacy/`
5. *(Or restore from `D:\Tinh\Backups\Legacy_UI_Phase_2_Theme_Foundation`)*

## 5. Next Steps
The foundation is fully built and isolated. Claude is now ready to begin **Phase 3** (App Shell & Navigation).
