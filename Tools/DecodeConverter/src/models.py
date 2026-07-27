from dataclasses import dataclass, field
from typing import List, Dict, Any, Optional

@dataclass
class SourceMetadata:
    source_path: str
    class_name: str

@dataclass
class ParsedAssignment:
    field: str
    value: Any
    raw_statement: str
    semantic_tag: Optional[str] = None

@dataclass
class ParsedCall:
    object_name: str
    method_name: str
    arguments: List[Any]
    raw_statement: str

@dataclass
class ParsedReference:
    ref_type: str
    value: str

@dataclass
class UnsupportedConstruct:
    source_file: str
    line_number: int
    context: str # class/method
    raw_statement: str
    construct_type: str
    reason: str
    severity: str
    recommended_handling: str

@dataclass
class ParsedJavaClass:
    source_path: str
    package_name: str
    class_name: str
    parent_class: Optional[str] = None
    interfaces: List[str] = field(default_factory=list)
    fields: Dict[str, Any] = field(default_factory=dict)
    assignments: List[ParsedAssignment] = field(default_factory=list)
    calls: List[ParsedCall] = field(default_factory=list)
    references: List[ParsedReference] = field(default_factory=list)
    warnings: List[str] = field(default_factory=list)
    unsupported: List[UnsupportedConstruct] = field(default_factory=list)
    statements_detected: int = 0
    statements_parsed: int = 0

@dataclass
class ValidationIssue:
    severity: str
    source_file: str
    entity_id: str
    rule_id: str
    message: str
    suggested_action: str

@dataclass
class ValidationReport:
    total_issues: int = 0
    issues_by_severity: Dict[str, int] = field(default_factory=lambda: {"INFO": 0, "WARNING": 0, "ERROR": 0, "FATAL": 0})
    issues: List[ValidationIssue] = field(default_factory=list)

@dataclass
class ExportManifest:
    schema_version: str
    definitions_version: str
    generated_at: str
    record_count: int

@dataclass
class ConverterResult:
    success: bool
    report: ValidationReport
