import json
import hashlib
import traceback
import logging

class BaseParser:
    def __init__(self):
        from src.java_parser import JavaParser
        from src.id_normalizer import IDNormalizer
        self.java_parser = JavaParser()
        self.normalizer = IDNormalizer()

    def parse_file(self, path: str):
        with open(path, 'r', encoding='utf-8-sig') as f:
            content = f.read()
        return self.java_parser.parse(path, content)

    def parse_files(self, context, files: list) -> list:
        raise NotImplementedError()
        
    def _finalize_record(self, rec: dict, parsed_class=None):
        status = "full"
        reasons = []
        
        # Base validation
        if not rec.get("id"):
            status = "failed"
            reasons.append("MISSING_ID")
            
        if parsed_class and len(parsed_class.unsupported) > 0:
            status = "partial" if status != "failed" else "failed"
            reasons.append("UNSUPPORTED_CONSTRUCTS")
            
        if rec.get("manualRuleRequired", False):
            status = "partial" if status != "failed" else "failed"
            reasons.append("MANUAL_RULE_REQUIRED")
            
        # Category specific validation
        cat = self.get_category_name()
        if cat == "items":
            if not rec.get("className"):
                status = "partial"
                reasons.append("MISSING_CLASSNAME")
                
        elif cat == "adventurers":
            if not rec.get("className"):
                status = "partial"
                reasons.append("MISSING_CLASSNAME")
            if not rec.get("baseStats") and "parentClass" not in rec:
                status = "partial"
                reasons.append("MISSING_STATS_AND_PARENT")
                
        elif cat == "enemies":
            if not rec.get("className"):
                status = "partial"
                reasons.append("MISSING_CLASSNAME")
            if not rec.get("stats"):
                status = "partial"
                reasons.append("MISSING_STATS")
                
        elif cat == "dungeons" or cat == "raids":
            pass # Just ID is enough for full for now if no calls
            
        elif cat == "quests":
            pass
            
        elif cat == "pets":
            pass
            
        elif cat == "recipes":
            if not rec.get("outputItemId"):
                status = "partial"
                reasons.append("MISSING_OUTPUT_ITEM")
            if not rec.get("ingredients"):
                status = "partial"
                reasons.append("MISSING_INGREDIENTS")
                
        elif cat == "skills" or cat == "status_effects":
            if "rawArgs" in rec and len(rec.keys()) <= 5: # Only basic fields
                status = "partial"
                reasons.append("UNPARSED_ARGS")
                
        rec["parseStatus"] = status
        rec["parseReasons"] = reasons
        
        # Calculate Hash
        hash_dict = {k: v for k, v in rec.items() if k not in ["generatedAt", "runId", "sourcePath"]}
        if "sourcePath" in rec:
            rel_path = rec["sourcePath"].replace('\\', '/').split('/sources/')[-1]
            hash_dict["relativeSourcePath"] = rel_path
            
        try:
            import json, hashlib
            json_str = json.dumps(hash_dict, sort_keys=True)
            rec["recordHash"] = hashlib.sha256(json_str.encode('utf-8')).hexdigest()
        except Exception as e:
            import logging
            logging.error(f"Failed to hash record {rec.get('id')}: {e}")
            rec["recordHash"] = "ERROR"
            rec["parseStatus"] = "failed"
            rec["parseReasons"].append("HASH_ERROR")
            
        return rec
        
    def validate(self, records: list, context):
        for rec in records:
            if rec.get("id"):
                context.validator.validate_id(rec['id'], rec.get('sourcePath', ''))
            
    def export(self, records: list, context, manifest, out_dir: str):
        from src.exporter import Exporter
        import os
        
        # Filter out completely failed records without an ID, but keep tracking them if possible
        valid_records = [r for r in records if r.get("id")]
        
        # Deterministic Sort before export
        valid_records.sort(key=lambda x: x["id"])
        
        Exporter.export(valid_records, os.path.join(out_dir, f"{self.get_category_name()}.json"), manifest)
        
    def get_category_name(self) -> str:
        raise NotImplementedError()

