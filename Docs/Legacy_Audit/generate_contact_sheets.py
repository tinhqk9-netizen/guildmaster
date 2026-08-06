"""Generate contact sheets from legacy_asset_inventory.csv grouped by category."""
import csv, os, math, sys
from pathlib import Path

try:
    from PIL import Image, ImageDraw, ImageFont
except ImportError:
    print("ERROR: Pillow not installed")
    sys.exit(1)

CSV_PATH = Path(r"D:\Tinh\Rebuild_GuildMaster\Docs\Legacy_Audit\legacy_asset_inventory.csv")
OUTPUT_DIR = Path(r"D:\Tinh\Rebuild_GuildMaster\Docs\Legacy_Audit\Asset_Gallery")

THUMB_SIZE = 96
PADDING = 8
COLS = 10
LABEL_HEIGHT = 14
CELL_W = THUMB_SIZE + PADDING * 2
CELL_H = THUMB_SIZE + LABEL_HEIGHT + PADDING * 2
BG_COLOR = (30, 30, 30)
LABEL_COLOR = (200, 200, 200)


def make_contact_sheet(category, items, output_dir):
    n = len(items)
    if n == 0:
        return
    rows = math.ceil(n / COLS)
    sheet_w = COLS * CELL_W + PADDING
    sheet_h = rows * CELL_H + PADDING + 30  # extra for title

    sheet = Image.new("RGB", (sheet_w, sheet_h), BG_COLOR)
    draw = ImageDraw.Draw(sheet)

    # Title
    draw.text((PADDING, PADDING), f"{category} ({n} assets)", fill=(255, 255, 100))

    for idx, item in enumerate(items):
        col = idx % COLS
        row = idx // COLS
        x = col * CELL_W + PADDING
        y = row * CELL_H + PADDING + 24

        filepath = item["FullPath"]
        try:
            with Image.open(filepath) as img:
                img.thumbnail((THUMB_SIZE, THUMB_SIZE), Image.Resampling.LANCZOS)
                # Center in cell
                ox = x + (THUMB_SIZE - img.width) // 2
                oy = y + (THUMB_SIZE - img.height) // 2
                if img.mode == "RGBA":
                    # Create checkerboard bg for alpha
                    bg = Image.new("RGBA", img.size, (50, 50, 50, 255))
                    bg.paste(img, mask=img.split()[3])
                    sheet.paste(bg.convert("RGB"), (ox, oy))
                else:
                    sheet.paste(img.convert("RGB"), (ox, oy))
        except Exception as e:
            draw.rectangle([x, y, x + THUMB_SIZE, y + THUMB_SIZE], fill=(80, 0, 0))
            draw.text((x + 2, y + 2), "ERR", fill=(255, 0, 0))

        # Label (truncated filename)
        label = item["Stem"][:15]
        dims = f"{item['Width']}x{item['Height']}" if int(item.get("Width", 0)) else ""
        draw.text((x, y + THUMB_SIZE + 2), label, fill=LABEL_COLOR)

    out_path = output_dir / f"contact_{category}.png"
    sheet.save(str(out_path), "PNG")
    print(f"  Saved: {out_path.name} ({n} assets, {sheet_w}x{sheet_h})")


def main():
    if not CSV_PATH.exists():
        print(f"ERROR: {CSV_PATH} not found. Run generate_asset_inventory.py first.")
        sys.exit(1)

    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    # Load CSV
    by_category = {}
    with open(CSV_PATH, "r", encoding="utf-8") as f:
        reader = csv.DictReader(f)
        for row in reader:
            cat = row.get("Category", "uncategorized")
            by_category.setdefault(cat, []).append(row)

    print(f"Loaded {sum(len(v) for v in by_category.values())} assets in {len(by_category)} categories")

    # Also generate an "all_referenced" sheet
    referenced = []
    unreferenced = []

    for cat in sorted(by_category.keys()):
        items = by_category[cat]
        make_contact_sheet(cat, items, OUTPUT_DIR)
        for item in items:
            if item.get("IsReferenced", "").lower() == "true":
                referenced.append(item)
            else:
                unreferenced.append(item)

    # Referenced vs unreferenced sheets
    if referenced:
        make_contact_sheet("ALL_REFERENCED", referenced, OUTPUT_DIR)
    if unreferenced:
        make_contact_sheet("ALL_UNREFERENCED", unreferenced, OUTPUT_DIR)

    print(f"\nDone. {len(referenced)} referenced, {len(unreferenced)} unreferenced.")


if __name__ == "__main__":
    main()
