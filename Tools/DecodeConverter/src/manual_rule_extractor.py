import os
import json

class ManualRuleExtractor:
    def extract(self, context, out_dir: str):
        rules = []
        for u in context.unsupported_constructs:
            rules.append({
                "id": f"unsupported_{len(rules)}",
                "category": "unsupported",
                "sourceFile": u.source_file,
                "className": u.context,
                "methodName": "unknown",
                "line": u.line_number,
                "rawStatement": u.raw_statement,
                "reason": u.reason,
                "severity": u.severity,
                "requiredForMVP": True,
                "suggestedUnityModule": u.recommended_handling,
                "dependency": "None"
            })
            
        os.makedirs(out_dir, exist_ok=True)
        with open(os.path.join(out_dir, "manual_rules.json"), "w", encoding="utf-8") as f:
            json.dump(rules, f, indent=4)
            
        with open(os.path.join(out_dir, "manual_rules.md"), "w", encoding="utf-8") as f:
            f.write("# Manual Rule Inventory\n\n")
            f.write("| ID | Category | File | Line | Reason | Severity |\n")
            f.write("|---|---|---|---|---|---|\n")
            for r in rules:
                f.write(f"| {r['id']} | {r['category']} | {os.path.basename(r['sourceFile'])} | {r['line']} | {r['reason']} | {r['severity']} |\n")
