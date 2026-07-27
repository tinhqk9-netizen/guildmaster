from parsers.base_parser import BaseParser
from typing import List, Dict, Any

class PetParser(BaseParser):
    def parse_files(self, context, files: List[str]) -> List[Dict[str, Any]]:
        records = []
        for path in files:
            parsed = self.parse_file(path)
            if not parsed.class_name: continue
            rec_id = self.normalizer.normalize(parsed.class_name)
            rec = {"id": rec_id, "className": parsed.class_name, "sourcePath": path}
            records.append(self._finalize_record(rec, parsed))
        return records
    def get_category_name(self) -> str: return "pets"

