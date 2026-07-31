import os
import json
import re
import datetime

ROOT_DIR = r"D:\Tinh\Rebuild_GuildMaster"
ASSETS_DIR = os.path.join(ROOT_DIR, "Assets")
OUT_DIR = r"D:\Tinh\Rebuild_GuildMaster\Reports\FoundationAudit_A1_20260729"
CMD_LOG_PATH = os.path.join(OUT_DIR, "15_A1_Command_Log.md")

facts = {
    "files_opened": 0,
    "cs_files": [],
    "tests_discovered": 0,
    "editmode_tests": [],
    "playmode_tests": [],
    "save_fields": [],
    "save_methods": [],
    "offline_methods": [],
    "bootstrap_classes": [],
    "service_locators": [],
    "data_files": [],
    "scenes": [],
    "prefabs": []
}

command_log = ["# 15. A1 Command Log\n\n"]

def log_action(action, target, result):
    ts = datetime.datetime.now().isoformat()
    command_log.append(f"**[{ts}]**\n- **Command/Action:** {action}\n- **Target:** {target}\n- **Result:** {result}\n\n")

def read_file(path):
    facts["files_opened"] += 1
    with open(path, 'r', encoding='utf-8', errors='ignore') as f:
        return f.read()

def scan_project():
    for root, dirs, files in os.walk(ASSETS_DIR):
        for f in files:
            full_path = os.path.join(root, f)
            rel_path = os.path.relpath(full_path, ROOT_DIR)
            
            if f.endswith('.cs'):
                facts["cs_files"].append(rel_path)
                content = read_file(full_path)
                
                # Tests
                test_matches = re.findall(r'\[Test\]|\[UnityTest\]', content)
                if test_matches:
                    facts["tests_discovered"] += len(test_matches)
                    if "EditMode" in rel_path:
                        facts["editmode_tests"].append({"file": rel_path, "count": len(test_matches)})
                    elif "PlayMode" in rel_path:
                        facts["playmode_tests"].append({"file": rel_path, "count": len(test_matches)})
                
                # Save schema
                if "SaveData" in f or "SaveManager" in f or "ISave" in f:
                    fields = re.findall(r'public\s+[\w<>]+\s+(\w+)\s*;', content)
                    methods = re.findall(r'(public|private|protected)\s+[\w<>]+\s+(\w+)\s*\(', content)
                    facts["save_fields"].extend([{"file": rel_path, "field": x} for x in fields])
                    facts["save_methods"].extend([{"file": rel_path, "method": x[1]} for x in methods])
                    log_action("Parse C# file for Save schema", rel_path, f"Found {len(fields)} fields, {len(methods)} methods")
                
                # Offline
                if "Offline" in f or "Time" in f:
                    methods = re.findall(r'(public|private|protected)\s+[\w<>]+\s+(\w+)\s*\(', content)
                    facts["offline_methods"].extend([{"file": rel_path, "method": x[1]} for x in methods])
                    log_action("Parse C# file for Offline logic", rel_path, f"Found {len(methods)} methods")
                    
                # Bootstrap / Services
                if "Bootstrap" in f or "Service" in f or "GameLoop" in f:
                    if "ServiceContainer" in content or "ServiceLocator" in content:
                        facts["service_locators"].append(rel_path)
                    facts["bootstrap_classes"].append(rel_path)
                    log_action("Parse C# for Services", rel_path, "Found potential service/bootstrap")
                    
            elif f.endswith('.json') or f.endswith('.csv'):
                facts["data_files"].append(rel_path)
                content = read_file(full_path)
                log_action("Read Data File", rel_path, f"Size: {len(content)} bytes")
                
            elif f.endswith('.unity'):
                facts["scenes"].append(rel_path)
                log_action("Parse Scene YAML", rel_path, "Indexed scene")
                
            elif f.endswith('.prefab'):
                facts["prefabs"].append(rel_path)

def main():
    log_action("Start Scan", "Assets Directory", "Scanning for A1 Foundation")
    scan_project()
    
    with open(os.path.join(OUT_DIR, "evidence", "raw_facts.json"), 'w', encoding='utf-8') as f:
        json.dump(facts, f, indent=2)
        
    with open(CMD_LOG_PATH, 'w', encoding='utf-8') as f:
        f.writelines(command_log)
        
    print(f"Scanned {facts['files_opened']} files. Extracted facts.")

if __name__ == "__main__":
    main()
