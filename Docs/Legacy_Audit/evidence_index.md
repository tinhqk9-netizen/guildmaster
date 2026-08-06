# Evidence Index

> Cross-reference of all audit evidence sources.

---

## Source Files Analyzed

### Java Source (Game Package Only)

| File | Path | Purpose |
|------|------|---------|
| `MainActivity.java` | `sources/it/.../MainActivity.java` | Entry point, HUD, navigation, all dialog guards |
| `HeadquartersFragment.java` | `sources/it/.../ui/headquarters/` | HQ tab, 6 building cards |
| `AdventurersFragment.java` | `sources/it/.../ui/adventurers/` | Adventurers tab |
| `DungeonsFragment.java` | `sources/it/.../ui/dungeons/` | Dungeons tab |
| `RaidsFragment.java` | `sources/it/.../ui/raids/` | Raids tab (conditional) |
| `UIUtils.java` | `sources/it/.../UIUtils.java` | UI helper methods |
| `Formulas.java` | `sources/it/.../Formulas.java` | Game formulas (capacity, stats) |
| `NonScrollableGridView.java` | `sources/it/.../ui/components/` | Custom GridView (no canvas) |
| 46× `Dialog*.java` | `sources/it/.../ui/dialogs/` | All feature dialogs |

### XML Resources

| File | Path | Purpose |
|------|------|---------|
| `AndroidManifest.xml` | `resources/` | Package name, permissions, activity |
| `mobile_navigation.xml` | `resources/res/navigation/` | Navigation graph (4 fragments) |
| `bottom_nav_menu.xml` | `resources/res/menu/` | Bottom tab definitions |
| `drawer_nav_menu.xml` | `resources/res/menu/` | Drawer menu items (10) |
| `activity_main.xml` | `resources/res/layout/` | Main activity layout (268 lines) |
| `fragment_headquarters.xml` | `resources/res/layout/` | HQ tab layout (273 lines) |
| `colors.xml` | `resources/res/values/` | Color definitions |
| `strings.xml` | `resources/res/values/` | All UI strings (486KB) |
| `styles.xml` | `resources/res/values/` | Theme/style definitions (445KB) |
| `dimens.xml` | `resources/res/values/` | Dimension values (39KB) |
| ~80 game-specific layout XMLs | `resources/res/layout/` | Dialog/item layouts |

---

## Generated Deliverables

| # | Deliverable | Path | Type | Content |
|---|-------------|------|------|---------|
| 1 | Audit Summary | `Docs/Legacy_Audit/legacy_audit_summary.md` | MD | Census, stats, Canvas audit |
| 2 | Asset Inventory | `Docs/Legacy_Audit/legacy_asset_inventory.csv` | CSV | 1,036 rows: name, dims, category, refs |
| 3 | Contact Sheets | `Docs/Legacy_Audit/Asset_Gallery/*.png` | PNG | 11 visual sheets by category |
| 4 | Screen Inventory | `Docs/Legacy_Audit/legacy_screen_inventory.md` | MD | 57 screens documented |
| 5 | Navigation Flow | `Docs/Legacy_Audit/legacy_navigation_flow.md` | MD | Mermaid diagram + flow details |
| 6 | Visual System | `Docs/Legacy_Audit/legacy_visual_system.md` | MD | Colors, fonts, spacing, patterns |
| 7 | Screen-Asset Map | `Docs/Legacy_Audit/legacy_screen_asset_map.csv` | CSV | 216 mappings across 58 screens |
| 8 | Gap Analysis | `Docs/Legacy_Audit/legacy_vs_rebuild_gap.md` | MD | 55 screens: 4 exist, 9 partial, 42 missing |
| 9 | Reconstruction Plan | `Docs/Legacy_Audit/legacy_reconstruction_plan.md` | MD | 9 phases, asset strategy, visual standards |
| 10 | Evidence Index | `Docs/Legacy_Audit/evidence_index.md` | MD | This file |
| 11 | Unresolved Questions | `Docs/Legacy_Audit/unresolved_questions.md` | MD | Open items |

---

## Scripts (Re-runnable)

| Script | Purpose | Output |
|--------|---------|--------|
| `generate_asset_inventory.py` | Scan drawables, classify, find refs | `legacy_asset_inventory.csv` |
| `generate_contact_sheets.py` | Create visual thumbnails | `Asset_Gallery/*.png` |
| `generate_screen_asset_map.py` | Map screens to assets | `legacy_screen_asset_map.csv` |
