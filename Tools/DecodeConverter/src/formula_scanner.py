import os
import re

class FormulaScanner:
    def scan(self, src_root: str):
        formulas = []
        for root, dirs, files in os.walk(src_root):
            for file in files:
                if file == "Formulas.java":
                    with open(os.path.join(root, file), 'r', encoding='utf-8') as f:
                        content = f.read()
                        
                    # Basic block extraction
                    matches = re.finditer(r'(?:public|private|protected)\s+(?:static\s+)?([\w<>\[\]]+)\s+(\w+)\((.*?)\)\s*\{', content)
                    
                    last_end = 0
                    blocks = []
                    for m in matches:
                        if last_end > 0:
                            blocks[-1]["body"] = content[last_end:m.start()]
                        blocks.append({
                            "match": m,
                            "ret_type": m.group(1),
                            "name": m.group(2),
                            "params": m.group(3)
                        })
                        last_end = m.end()
                        
                    if blocks:
                        blocks[-1]["body"] = content[last_end:]
                        
                    for b in blocks:
                        body = b.get("body", "")
                        # Count operators
                        ops = len(re.findall(r'[\+\-\*\/\%]|==|!=|>|<|>=|<=', body))
                        # Count branches
                        branches = len(re.findall(r'\b(if|switch|case|else|while|for)\b', body)) + len(re.findall(r'\?', body))
                        # Math deps
                        deps = []
                        if "Math." in body: deps.append("Math")
                        if "Random" in body: deps.append("Random")
                        
                        classification = "TYPE_A_AUTO_PORT"
                        if branches > 0 or ops > 5:
                            classification = "TYPE_B_MANUAL_REVIEW"
                        if branches > 3 or len(deps) > 0:
                            classification = "TYPE_C_MANUAL_PORT"
                            
                        formulas.append({
                            "name": b["name"],
                            "input": b["params"],
                            "output": b["ret_type"],
                            "dependencies": ",".join(deps) if deps else "None",
                            "calls": len(re.findall(r'[a-zA-Z0-9_]+\(', body)),
                            "operators": ops,
                            "complexity": "High" if classification == "TYPE_C_MANUAL_PORT" else "Medium",
                            "classification": classification,
                            "manualPortRequired": "YES" if classification != "TYPE_A_AUTO_PORT" else "NO"
                        })
        return formulas
        
    def write_inventory(self, formulas: list, out_dir: str):
        os.makedirs(out_dir, exist_ok=True)
        with open(os.path.join(out_dir, "formula_inventory.md"), "w", encoding="utf-8") as f:
            f.write("# Formula Inventory\n\n")
            f.write("| Name | Input | Output | Dependencies | Calls | Operators | Complexity | Class | Manual Port |\n")
            f.write("|---|---|---|---|---|---|---|---|---|\n")
            for form in formulas:
                f.write(f"| {form['name']} | {form['input']} | {form['output']} | {form['dependencies']} | {form['calls']} | {form['operators']} | {form['complexity']} | {form['classification']} | {form['manualPortRequired']} |\n")
