from typing import Dict
from src.validator import Validator

class ReferenceResolver:
    def __init__(self, alias_map: dict, migration_rules: dict, validator: Validator):
        self.alias_map = alias_map
        self.migration_rules = migration_rules
        self.validator = validator

    def resolve_id(self, original_id: str, category: str, source_path: str) -> str:
        # Check alias
        if category in self.alias_map and original_id in self.alias_map[category]:
            return self.alias_map[category][original_id]
            
        # Check migration rules
        if original_id in self.migration_rules:
            rule = self.migration_rules[original_id]
            if rule.get("remove", False):
                return None
            if "rename" in rule:
                return rule["rename"]
            if "fallback" in rule:
                return rule["fallback"]
                
        return original_id
