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
    print("--- COMBAT ---")
    for f in search_files(r'Combat|Skill|StatusEffect|Damage|Heal'):
        print(f"File: {os.path.relpath(f, SRC_DIR)}")
        
    print("\n--- ADVENTURER / EQUIPMENT ---")
    for f in search_files(r'Adventurer|Equipment|Inventory|Item'):
        print(f"File: {os.path.relpath(f, SRC_DIR)}")
        
    print("\n--- TAVERN / QUARTERS / PARTY ---")
    for f in search_files(r'Tavern|Quarters|Party'):
        print(f"File: {os.path.relpath(f, SRC_DIR)}")
        
    print("\n--- DUNGEON / RAID / LOOT / QUEST ---")
    for f in search_files(r'Dungeon|Raid|Loot|Quest'):
        print(f"File: {os.path.relpath(f, SRC_DIR)}")

if __name__ == "__main__":
    main()
