import os
import json
import hashlib
import shutil
import tempfile
import logging

class DeterminismChecker:
    def check(self, cli_instance, args):
        logging.info("Running Determinism Verification...")
        
        dir1 = tempfile.mkdtemp(prefix="guildmaster_stage1_")
        dir2 = tempfile.mkdtemp(prefix="guildmaster_stage2_")
        
        args_list1 = ["convert-all-production", "--config", "config/production_profile.json", "--output", dir1]
        cli_instance.run(args_list1)
        
        args_list2 = ["convert-all-production", "--config", "config/production_profile.json", "--output", dir2]
        cli_instance.run(args_list2)
        
        report = self.compare_dirs(dir1, dir2)
        
        # Write report
        out_root = args.output if hasattr(args, 'output') and getattr(args, 'output') and getattr(args, 'output') != "output/debug/integration" else "output/production_reports"
        os.makedirs(out_root, exist_ok=True)
        with open(os.path.join(out_root, "determinism_report.md"), "w", encoding="utf-8") as f:
            f.write("# Determinism Evidence Report\n\n")
            f.write(f"**Final Status**: {report['status']}\n\n")
            f.write(f"- Files compared: {report['totalFiles']}\n")
            f.write(f"- Identical files: {len(report['identical'])}\n")
            f.write(f"- Differing files: {len(report['differing'])}\n")
            f.write(f"- Ignored fields: {', '.join(report['allowed_differences'])}\n\n")
            
            f.write("## Normalized Hashes\n")
            f.write("| File | Hash |\n|---|---|\n")
            for file, hash_val in report['hashes'].items():
                f.write(f"| {file} | {hash_val} |\n")
                
            if report['differing']:
                f.write("\n## Differences\n")
                for d in report['differing']:
                    f.write(f"- **{d}**\n")
                    
        shutil.rmtree(dir1)
        shutil.rmtree(dir2)
        
        return report
        
    def compare_dirs(self, dir1, dir2):
        files1 = sorted([f for f in os.listdir(dir1) if f.endswith('.json')])
        files2 = sorted([f for f in os.listdir(dir2) if f.endswith('.json')])
        
        if files1 != files2:
            return {"status": "FAIL", "reason": "Different file lists", "files": [files1, files2], "totalFiles": 0, "identical": [], "differing": [], "allowed_differences": [], "hashes": {}}
            
        differing = []
        identical = []
        hashes = {}
        
        for f in files1:
            with open(os.path.join(dir1, f), 'r', encoding='utf-8') as fd:
                data1 = json.load(fd)
            with open(os.path.join(dir2, f), 'r', encoding='utf-8') as fd:
                data2 = json.load(fd)
                
            h1 = self._hash_data(data1)
            h2 = self._hash_data(data2)
            
            hashes[f] = h1
            
            if h1 == h2:
                identical.append(f)
            else:
                differing.append(f)
                
        return {
            "status": "PASS" if not differing else "FAIL",
            "totalFiles": len(files1),
            "identical": identical,
            "differing": differing,
            "allowed_differences": ["generatedAt", "runId"],
            "hashes": hashes
        }
        
    def _hash_data(self, data):
        if isinstance(data, dict):
            return hashlib.sha256(json.dumps({k: self._hash_data(v) for k, v in data.items() if k not in ["generatedAt", "runId"]}, sort_keys=True).encode('utf-8')).hexdigest()
        elif isinstance(data, list):
            return hashlib.sha256(json.dumps([self._hash_data(i) for i in data], sort_keys=True).encode('utf-8')).hexdigest()
        return data
