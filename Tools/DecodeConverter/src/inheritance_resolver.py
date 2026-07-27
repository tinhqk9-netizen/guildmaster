from typing import Dict, List, Any
import copy
import logging

class InheritanceResolver:
    def __init__(self):
        self.class_map = {}
        self.resolved_cache = {}
        self.resolving_stack = set()

    def build_graph(self, parsed_classes: list):
        for c in parsed_classes:
            if c.class_name:
                self.class_map[c.class_name] = c

    def resolve(self, class_name: str) -> dict:
        if class_name in self.resolved_cache:
            return self.resolved_cache[class_name]
            
        if class_name in self.resolving_stack:
            raise RecursionError(f"Circular inheritance detected at: {class_name}")
            
        if class_name not in self.class_map:
            # Missing or external parent
            return {}
            
        self.resolving_stack.add(class_name)
        cls = self.class_map[class_name]
        
        merged_fields = {}
        if cls.parent_class:
            try:
                parent_fields = self.resolve(cls.parent_class)
                merged_fields.update(parent_fields)
            except RecursionError as e:
                # Re-raise to be handled by caller as FATAL
                raise e
                
        # Override with current class fields
        merged_fields.update(copy.deepcopy(cls.fields))
        
        # Override with assignments (this.field = value)
        for assignment in cls.assignments:
            merged_fields[assignment.field] = assignment.value
            
        self.resolved_cache[class_name] = merged_fields
        self.resolving_stack.remove(class_name)
        return merged_fields
