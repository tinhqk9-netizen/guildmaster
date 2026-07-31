import os
import json
import re

ROOT_DIR = r"D:\Tinh\Rebuild_GuildMaster\Assets"
OUT_FILE = r"D:\Tinh\Rebuild_GuildMaster\Reports\UI_Audit_Batch1_Foundation_20260730\scripts\extracted_batch1.json"

def scan_files(extension):
    matched = []
    for root, dirs, files in os.walk(ROOT_DIR):
        for f in files:
            if f.endswith(extension):
                matched.append(os.path.join(root, f))
    return matched

def extract_class_info(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Simple extraction of methods
    methods = re.findall(r'(?:public|private|protected|internal)(?:\s+(?:static|virtual|override|async))?\s+([\w\<\>\[\]]+)\s+(\w+)\s*\(([^)]*)\)', content)
    fields = re.findall(r'(?:public|private|protected|internal)(?:\s+(?:readonly|const|static))?\s+([\w\<\>\[\]]+)\s+(\w+)\s*(?:=|;)', content)
    
    lines = content.splitlines()
    
    return {
        "methods": [{"return_type": m[0], "name": m[1], "args": m[2]} for m in methods],
        "fields": [{"type": f[0], "name": f[1]} for f in fields],
        "content_lines": len(lines),
        "raw_content": content
    }

def main():
    cs_files = scan_files('.cs')
    unity_files = scan_files('.unity')
    prefab_files = scan_files('.prefab')
    
    target_classes = [
        "BootSceneLoader", "UIRuntimeBootstrap", "Bootstrapper", "GameStartup",
        "ServiceContainer", "GameDatabase", "SaveService", "SaveData", "SaveMetadata",
        "NormalizeAfterLoad", "OfflineProgressService", "UIService"
    ]
    
    data = {
        "classes": {},
        "scenes": {},
        "prefabs": {}
    }
    
    for fp in cs_files:
        basename = os.path.basename(fp).replace(".cs", "")
        # We also want to capture anything that looks related to HUD, Headquarters, Main, save, boot
        if basename in target_classes or any(x in basename.lower() for x in ['hud', 'headquarter', 'main', 'boot', 'save', 'offline']):
            data["classes"][basename] = {
                "path": fp.replace(r"D:\Tinh\Rebuild_GuildMaster\\", ""),
                "info": extract_class_info(fp)
            }

    for fp in unity_files:
        basename = os.path.basename(fp).replace(".unity", "")
        if basename in ['Boot', 'Main'] or 'boot' in basename.lower() or 'main' in basename.lower():
            with open(fp, 'r', encoding='utf-8') as f:
                data["scenes"][basename] = {
                    "path": fp.replace(r"D:\Tinh\Rebuild_GuildMaster\\", ""),
                    "contains_boot_loader": "BootSceneLoader" in f.read()
                }

    for fp in prefab_files:
        basename = os.path.basename(fp).replace(".prefab", "")
        if any(x in basename.lower() for x in ['boot', 'main', 'hud', 'popup']):
            with open(fp, 'r', encoding='utf-8') as f:
                data["prefabs"][basename] = {
                    "path": fp.replace(r"D:\Tinh\Rebuild_GuildMaster\\", ""),
                    "size": os.path.getsize(fp)
                }

    with open(OUT_FILE, 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2)

    print(f"Extracted data saved to {OUT_FILE}")

if __name__ == "__main__":
    main()
