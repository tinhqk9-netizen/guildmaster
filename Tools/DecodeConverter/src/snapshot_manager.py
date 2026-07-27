import os
import hashlib
from datetime import datetime
import json

class SnapshotManager:
    @staticmethod
    def create_snapshot(decode_root: str, game_src_root: str, res_root: str, out_path: str) -> dict:
        source_count = 0
        res_count = 0
        cat_counts = {}
        
        hasher = hashlib.sha256()
        file_hashes = []
        
        # Scan game sources
        if os.path.exists(game_src_root):
            for root_dir, dirs, files in os.walk(game_src_root):
                for f in files:
                    if f.endswith('.java'):
                        source_count += 1
                        full_path = os.path.join(root_dir, f)
                        rel_path = os.path.relpath(full_path, decode_root).replace('\\', '/')
                        
                        cat = "other"
                        if "items" in rel_path: cat = "items"
                        elif "adventurers" in rel_path: cat = "adventurers"
                        elif "enemies" in rel_path: cat = "enemies"
                        elif "areas" in rel_path: cat = "areas"
                        elif "quests" in rel_path: cat = "quests"
                        
                        cat_counts[cat] = cat_counts.get(cat, 0) + 1
                        
                        with open(full_path, 'rb') as fd:
                            fhash = hashlib.sha256(fd.read()).hexdigest()
                        file_hashes.append(f"{rel_path}:{fhash}")
                        
        # Scan resources
        if os.path.exists(res_root):
            for root_dir, dirs, files in os.walk(res_root):
                for f in files:
                    res_count += 1
                    full_path = os.path.join(root_dir, f)
                    rel_path = os.path.relpath(full_path, decode_root).replace('\\', '/')
                    with open(full_path, 'rb') as fd:
                        fhash = hashlib.sha256(fd.read()).hexdigest()
                    file_hashes.append(f"{rel_path}:{fhash}")
                    
        # Deterministic sort
        file_hashes.sort()
        for fh in file_hashes:
            hasher.update(fh.encode('utf-8'))
            
        final_hash = hasher.hexdigest()
        
        snapshot = {
            "decodeRoot": decode_root,
            "packageName": "it.paranoidsquirrels.idleguildmaster",
            "appVersionName": "Unknown",
            "appVersionCode": "Unknown",
            "sourceFileCount": source_count,
            "resourceFileCount": res_count,
            "contentFileCountByCategory": cat_counts,
            "sourceHash": final_hash,
            "generatedAt": datetime.now().isoformat()
        }
        
        os.makedirs(os.path.dirname(out_path), exist_ok=True)
        with open(out_path, 'w', encoding='utf-8') as f:
            json.dump(snapshot, f, indent=4)
            
        return snapshot
