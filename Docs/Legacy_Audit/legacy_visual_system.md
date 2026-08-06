# Legacy Visual System

> Color palette, typography, spacing, and component patterns.
> All values extracted from `colors.xml`, layout XMLs, and Java source.

---

## Color Palette (Game-Specific)

### Core Colors

| Name | Hex | Alpha | Usage |
|------|-----|-------|-------|
| `standard_background` | `#1fffffff` | 12% white | Card/section backgrounds |
| `standard_background_unavailable` | `#1fff0000` | 12% red | Disabled/unavailable cards |
| `lower_drawer_body` | `#282828` | 100% | Bottom nav bg, drawer bg |
| `lower_drawer_header` | `#505050` | 100% | Drawer header bg |
| `dim_white` | `#c8c8c8` | 100% | Primary text, drawer text |
| `black` | `#000000` | 100% | True black |

### Accent / Brass System

| Name | Hex | Alpha | Usage |
|------|-----|-------|-------|
| `brass_border` | `#befaa03e` | 75% | Tutorial borders, highlights |
| `brass_filler` | `#faa03e` | 100% | Brass accent, "NEW" label, tavern highlight |

### Rarity Border Colors

| Rarity | Hex | Alpha | Evidence |
|--------|-----|-------|----------|
| None | `#80000000` | 50% black | `border_rarity_none` |
| Common | `#80969696` | 50% gray | `border_rarity_common` |
| Uncommon | `#8032ff32` | 50% green | `border_rarity_uncommon` |
| Rare | `#bf3200ff` | 75% blue-purple | `border_rarity_rare` |
| Epic | `#80ff6400` | 50% orange | `border_rarity_epic` |
| Legendary | `#80ff0000` | 50% red | `border_rarity_legendary` |

### Ascended Unit

| Name | Hex | Alpha | Usage |
|------|-----|-------|-------|
| `ascended_unit` | `#ffdb7f` | 100% | Ascended unit name/border |
| `ascended_background` | `#1fffdb7f` | 12% | Ascended card bg |
| `ascended_background_extra_opaque` | `#4dffdb7f` | 30% | Highlight bg |

### HP/Combat

| Name | Hex | Usage |
|------|-----|-------|
| `hp_bar` | `#ff5a5a` | HP bar fill |
| `hp_bar_summary` | `#b3ff5a5a` | HP bar summary (70%) |

### Doctrine

| Name | Hex | Usage |
|------|-----|-------|
| `background_doctrine_icon` | `#353535` | Doctrine icon bg |
| `background_doctrine_level` | `#141414` | Doctrine level bg |

### Dungeon

| Name | Hex | Usage |
|------|-----|-------|
| `dungeon_log_lighter_background` | `#0fffffff` | Log entry alternating bg |

### UI Elements

| Name | Hex | Usage |
|------|-----|-------|
| `item_outer_border` | `#54ffffff` | Item card outer border |

---

## Typography

### Font
- **No custom font** (`res/font/` does not exist)
- Uses Android system default (Roboto on most devices)
- Evidence: No font resource folder found

### Text Sizes (from XML layouts)

| Size | Usage | Evidence |
|------|-------|----------|
| 20sp | Section titles (building names, fragment titles) | `fragment_headquarters.xml`, `activity_main.xml` |
| 16sp | Body text, currency amounts, tutorial body | `activity_main.xml` lines 138, 215 |
| 14sp | Default (Android default) | Most TextViews without explicit size |
| 12sp | Dots, badges, small indicators | `fragment_headquarters.xml` line 79 |

### Text Styles

| Style | Usage |
|-------|-------|
| `bold` | Titles, names, currency amounts, tutorial title |
| `bold|italic` | "NEW" label on tavern |
| Default (regular) | Descriptions, body text |

---

## Spacing & Dimensions

### Container Padding

