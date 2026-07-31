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

def get_methods(filepath):
    methods = []
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            for line in f:
                # Naive method detection
                if re.match(r'\s*(public|private|protected|internal).+\(', line) and not "=" in line and not ";" in line:
                    methods.append(line.strip())
    except Exception:
        pass
    return methods

def main():
    print("--- BOOTSTRAP / SERVICE CREATION ---")
    for f in search_files(r'ServiceLocator|Bootstrap|Initialize|Startup'):
        print(f"File: {os.path.relpath(f, SRC_DIR)}")
        
    print("\n--- SAVE / OFFLINE ---")
    for f in search_files(r'SaveManager|Offline|TrueTime|GameSave'):
        print(f"File: {os.path.relpath(f, SRC_DIR)}")
        
    print("\n--- EVENT / TICK ---")
    for f in search_files(r'EventBus|UpdateLoop|Tick|ITickable'):
        print(f"File: {os.path.relpath(f, SRC_DIR)}")
        
    print("\n--- TESTS ---")
    test_files = glob.glob(os.path.join(SRC_DIR, "**", "*Test*.cs"), recursive=True)
    test_count = 0
    for tf in test_files:
        with open(tf, 'r', encoding='utf-8') as f:
            test_count += len(re.findall(r'\[Test\]|\[UnityTest\]', f.read()))
        print(f"Test File: {os.path.relpath(tf, SRC_DIR)}")
    print(f"Total Test Attributes found: {test_count}")
    
    # We also check for 'party persistence' and 'Dungeon restoration'
    print("\n--- PARTY / DUNGEON RESTORATION ---")
    for f in search_files(r'PartyData|PartySave|DungeonSave|Restore'):
        print(f"File: {os.path.relpath(f, SRC_DIR)}")

if __name__ == "__main__":
    main()
