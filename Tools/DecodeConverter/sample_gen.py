import os
import json
import random

staging_dir = os.path.join("D:\Tinh\Rebuild_GuildMaster\Tools\DecodeConverter", "output", "production_staging")

samples_req = {
    "items.json": 10,
    "adventurers.json": 5,
    "enemies.json": 5,
    "quests.json": 3,
    "recipes.json": 3,
    "dungeons.json": 1,
    "raids.json": 1,
    "pets.json": 2,
    "skills.json": 5,
    "status_effects.json": 5
}

random.seed(42) # Deterministic

with open(os.path.join(staging_dir, "sample_manual_review.md"), "w", encoding="utf-8") as out:
    out.write("# Sample Manual Review\n\n")
    for fname, count in samples_req.items():
        out.write(f"## {fname}\n")
        path = os.path.join(staging_dir, fname)
        if not os.path.exists(path):
            out.write("Not found.\n")
            continue
            
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f).get("data", [])
            
        if not data:
            out.write("Empty.\n")
            continue
            
        k = min(count, len(data))
        samples = random.sample(data, k)
        
        for s in samples:
            out.write(f"### {s.get('id')}\n")
            out.write(f"- Source Path: {s.get('sourcePath')}\n")
            out.write(f"- Parse Status: {s.get('parseStatus')}\n")
            out.write(f"- Record Hash: {s.get('recordHash')}\n")
            out.write("`json\n")
            out.write(json.dumps(s, indent=2, ensure_ascii=False))
            out.write("\n`\n\n")
