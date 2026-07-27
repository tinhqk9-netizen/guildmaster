from parsers.base_parser import BaseParser
from typing import List, Dict, Any
import re

class EnumFileParser(BaseParser):
    def parse_files(self, context, files: List[str]) -> List[Dict[str, Any]]:
        records = []
        for path in files:
            with open(path, 'r', encoding='utf-8-sig') as f:
                content = f.read()
            matches = re.finditer(r'([A-Z0-9_a-z]+)\((.*?)\)(?:,|\;)', content, re.DOTALL)
            for m in matches:
                enum_id = m.group(1).lower()
                args = m.group(2)
                if enum_id in ["return", "if", "for", "while", "class", "public"]: continue
                rec = {
                    "id": enum_id,
                    "className": m.group(1),
                    "rawArgs": args.strip(),
                    "sourcePath": path
                }
                records.append(self._finalize_record(rec))
        return records

class SkillParser(EnumFileParser):
    def get_category_name(self) -> str: return "skills"

class StatusEffectParser(EnumFileParser):
    def get_category_name(self) -> str: return "status_effects"

class RecipeParser(EnumFileParser):
    def get_category_name(self) -> str: return "recipes"

