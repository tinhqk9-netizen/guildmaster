import os
import glob
import re

SRC_DIR = r"D:\Tinh\Rebuild_GuildMaster\Assets\_Game"

def search_files(pattern):
    results = []
    for root, _, files in os.walk(SRC_DIR):
        for f in files:
            if f.endswith(".cs"):
                path = os.path.join(root, f)
                with open(path, 'r', encoding='utf-8') as file:
                    content = file.read()
                    if re.search(pattern, content, re.IGNORECASE):
                        results.append(path)
    return results

def main():
    print("--- A3: INVENTORY / STORAGE / WORKSHOP / RECIPES ---")
    for f in search_files(r'Inventory|Storage|Workshop|Recipe'):
        print(f"File: {os.path.relpath(f, SRC_DIR)}")
        
    print("\n--- A3: MERCHANT / MARKET / SHOP ---")
    for f in search_files(r'Merchant|Market|Shop'):
        print(f"File: {os.path.relpath(f, SRC_DIR)}")
        
    print("\n--- A3: PETS / SHELTER / PROMOTION / ASCENSION / DOCTRINE ---")
    for f in search_files(r'Pet|Shelter|Promotion|Ascension|Doctrine'):
        print(f"File: {os.path.relpath(f, SRC_DIR)}")

    print("\n--- A4: UI AND FLOW ---")
    for f in search_files(r'Screen|UI|Button|Listener'):
        print(f"File: {os.path.relpath(f, SRC_DIR)}")

if __name__ == "__main__":
    main()
