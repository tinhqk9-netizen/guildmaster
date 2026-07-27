import json
import os
from src.models import ValidationReport

class ReportWriter:
    @staticmethod
    def write(report: ValidationReport, out_dir: str):
        os.makedirs(out_dir, exist_ok=True)
        
        # summary.md
        summary = f"# Validation Summary\n\nTotal issues: {report.total_issues}\n"
        for k, v in report.issues_by_severity.items():
            summary += f"- {k}: {v}\n"
        
        with open(os.path.join(out_dir, 'summary.md'), 'w', encoding='utf-8') as f:
            f.write(summary)
            
        # validation.json
        issues = [{"severity": i.severity, "source": i.source_file, "id": i.entity_id, "message": i.message} for i in report.issues]
        with open(os.path.join(out_dir, 'validation.json'), 'w', encoding='utf-8') as f:
            json.dump(issues, f, indent=2)
