from parsers.base_parser import BaseParser
from typing import List, Dict, Any

class DungeonParser(BaseParser):
    def parse_files(self, context, files: List[str]) -> List[Dict[str, Any]]:
        records = []
        for path in files:
            parsed = self.parse_file(path)
            if not parsed.class_name: continue
            
            rec_id = self.normalizer.normalize(parsed.class_name)
            fields = {**parsed.fields}
            for a in parsed.assignments: fields[a.field] = a.value
            
            rec = {
                "id": rec_id,
                "className": parsed.class_name,
                "nameKey": fields.get("name"),
                "manualRuleRequired": True if parsed.calls else False,
                "sourcePath": path
            }
            records.append(self._finalize_record(rec, parsed))
        return records
    def get_category_name(self) -> str: return "dungeons"

