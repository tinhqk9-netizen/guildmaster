from parsers.base_parser import BaseParser
from typing import List, Dict, Any

class EnemyParser(BaseParser):
    def parse_files(self, context, files: List[str]) -> List[Dict[str, Any]]:
        records = []
        for path in files:
            parsed = self.parse_file(path)
            if not parsed.class_name: continue
            
            rec_id = self.normalizer.normalize(parsed.class_name)
            fields = {**parsed.fields}
            for a in parsed.assignments: fields[a.field] = a.value
            
            if parsed.calls:
                parsed.warnings.append("manualRuleRequired=true due to method calls in enemy")
                
            rec = self._finalize_record({
                "id": rec_id,
                "className": parsed.class_name,
                "parentClass": parsed.parent_class,
                "nameKey": fields.get("name"),
                "stats": context.semantic_tagger.tag_fields({k:v for k,v in fields.items() if "health" in k.lower() or "damage" in k.lower()}),
                "manualRuleRequired": len(parsed.calls) > 0,
                "sourcePath": path
            }, parsed)
            records.append(rec)
        return records
        
    def get_category_name(self) -> str: return "enemies"


