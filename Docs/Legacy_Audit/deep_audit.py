"""Master deep audit script: reads all game layouts, dialogs, fragments, XML drawables,
extracts hierarchy, assets, strings, colors, dimens, and generates detailed CSVs."""
import os, csv, re, json
from pathlib import Path
from xml.etree import ElementTree as ET

BASE = Path(r"D:\Tinh\Guild Master - Idle Dungeons\resources\res")
SRC = Path(r"D:\Tinh\Guild Master - Idle Dungeons\sources\it\paranoidsquirrels\idleguildmaster")
OUT = Path(r"D:\Tinh\Rebuild_GuildMaster\Docs\Legacy_Audit")

LAYOUT_DIR = BASE / "layout"
DRAWABLE_DIR = BASE / "drawable"
VALUES_DIR = BASE / "values"

NS = {"android": "http://schemas.android.com/apk/res/android",
      "app": "http://schemas.android.com/apk/res-auto"}

# ============= PART 1: Reclassify uncategorized assets =============
def reclassify_assets():
    """Reclassify 685 uncategorized assets into proper categories."""
    csv_path = OUT / "legacy_asset_inventory.csv"
    out_path = OUT / "legacy_asset_inventory_v2.csv"

    UNIT_PATTERNS = {
        "adventurer_class": re.compile(r"^unit_(knight|warrior|rogue|mage|paladin|ranger|cleric|monk|barbarian|bard|druid|templar|inquisitor|marksman|sureshot|juggernaut|lorekeeper|trickster|minstrel|justiciar|iron_defender|iron_warden|holy_knight|horse_rider|guard|royal_|light_disciple|silver_tongue|nightblade)"),
        "enemy_boss": re.compile(r"^unit_(king_aino|sha_|tekeli|kabar|kasimir|legate_hadrian|thorvus|lazarus|herald_|smoldering_titan|primordial_titan|slime_king|the_ancient|the_exiled|the_machine|infernal_lord)"),
        "enemy_undead": re.compile(r"^unit_(skeleton|zombie|undead|wraith|lich|necro|ghost|phantasm|spectr|vampire|night_terror|night_specter|night_lament|night_veil|night_blade)"),
        "enemy_beast": re.compile(r"^unit_(wolf|slime|troll|spider|bat|wurm|wyvern|treant|phoenix|pterodactyl|imp|elemental|golem)"),
        "enemy_humanoid": re.compile(r"^unit_(pirate|thief|bandit|brigand|mercenary|insane_|lost_|plague_|shadow_|spire_|shahuri_|nexus_)"),
    }

    rows = []
    with open(csv_path, "r", encoding="utf-8") as f:
        reader = csv.DictReader(f)
        for row in reader:
            if row["Category"] == "uncategorized":
                stem = row["Stem"].lower()
                # Unit classification
                if stem.startswith("unit_"):
                    matched = False
                    for cat, pat in UNIT_PATTERNS.items():
                        if pat.search(stem):
                            row["Category"] = cat
                            matched = True
                            break
                    if not matched:
                        row["Category"] = "enemy_other"
                # Item classification
                elif any(stem.startswith(p) for p in ("sword","shield","armor","helmet","axe","bow","staff","ring","amulet","boots","gloves","dagger","scepter","cape","belt","pendant","hammer","spear","crossbow")):
                    row["Category"] = "item_weapon_armor"
                elif any(stem.endswith(p) for p in ("_sword","_shield","_armor","_helmet","_axe","_bow","_staff","_ring","_amulet","_boots","_gloves","_dagger","_scepter","_cape","_belt","_pendant")):
                    row["Category"] = "item_weapon_armor"
                elif any(stem.startswith(p) for p in ("potion","food_","recipe_","elixir","vial","vinegar","yoghurt","bread","cheese","mushroom","herb","berry","meat","fish")):
                    row["Category"] = "item_consumable"
                elif any(stem.startswith(p) for p in ("sign_","upgrade_","unknown","brass_circle","coin_")):
                    row["Category"] = "ui_element"
                elif any(stem.endswith(p) for p in ("_fang","_scale","_blood","_bone","_hide","_claw","_eye","_feather","_horn","_tail","_tooth","_core","_dust","_essence","_shard","_gem","_stone","_crystal","_ore","_cloth","_leather","_wood","_metal","_iron","_steel","_gold","_silver","_bronze")):
                    row["Category"] = "item_material"
                else:
                    row["Category"] = "item_misc"
            rows.append(row)

    # Write v2
    with open(out_path, "w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=rows[0].keys())
        writer.writeheader()
        writer.writerows(rows)

    # Stats
    cats = {}
    for r in rows:
        cats[r["Category"]] = cats.get(r["Category"], 0) + 1
    print("=== RECLASSIFICATION COMPLETE ===")
    for cat, count in sorted(cats.items(), key=lambda x: -x[1]):
        print(f"  {cat}: {count}")
    return rows

# ============= PART 2: Parse all XML layouts for hierarchy =============
def parse_layout_hierarchy(xml_path):
    """Parse XML layout and return hierarchy tree."""
    try:
        tree = ET.parse(str(xml_path))
    except:
        return None

    def parse_node(elem, depth=0):
        tag = elem.tag.split("}")[-1] if "}" in elem.tag else elem.tag
        # Simplify class names
        tag = tag.replace("androidx.constraintlayout.widget.", "")
        tag = tag.replace("com.google.android.material.", "")
        tag = tag.replace("androidx.viewpager2.widget.", "")
        tag = tag.replace("androidx.drawerlayout.widget.", "")

        node_id = elem.get("{http://schemas.android.com/apk/res/android}id", "")
        node_id = node_id.replace("@+id/", "").replace("@id/", "")

        src = elem.get("{http://schemas.android.com/apk/res-auto}srcCompat", "")
        if not src:
            src = elem.get("{http://schemas.android.com/apk/res/android}src", "")
        src = src.replace("@drawable/", "")

        bg = elem.get("{http://schemas.android.com/apk/res/android}background", "")
        bg = bg.replace("@drawable/", "").replace("@color/", "")

        text = elem.get("{http://schemas.android.com/apk/res/android}text", "")
        text = text.replace("@string/", "")

        include_layout = elem.get("layout", "")
        include_layout = include_layout.replace("@layout/", "")

        visibility = elem.get("{http://schemas.android.com/apk/res/android}visibility", "visible")

        text_size = elem.get("{http://schemas.android.com/apk/res/android}textSize", "")
        text_style = elem.get("{http://schemas.android.com/apk/res/android}textStyle", "")
        text_color = elem.get("{http://schemas.android.com/apk/res/android}textColor", "")
        text_color = text_color.replace("@color/", "")

        result = {
            "depth": depth,
            "tag": tag,
            "id": node_id,
            "src": src,
            "bg": bg,
            "text": text,
            "include": include_layout,
            "visibility": visibility,
            "textSize": text_size,
            "textStyle": text_style,
            "textColor": text_color,
            "children": []
        }

        for child in elem:
            result["children"].append(parse_node(child, depth + 1))

        return result

    return parse_node(tree.getroot())

def flatten_hierarchy(node, rows, screen_name, prefix=""):
    """Flatten hierarchy tree into rows for CSV."""
    indent = "  " * node["depth"]
    rows.append({
        "Screen": screen_name,
        "Depth": node["depth"],
        "Tag": node["tag"],
        "ID": node["id"],
        "Drawable": node["src"],
        "Background": node["bg"],
        "Text": node["text"],
        "Include": node["include"],
        "Visibility": node["visibility"],
        "TextSize": node["textSize"],
        "TextStyle": node["textStyle"],
        "TextColor": node["textColor"],
        "IndentedTag": indent + node["tag"]
    })
    for child in node["children"]:
        flatten_hierarchy(child, rows, screen_name)

# ============= PART 3: Dynamic resource loading audit =============
def audit_dynamic_loading():
    """Find all dynamic resource loading patterns in game code."""
    patterns = {
        "getIdentifier": re.compile(r'getIdentifier\s*\(\s*["\']?(\w+)?'),
        "setImageResource": re.compile(r'setImageResource\s*\(\s*(.+?)\)'),
        "getDrawable": re.compile(r'getDrawable\s*\(\s*(.+?)\)'),
        "string_concat_drawable": re.compile(r'"(unit_|pet_|item_|dungeon_|sign_)"?\s*\+'),
        "R_drawable_variable": re.compile(r'R\.drawable\.(\w+)'),
    }

    results = []
    for java_file in SRC.rglob("*.java"):
        try:
            content = java_file.read_text(encoding="utf-8", errors="ignore")
            lines = content.split("\n")
            for line_num, line in enumerate(lines, 1):
                for pat_name, pat in patterns.items():
                    for match in pat.finditer(line):
                        results.append({
                            "File": java_file.name,
                            "Line": line_num,
                            "Pattern": pat_name,
                            "Match": match.group(0)[:100],
                            "Context": line.strip()[:150]
                        })
        except:
            pass

    return results

# ============= PART 4: Extract game-specific XML drawables =============
def audit_xml_drawables():
    """List all XML drawables and classify them."""
    results = []
    for xml_file in sorted(DRAWABLE_DIR.glob("*.xml")):
        try:
            tree = ET.parse(str(xml_file))
            root = tree.getroot()
            tag = root.tag.split("}")[-1] if "}" in root.tag else root.tag
            results.append({
                "FileName": xml_file.name,
                "RootTag": tag,
                "GameSpecific": not xml_file.name.startswith(("abc_", "avd_", "btn_mtrl", "design_", "ic_mtrl", "material_", "mtrl_", "notification_", "tooltip_")),
            })
        except:
            results.append({
                "FileName": xml_file.name,
                "RootTag": "PARSE_ERROR",
                "GameSpecific": False,
            })
    return results

# ============= PART 5: Extract game-specific colors, dimens =============
def extract_game_colors():
    """Extract all game-specific colors from colors.xml."""
    colors = {}
    xml_path = VALUES_DIR / "colors.xml"
    try:
        tree = ET.parse(str(xml_path))
        for elem in tree.getroot():
            name = elem.get("name", "")
            # Filter out library colors
            if any(name.startswith(p) for p in ("abc_", "design_", "material_", "mtrl_",
                "browser_", "cardview_", "common_google", "call_notification",
                "primary_text_default", "secondary_text_default",
                "bright_foreground", "dim_foreground", "highlighted_text",
                "ripple_material", "switch_thumb", "notification_")):
                continue
            if name.startswith(("accent_material", "background_material",
                "background_floating", "button_material")):
                continue
            colors[name] = elem.text or ""
    except:
        pass
    return colors

def extract_game_dimens():
    """Extract game-specific dimensions."""
    dimens = {}
    xml_path = VALUES_DIR / "dimens.xml"
    try:
        tree = ET.parse(str(xml_path))
        for elem in tree.getroot():
            name = elem.get("name", "")
            if any(name.startswith(p) for p in ("abc_", "design_", "material_", "mtrl_",
                "cardview_", "compat_", "notification_", "tooltip_")):
                continue
            dimens[name] = elem.text or ""
    except:
        pass
    return dimens

# ============= MAIN =============
def main():
    print("=== DEEP AUDIT START ===\n")

    # 1. Reclassify assets
    print("[1/5] Reclassifying assets...")
    reclassify_assets()

    # 2. Parse all game layouts
    print("\n[2/5] Parsing layout hierarchies...")
    GAME_LAYOUT_PREFIXES = ("activity_main", "fragment_", "dialog_", "layout_", "custom_dialog")
    hierarchy_rows = []
    for xml_file in sorted(LAYOUT_DIR.glob("*.xml")):
        if any(xml_file.name.startswith(p) for p in GAME_LAYOUT_PREFIXES):
            tree = parse_layout_hierarchy(xml_file)
            if tree:
                flatten_hierarchy(tree, hierarchy_rows, xml_file.stem)

    hierarchy_csv = OUT / "deep_layout_hierarchy.csv"
    with open(hierarchy_csv, "w", newline="", encoding="utf-8") as f:
        if hierarchy_rows:
            writer = csv.DictWriter(f, fieldnames=hierarchy_rows[0].keys())
            writer.writeheader()
            writer.writerows(hierarchy_rows)
    print(f"  Layouts parsed: {len(set(r['Screen'] for r in hierarchy_rows))}")
    print(f"  Total nodes: {len(hierarchy_rows)}")

    # 3. Dynamic loading audit
    print("\n[3/5] Auditing dynamic resource loading...")
    dynamic = audit_dynamic_loading()
    dynamic_csv = OUT / "deep_dynamic_loading.csv"
    with open(dynamic_csv, "w", newline="", encoding="utf-8") as f:
        if dynamic:
            writer = csv.DictWriter(f, fieldnames=dynamic[0].keys())
            writer.writeheader()
            writer.writerows(dynamic)
    print(f"  Dynamic patterns found: {len(dynamic)}")

    # 4. XML drawable audit
    print("\n[4/5] Auditing XML drawables...")
    xml_drawables = audit_xml_drawables()
    game_drawables = [d for d in xml_drawables if d["GameSpecific"]]
    xml_csv = OUT / "deep_xml_drawables.csv"
    with open(xml_csv, "w", newline="", encoding="utf-8") as f:
        if xml_drawables:
            writer = csv.DictWriter(f, fieldnames=xml_drawables[0].keys())
            writer.writeheader()
            writer.writerows(xml_drawables)
    print(f"  Total XML drawables: {len(xml_drawables)}")
    print(f"  Game-specific: {len(game_drawables)}")

    # 5. Game colors/dimens
    print("\n[5/5] Extracting game colors & dimens...")
    colors = extract_game_colors()
    dimens = extract_game_dimens()

    colors_json = OUT / "deep_game_colors.json"
    with open(colors_json, "w", encoding="utf-8") as f:
        json.dump(colors, f, indent=2)
    print(f"  Game colors: {len(colors)}")

    dimens_json = OUT / "deep_game_dimens.json"
    with open(dimens_json, "w", encoding="utf-8") as f:
        json.dump(dimens, f, indent=2)
    print(f"  Game dimens: {len(dimens)}")

    print("\n=== DEEP AUDIT COMPLETE ===")

if __name__ == "__main__":
    main()
