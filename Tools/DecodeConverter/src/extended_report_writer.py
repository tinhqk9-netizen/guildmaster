import os
import json
from src.models import ValidationReport

class ExtendedReportWriter:
    @staticmethod
    def write_coverage_report(out_dir: str, stats_by_cat: dict):
        os.makedirs(out_dir, exist_ok=True)
        with open(os.path.join(out_dir, "coverage_report.md"), "w", encoding="utf-8") as f:
            f.write("# Coverage Report\n\n")
            for cat, stats in stats_by_cat.items():
                f.write(f"## {cat}\n")
                f.write(f"- Files scanned: {stats['filesScanned']}\n")
                f.write(f"- Fully parsed: {stats['fullyParsed']}\n")
                f.write(f"- Partially parsed: {stats['partiallyParsed']}\n")
                f.write(f"- Failed: {stats['failed']}\n")
                f.write(f"- Statements detected: {stats['statementsDetected']}\n")
                f.write(f"- Statements parsed: {stats['statementsParsed']}\n")
                f.write(f"- Unsupported statements: {stats['unsupportedStatements']}\n")
                f.write(f"- Total fields: {stats['totalFields']}\n")
                f.write(f"- Coverage percent: {stats['coveragePercent']:.2f}%\n\n")

    @staticmethod
    def write_unsupported_report(out_dir: str, unsupported_list: list):
        os.makedirs(out_dir, exist_ok=True)
        with open(os.path.join(out_dir, "unsupported_constructs.md"), "w", encoding="utf-8") as f:
            f.write("# Unsupported Constructs\n\n")
            f.write("| File | Line | Context | Statement | Type | Reason | Severity | Handling |\n")
            f.write("|---|---|---|---|---|---|---|---|\n")
            for u in unsupported_list:
                f.write(f"| {os.path.basename(u.source_file)} | {u.line_number} | {u.context} | {u.raw_statement} | {u.construct_type} | {u.reason} | {u.severity} | {u.recommended_handling} |\n")

    @staticmethod
    def write_semantic_tag_report(out_dir: str, tag_counts: dict):
        os.makedirs(out_dir, exist_ok=True)
        with open(os.path.join(out_dir, "semantic_tag_coverage.md"), "w", encoding="utf-8") as f:
            f.write("# Semantic Tag Coverage\n\n")
            for tag, count in tag_counts.items():
                f.write(f"- {tag}: {count} fields\n")

    @staticmethod
    def write_dependency_report(out_dir: str, graph_stats: dict):
        os.makedirs(out_dir, exist_ok=True)
        with open(os.path.join(out_dir, "dependency_statistics.md"), "w", encoding="utf-8") as f:
            f.write("# Dependency Statistics\n\n")
            for k, v in graph_stats.items():
                f.write(f"- {k}: {v}\n")

    @staticmethod
    def write_reference_report(out_dir: str, ref_stats: dict):
        os.makedirs(out_dir, exist_ok=True)
        with open(os.path.join(out_dir, "reference_coverage.md"), "w", encoding="utf-8") as f:
            f.write("# Reference Coverage\n\n")
            for k, v in ref_stats.items():
                f.write(f"- {k}: {v}\n")
