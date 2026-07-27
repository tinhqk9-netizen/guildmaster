from parsers.base_parser import BaseParser
from typing import List, Dict, Any

class ItemParser(BaseParser):
    def parse_files(self, context, files: List[str]) -> List[Dict[str, Any]]:
        records = []
        for path in files:
            parsed = self.parse_file(path)
            if not parsed.class_name: continue
            
            # Resolve inheritance immediately if context allows (for now, just use fields/assignments)
            # Full inheritance resolution happens in pipeline manager
            
            item_id = self.normalizer.normalize(parsed.class_name)
            
            fields = parsed.fields
            for a in parsed.assignments:
                fields[a.field] = a.value
                
            name_key = fields.get("name") or fields.get("nameKey")
            icon_key = fields.get("icon") or fields.get("iconKey")
            
            if not name_key:
                context.validator._add_issue("WARNING", path, item_id, "V-MISS-NAME", "nameKey not found", "Check Java")
            
            rec = {
                "id": item_id,
                "className": parsed.class_name,
                "parentClass": parsed.parent_class,
                "nameKey": name_key,
                "iconKey": icon_key,
                "price": fields.get("price", 0),
                "rarity": fields.get("rarity", "COMMON"),
                "sourcePath": path,
                "fields": context.semantic_tagger.tag_fields(fields),
            }
            rec = self._finalize_record(rec, parsed)
            records.append(rec)
        return records

    def get_category_name(self) -> str:
        return "items"


