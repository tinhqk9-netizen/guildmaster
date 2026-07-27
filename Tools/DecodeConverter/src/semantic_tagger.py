import json
import os
from typing import Dict, Any

class SemanticTagger:
    def __init__(self, config_path: str):
        self.mapping = {}
        if os.path.exists(config_path):
            with open(config_path, 'r', encoding='utf-8-sig') as f:
                self.mapping = json.load(f)
                
    def get_tag(self, field_name: str) -> str:
        # Exact match
        if field_name in self.mapping:
            return self.mapping[field_name]
            
        # Contains match (case insensitive)
        fname_lower = field_name.lower()
        for k, v in self.mapping.items():
            if k.lower() in fname_lower:
                return v
                
        return "UNKNOWN"
        
    def tag_fields(self, fields_dict: Dict[str, Any]) -> Dict[str, Dict[str, Any]]:
        tagged = {}
        for k, v in fields_dict.items():
            tagged[k] = {
                "fieldName": k,
                "semanticTag": self.get_tag(k),
                "value": v
            }
        return tagged

