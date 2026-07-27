from parsers.base_parser import BaseParser
from typing import List, Dict, Any
import re
import traceback
import logging

class RecipeParser(BaseParser):
    def parse_files(self, context, files: List[str]) -> List[Dict[str, Any]]:
        records = []
        
        total_candidates = 0
        confirmed = 0
        partial = 0
        false_positives = 0
        duplicates = 0
        
        seen_variants = {}
        
        for path in files:
            if not path.endswith("Recipes.java"):
                continue
            try:
                with open(path, 'r', encoding='utf-8-sig') as f:
                    content = f.read()
                    
                # Extract the enum body
                enum_match = re.search(r'public\s+enum\s+Recipes\s*\{(.*?)\;', content, re.DOTALL)
                if not enum_match:
                    continue
                    
                enum_body = enum_match.group(1)
                
                # Match enum constants: Name(arg1, arg2)
                matches = re.finditer(r'^\s*([A-Za-z0-9_]+)\((.*?)\)(?:,|\z)', enum_body, re.MULTILINE | re.DOTALL)
                
                for m in matches:
                    total_candidates += 1
                    enum_id = m.group(1)
                    args_str = m.group(2).strip()
                    
                    if enum_id in ["return", "if", "for", "while", "class", "public", "Recipes", "from", "into"]:
                        false_positives += 1
                        continue
                        
                    out_id = enum_id.lower()
                    
                    seen_variants[out_id] = seen_variants.get(out_id, 0) + 1
                    variant = seen_variants[out_id]
                    
                    if variant > 1:
                        duplicates += 1
                        
                    rec_id = f"recipe_{out_id}" if variant == 1 else f"recipe_{out_id}_{variant}"
                    
                    # Parse ingredients
                    ingredients = []
                    ing_matches = re.finditer(r'Item\.getInstance\(\"([A-Za-z0-9_]+)\"\s*(?:,\s*(\d+))?\)', args_str)
                    
                    for ing_m in ing_matches:
                        ing_id = ing_m.group(1).lower()
                        amount = int(ing_m.group(2)) if ing_m.group(2) else 1
                        ingredients.append({"itemId": ing_id, "amount": amount})
                        
                    confirmed += 1
                    rec = {
                        "id": rec_id,
                        "className": "Recipe",
                        "outputItemId": out_id,
                        "ingredients": ingredients,
                        "rawArgs": args_str,
                        "sourcePath": path,
                        "manualRuleRequired": False if len(ingredients) > 0 else True
                    }
                    
                    rec = self._finalize_record(rec)
                    if rec["parseStatus"] == "partial":
                        partial += 1
                        
                    records.append(rec)
                    
            except Exception as e:
                logging.error(f"Recipe parser failed on {path}: {e}")
                
        # Generate audit report
        from src.config_loader import ConfigLoader
        try:
            config = ConfigLoader.load("config/production_profile.json")
            out_root = config.get("reportRoot", "output/production_reports")
        except:
            out_root = "output/production_reports"
            
        import os
        os.makedirs(out_root, exist_ok=True)
        with open(os.path.join(out_root, "recipe_conversion_audit.md"), "w", encoding="utf-8") as f:
            f.write("# Recipe Conversion Audit\n\n")
            f.write(f"- Total Candidates: {total_candidates}\n")
            f.write(f"- Confirmed: {confirmed}\n")
            f.write(f"- Partial: {partial}\n")
            f.write(f"- False Positives Removed: {false_positives}\n")
            f.write(f"- Duplicate Outputs (Variants): {duplicates}\n")
            f.write(f"- Final Exported Count: {len(records)}\n")
            
        return records

    def get_category_name(self) -> str: return "recipes"
