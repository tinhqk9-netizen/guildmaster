"""Generate screen-to-asset mapping CSV from layout XMLs and Java source."""
import os, csv, re
from pathlib import Path

LAYOUT_DIR = Path(r"D:\Tinh\Guild Master - Idle Dungeons\resources\res\layout")
SOURCES_DIR = Path(r"D:\Tinh\Guild Master - Idle Dungeons\sources\it\paranoidsquirrels\idleguildmaster")
OUTPUT_CSV = Path(r"D:\Tinh\Rebuild_GuildMaster\Docs\Legacy_Audit\legacy_screen_asset_map.csv")

# Game-specific layout files (not library)
GAME_LAYOUTS = [f for f in LAYOUT_DIR.glob("*.xml")
                if not f.name.startswith(("abc_", "design_", "browser_", "admob_",
                    "notification_", "mtrl_", "support_", "custom_tab_",
                    "test_", "select_dialog", "m3_", "ime_", "material_",
                    "text_view_", "offline_ads"))]

DRAWABLE_RE = re.compile(r'@drawable/(\w+)')
JAVA_DRAWABLE_RE = re.compile(r'R\.drawable\.(\w+)')
JAVA_GETIDENT_RE = re.compile(r'getIdentifier\(\s*"(\w+)"')

def extract_drawables_from_xml(xml_path):
    """Extract all @drawable/ references from an XML file."""
    drawables = set()
    try:
        content = xml_path.read_text(encoding="utf-8", errors="ignore")
        drawables.update(DRAWABLE_RE.findall(content))
    except:
        pass
    return drawables

def extract_drawables_from_java(java_path):
    """Extract R.drawable.X and getIdentifier references."""
    drawables = set()
    try:
        content = java_path.read_text(encoding="utf-8", errors="ignore")
        drawables.update(JAVA_DRAWABLE_RE.findall(content))
        drawables.update(JAVA_GETIDENT_RE.findall(content))
    except:
        pass
    return drawables

# Map layout XML name → Java class (known mappings)
LAYOUT_TO_CLASS = {
    "activity_main.xml": "MainActivity.java",
    "fragment_headquarters.xml": "HeadquartersFragment.java",
    "fragment_adventurers.xml": "AdventurersFragment.java",
    "fragment_dungeons.xml": "DungeonsFragment.java",
    "fragment_raids.xml": "RaidsFragment.java",
}
# Auto-map dialog_*.xml → Dialog*.java
for f in GAME_LAYOUTS:
    if f.name.startswith("dialog_"):
        # dialog_entity_detail.xml → DialogEntityDetail.java
        parts = f.stem.replace("dialog_", "").split("_")
        class_name = "Dialog" + "".join(p.capitalize() for p in parts) + ".java"
        LAYOUT_TO_CLASS[f.name] = class_name

# Auto-map layout_*.xml → search in all Java for usage
# (these are list item layouts, included by adapters)

def main():
    rows = []

    for layout_file in sorted(GAME_LAYOUTS):
        xml_drawables = extract_drawables_from_xml(layout_file)

        # Find associated Java class
        java_class = LAYOUT_TO_CLASS.get(layout_file.name, "")
        java_drawables = set()

        if java_class:
            # Search for Java file
            for java_file in SOURCES_DIR.rglob(java_class):
                java_drawables.update(extract_drawables_from_java(java_file))

        all_drawables = xml_drawables | java_drawables

        for drawable in sorted(all_drawables):
            source = []
            if drawable in xml_drawables:
                source.append("XML")
            if drawable in java_drawables:
                source.append("Java")

            rows.append({
                "Screen": layout_file.stem,
                "JavaClass": java_class,
                "DrawableName": drawable,
                "Source": "+".join(source),
            })

    # Write CSV
    fieldnames = ["Screen", "JavaClass", "DrawableName", "Source"]
    with open(OUTPUT_CSV, "w", newline="", encoding="utf-8") as csvfile:
        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)

    # Stats
    screens = set(r["Screen"] for r in rows)
    assets = set(r["DrawableName"] for r in rows)
    print(f"=== SCREEN-ASSET MAP COMPLETE ===")
    print(f"Screens mapped: {len(screens)}")
    print(f"Unique assets referenced: {len(assets)}")
    print(f"Total mappings: {len(rows)}")
    print(f"Output: {OUTPUT_CSV}")

if __name__ == "__main__":
    main()