| Element | Padding | Evidence |
|---------|---------|----------|
| activity_main top bar | 16dp LR, 16dp top, 4dp bottom | `activity_main.xml` lines 179-182 |
| activity_main currency bar | 16dp LR, 2dp top, 4dp bottom | `activity_main.xml` lines 14-17 |
| HQ fragment container | 16dp all sides | `fragment_headquarters.xml` line 4 |
| Building cards | 12dp all sides | `fragment_headquarters.xml` line 21 |
| Tooltip icons | 8dp all sides | `activity_main.xml` passim |

### Margins

| Between | Margin | Evidence |
|---------|--------|----------|
| Building cards | 16dp top | `fragment_headquarters.xml` line 51 |
| Tooltip icons | 16dp top, 16dp start | `activity_main.xml` lines 44-45 |
| Gem text from icon | 4dp start | `activity_main.xml` line 221 |
| Tutorial title from icon | 16dp start | `activity_main.xml` line 152 |

### Sign Icons (Building Signs)

| Dimension | Value | Evidence |
|-----------|-------|----------|
| Width | 42dp | `fragment_headquarters.xml` line 218 |
| Height | 54dp | `fragment_headquarters.xml` line 219 |
| Position | 24dp start, 1dp top margin | lines 222-223 |

### Notification Dots

| Dimension | Value | Evidence |
|-----------|-------|----------|
| Width | 12dp | `activity_main.xml` line 77 |
| Height | 12dp | line 78 |
| Offset | -3dp top, -3dp end | lines 79-80 |

---

## Component Patterns

### 1. Card Pattern (object_border_dim_white)
Used for: building cards, tooltip icons, gem container, items.
- Background: `@drawable/object_border_dim_white` (XML drawable — dim white border shape)
- Inner padding: 8dp (icons) or 12dp (cards)
- Content: centered text (title + description)

### 2. Dialog Pattern (custom_dialog)
- Base: `CustomDialog.java` extends `DialogFragment`
- Layout: `custom_dialog.xml` as base
- Full content loaded into dialog

### 3. Notification Dot Pattern
- 12×12dp `@drawable/brass_circle`
- Positioned at top-end of parent with -3dp offset
- Visibility toggled by boolean condition

### 4. Building Card Pattern (Headquarters)
```
┌─────────────────────────────────────────────┐
│ [Sign 42×54]  BUILDING NAME (bold 20sp)     │
│               description text              │
│               (count / capacity)            │
└─────────────────────────────────────────────┘
```
- Background: `object_border_dim_white`
- Sign image overlaps from left (24dp start, 1dp top)
- Name centered, description centered below

### 5. HUD Icon Pattern
```
┌────┐
│ 🔲 │  ← icon with object_border_dim_white bg, 8dp padding
└────┘
```
- Icons are ImageView with `srcCompat`
- Clickable → opens Dialog
- Optional notification dot at top-right

---

## Layout Architecture

### activity_main.xml Structure (268 lines)
```
DrawerLayout (root)
├── ConstraintLayout (container)
│   ├── ConstraintLayout (top bar — constraintLayout)
│   │   ├── ImageView (menu_button)
│   │   ├── TextView (fragment_name)
│   │   └── ConstraintLayout (container_gems)
│   │       ├── ImageView (image_gems)
│   │       └── TextView (amount_gems)
│   ├── ConstraintLayout (currency bar — constraintLayout2)
│   │   └── include (layout_money)
│   ├── ConstraintLayout (tooltip icons — containerTooltips)
│   │   ├── ImageView (shop, king_message, merchant, quests, ad, adfree)
│   │   └── ImageView (new_items, quests_notification — dots)
│   ├── ConstraintLayout (tutorial — containerTutorial)
│   │   ├── ImageView (tutorial_icon)
│   │   ├── TextView (tutorial_title, tutorial_step, tutorial_body)
│   ├── ViewPager2 (pager — fills remaining space)
│   └── BottomNavigationView (nav_view — bottom)
└── NavigationView (nav_view_drawer — drawer)
```

### Vertical Stack Order (top to bottom)
1. Top bar (menu + title + gems)
2. Currency bar
3. Tooltip icons row
4. Tutorial section (conditional)
5. ViewPager2 (main content — fragments)
6. BottomNavigationView (tabs)
