"""Generate legacy_asset_inventory.csv from decoded game resources."""
import os, csv, re, sys
from pathlib import Path

try:
    from PIL import Image
except ImportError:
    print("ERROR: Pillow not installed. Run: pip install Pillow")
    sys.exit(1)

DRAWABLE_DIR = Path(r"D:\Tinh\Guild Master - Idle Dungeons\resources\res\drawable")
DRAWABLE_HDPI = Path(r"D:\Tinh\Guild Master - Idle Dungeons\resources\res\drawable-hdpi")
SOURCES_DIR = Path(r"D:\Tinh\Guild Master - Idle Dungeons\sources\it\paranoidsquirrels\idleguildmaster")
LAYOUT_DIR = Path(r"D:\Tinh\Guild Master - Idle Dungeons\resources\res\layout")
OUTPUT_CSV = Path(r"D:\Tinh\Rebuild_GuildMaster\Docs\Legacy_Audit\legacy_asset_inventory.csv")

CATEGORY_PATTERNS = {
    "icon_nav": re.compile(r"(bottom_nav|drawer_icon|vector_menu|ic_)"),
    "icon_ui": re.compile(r"(icon_|btn_|arrow_|close_|back_|check_|star_|lock_)"),
    "character": re.compile(r"(adventurer|enemy|boss|hero|knight|mage|ranger|cleric|warrior|rogue|paladin|monk|barbarian|bard|druid)"),
    "item_equipment": re.compile(r"(sword|shield|armor|helmet|axe|bow|staff|ring|amulet|boots|gloves|item_|weapon_|equip_)"),
    "pet": re.compile(r"(pet_|familiar_)"),
    "dungeon_place": re.compile(r"(dungeon_|cave_|forest_|castle_|tower_|ruins_|raid_|place_)"),
    "resource_currency": re.compile(r"(gold|gem|coin|diamond|crystal|soul_|material_|currency_)"),
    "ui_frame": re.compile(r"(border|frame|panel|bg_|background|card_|container_|object_border)"),
    "fx_particle": re.compile(r"(particle_|effect_|glow_|spark_|aura_)"),
    "misc_system": re.compile(r"(shop|merchant|quest|tutorial|achievement|setting|cloud|reddit|cafe_naver|faq|king_message|advertisement|shelter|tavern|workshop|market|craft|bestiary|report|storage|quarters|promotion|doctrine|potion|food|intercession)"),
}

def classify(name):
    name_lower = name.lower()
    for cat, pat in CATEGORY_PATTERNS.items():
        if pat.search(name_lower):
            return cat
    return "uncategorized"

def find_references(drawable_name):
    """Search Java and XML for references to this drawable (without extension)."""
    refs = {"java": [], "xml": []}
    stem = Path(drawable_name).stem

    # Search Java for R.drawable.<stem> or getIdentifier("<stem>"
    java_patterns = [f"R.drawable.{stem}", f'"{stem}"']
    for java_file in SOURCES_DIR.rglob("*.java"):
        try:
            content = java_file.read_text(encoding="utf-8", errors="ignore")
            for pat in java_patterns:
                if pat in content:
                    refs["java"].append(java_file.name)
                    break
        except:
            pass

    # Search XML for @drawable/<stem>
    xml_pattern = f"@drawable/{stem}"
    for xml_file in LAYOUT_DIR.glob("*.xml"):
        try:
            content = xml_file.read_text(encoding="utf-8", errors="ignore")
            if xml_pattern in content:
                refs["xml"].append(xml_file.name)
        except:
            pass

    return refs

def process_file(filepath):
    name = filepath.name
    ext = filepath.suffix.lower()
    size = filepath.stat().st_size
    width, height, has_alpha = 0, 0, False

    if ext in (".png", ".webp", ".jpg", ".jpeg"):
        try:
            with Image.open(filepath) as img:
                width, height = img.size
                has_alpha = img.mode in ("RGBA", "LA", "PA")
        except:
            pass

    category = classify(name)
    refs = find_references(name)
    java_refs = "; ".join(sorted(set(refs["java"])))
    xml_refs = "; ".join(sorted(set(refs["xml"])))
    is_referenced = bool(refs["java"] or refs["xml"])

    return {
        "FileName": name,
        "Stem": Path(name).stem,
        "Extension": ext,
        "Width": width,
        "Height": height,
        "FileSize": size,
        "HasAlpha": has_alpha,
        "Category": category,
        "IsReferenced": is_referenced,
        "JavaRefs": java_refs,
        "XMLRefs": xml_refs,
        "FullPath": str(filepath),
    }

def main():
    rows = []

    # Process drawable folder
    if DRAWABLE_DIR.exists():
        for f in sorted(DRAWABLE_DIR.iterdir()):
            if f.is_file() and f.suffix.lower() in (".png", ".webp", ".jpg", ".jpeg"):
                print(f"  Processing: {f.name}")
                rows.append(process_file(f))

    # Process drawable-hdpi (may have additional assets)
    if DRAWABLE_HDPI.exists():
        existing_names = {r["FileName"] for r in rows}
        for f in sorted(DRAWABLE_HDPI.iterdir()):
            if f.is_file() and f.suffix.lower() in (".png", ".webp", ".jpg", ".jpeg"):
                if f.name not in existing_names:
                    print(f"  Processing (hdpi): {f.name}")
                    rows.append(process_file(f))

    # Write CSV
    fieldnames = ["FileName", "Stem", "Extension", "Width", "Height", "FileSize",
                   "HasAlpha", "Category", "IsReferenced", "JavaRefs", "XMLRefs", "FullPath"]
    with open(OUTPUT_CSV, "w", newline="", encoding="utf-8") as csvfile:
        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)

    # Stats
    total = len(rows)
    referenced = sum(1 for r in rows if r["IsReferenced"])
    unreferenced = total - referenced
    cats = {}
    for r in rows:
        cats[r["Category"]] = cats.get(r["Category"], 0) + 1

    print(f"\n=== INVENTORY COMPLETE ===")
    print(f"Total image assets: {total}")
    print(f"Referenced (Java/XML): {referenced}")
    print(f"Unreferenced: {unreferenced}")
    print(f"\nBy category:")
    for cat, count in sorted(cats.items(), key=lambda x: -x[1]):
        print(f"  {cat}: {count}")
    print(f"\nOutput: {OUTPUT_CSV}")

if __name__ == "__main__":
    main()
