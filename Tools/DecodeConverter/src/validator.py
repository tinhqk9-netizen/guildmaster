import re
import os
from typing import List, Dict
from src.models import ValidationReport, ValidationIssue

class Validator:
    def __init__(self):
        self.report = ValidationReport()
        self.seen_ids = set()

    def validate_id(self, entity_id: str, source: str):
        if not entity_id:
            self._add_issue("FATAL", source, entity_id, "V-001", "ID is empty", "Fix normalizer")
            return
            
        if entity_id in self.seen_ids:
            self._add_issue("FATAL", source, entity_id, "V-002", "Duplicate ID found", "Check for conflicts")
        else:
            self.seen_ids.add(entity_id)

        if not re.match(r'^[a-z0-9_]+$', entity_id):
            if re.search(r'[A-Z]', entity_id):
                self._add_issue("FATAL", source, entity_id, "V-003", "ID contains uppercase", "Check normalizer")
            elif re.search(r'\s', entity_id):
                self._add_issue("FATAL", source, entity_id, "V-004", "ID contains whitespace", "Check normalizer")
            else:
                self._add_issue("FATAL", source, entity_id, "V-005", "ID contains invalid character", "Must be lower snake_case")
            
        if not os.path.exists(source):
            self._add_issue("WARNING", source, entity_id, "V-006", "Invalid source path", "File not found on disk")

    def validate_resource_key(self, key: str, source: str, known_keys: set):
        if key in known_keys:
            self._add_issue("ERROR", source, key, "V-007", "Duplicate resource key", "Check original XML")
        else:
            known_keys.add(key)
            
    def validate_missing(self, entity_id: str, source: str, missing_type: str, missing_val: str):
        if missing_type == "localization":
            self._add_issue("WARNING", source, entity_id, "V-008", f"Missing localization for {missing_val}", "Fallback to raw ID")
        elif missing_type == "asset":
            self._add_issue("WARNING", source, entity_id, "V-009", f"Missing asset {missing_val}", "Fallback to default icon")
        elif missing_type == "inheritance":
            self._add_issue("FATAL", source, entity_id, "V-010", f"Invalid inheritance parent: {missing_val}", "Check parent class")
        else:
            self._add_issue("ERROR", source, entity_id, "V-011", f"Missing required entity: {missing_val}", "Check references")

    def _add_issue(self, severity, source, entity_id, rule_id, message, action):
        issue = ValidationIssue(severity, source, entity_id, rule_id, message, action)
        self.report.issues.append(issue)
        self.report.issues_by_severity[severity] += 1
        self.report.total_issues += 1
