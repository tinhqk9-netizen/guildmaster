import os
import json
import hashlib
from datetime import datetime

staging_dir = os.path.join("D:\Tinh\Rebuild_GuildMaster\Tools\DecodeConverter", "output", "production_staging")
files = [f for f in os.listdir(staging_dir) if f.endswith(".json") and f != "manifest.json" and f != "source_snapshot.json"]

manifest = {
    "schemaVersion": "1.0",
    "definitionsVersion": "1.0",
    "decodeVersion": "unknown",
    "generatedAt": datetime.now().isoformat(),
    "sourceHash": "",
    "converterVersion": "1.0",
    "deterministic": True,
    "files": [],
    "recordCountByCategory": {},
    "partialCountByCategory": {},
    "failedCountByCategory": {},
    "manualRuleCount": 0,
    "assetCount": 0,
    "localizationCount": 0
}

# Source Hash
if os.path.exists(os.path.join(staging_dir, "source_snapshot.json")):
    with open(os.path.join(staging_dir, "source_snapshot.json"), "r", encoding="utf-8") as f:
        ss = json.load(f)
        manifest["sourceHash"] = ss.get("sourceHash", "")
        
# Assets count
if os.path.exists(os.path.join(staging_dir, "assets_manifest.json")):
    with open(os.path.join(staging_dir, "assets_manifest.json"), "r", encoding="utf-8") as f:
        assets = json.load(f)
        manifest["assetCount"] = len(assets)
        
# Localizations count
if os.path.exists(os.path.join(staging_dir, "localization.json")):
    with open(os.path.join(staging_dir, "localization.json"), "r", encoding="utf-8") as f:
        locs = json.load(f)
        manifest["localizationCount"] = len(locs)

# Read reports for manual rule counts
reports_dir = os.path.join("D:\Tinh\Rebuild_GuildMaster\Tools\DecodeConverter", "output", "production_reports")
if os.path.exists(os.path.join(reports_dir, "manual_rules.json")):
    with open(os.path.join(reports_dir, "manual_rules.json"), "r", encoding="utf-8") as f:
        mrules = json.load(f)
        manifest["manualRuleCount"] = len(mrules)

# Hash and counts per file
for file in files:
    cat = file.replace(".json", "")
    if cat in ["assets_manifest", "localization", "source_snapshot"]: continue
    
    with open(os.path.join(staging_dir, file), "r", encoding="utf-8") as f:
        data = json.load(f)
        
    fhash = hashlib.sha256(json.dumps(data, sort_keys=True).encode('utf-8')).hexdigest()
    
    records = data.get("data", [])
    rcount = len(records)
    partial_count = sum(1 for r in records if r.get("parseStatus") == "partial")
    failed_count = sum(1 for r in records if r.get("parseStatus") == "failed")
    
    manifest["recordCountByCategory"][cat] = rcount
    manifest["partialCountByCategory"][cat] = partial_count
    manifest["failedCountByCategory"][cat] = failed_count
    
    manifest["files"].append({
        "filename": file,
        "category": cat,
        "recordCount": rcount,
        "hash": fhash,
        "dependencies": [],
        "loadOrder": 1
    })
    
with open(os.path.join(staging_dir, "manifest.json"), "w", encoding="utf-8") as f:
    json.dump(manifest, f, indent=4)
