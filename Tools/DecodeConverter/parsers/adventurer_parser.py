from parsers.base_parser import BaseParser
from typing import List, Dict, Any

class AdventurerParser(BaseParser):
    def parse_files(self, context, files: List[str]) -> List[Dict[str, Any]]:
        records = []
        for path in files:
            parsed = self.parse_file(path)
            if not parsed.class_name: continue
            
            rec_id = self.normalizer.normalize(parsed.class_name)
            fields = {**parsed.fields}
            for a in parsed.assignments: fields[a.field] = a.value
            
            rec = self._finalize_record({
                "id": rec_id,
                "className": parsed.class_name,
                "parentClass": parsed.parent_class,
                "nameKey": fields.get("name"),
                "baseStats": context.semantic_tagger.tag_fields({k:v for k,v in fields.items() if "base" in k.lower() or "stat" in k.lower()}),
                "sourcePath": path
            }, parsed)
            records.append(rec)
        return records
        
    def get_category_name(self) -> str: return "adventurers"


