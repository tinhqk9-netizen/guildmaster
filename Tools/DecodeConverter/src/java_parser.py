import re
from typing import Optional, List, Tuple, Any
from src.models import ParsedJavaClass, ParsedReference, ParsedAssignment, ParsedCall, UnsupportedConstruct

class JavaParser:
    @staticmethod
    def parse(source_path: str, content: str) -> ParsedJavaClass:
        parsed = ParsedJavaClass(source_path=source_path, package_name="", class_name="")
        
        pkg_match = re.search(r'package\s+([\w\.]+);', content)
        if pkg_match: parsed.package_name = pkg_match.group(1)

        class_match = re.search(r'class\s+(\w+)(?:\s+extends\s+(\w+))?(?:\s+implements\s+([^{]+))?', content)
        if class_match:
            parsed.class_name = class_match.group(1)
            parsed.parent_class = class_match.group(2)
            if class_match.group(3):
                parsed.interfaces = [i.strip() for i in class_match.group(3).split(',')]

        statements = JavaParser._extract_statements(content)
        parsed.statements_detected = len(statements)
        
        for idx, stmt in enumerate(statements):
            stmt = stmt.strip()
            
            for match in re.finditer(r'R\.string\.(\w+)', stmt):
                parsed.references.append(ParsedReference("string", match.group(1)))
            for match in re.finditer(r'R\.drawable\.(\w+)', stmt):
                parsed.references.append(ParsedReference("drawable", match.group(1)))
                
            # Detect unsupported lambda or anonymous method
            if "->" in stmt:
                parsed.unsupported.append(UnsupportedConstruct(source_path, idx+1, parsed.class_name, stmt, "Lambda", "Unsupported lambda syntax", "WARNING", "Manual implementation"))
                continue
                
            parsed.statements_parsed += 1

            # a) Field decl
            f_match = re.match(r'(?:(?:public|private|protected)\s+)?(?:static\s+)?(?:final\s+)?(?:int|long|float|double|boolean|String|[\w<>\[\]]+)\s+(\w+)\s*=\s*(.+)', stmt, re.DOTALL)
            if f_match:
                fname = f_match.group(1)
                fval = JavaParser._parse_value(f_match.group(2), parsed, stmt, idx+1)
                parsed.fields[fname] = fval
                continue

            # b) Assignment
            a_match = re.match(r'(?:this\.)?(\w+)\s*=\s*(.+)', stmt, re.DOTALL)
            if a_match and a_match.group(1) not in ["if", "for", "while", "return"]:
                fname = a_match.group(1)
                fval = JavaParser._parse_value(a_match.group(2), parsed, stmt, idx+1)
                parsed.assignments.append(ParsedAssignment(fname, fval, stmt))
                continue

            # c) Method calls
            c_match = re.match(r'(?:this\.)?(\w+)\.(add|put)\((.*)\)', stmt, re.DOTALL)
            if c_match:
                obj = c_match.group(1)
                mth = c_match.group(2)
                args_str = c_match.group(3)
                args = [JavaParser._parse_value(arg.strip(), parsed, stmt, idx+1) for arg in JavaParser._split_args(args_str)]
                parsed.calls.append(ParsedCall(obj, mth, args, stmt))

        return parsed

    @staticmethod
    def _extract_statements(content: str) -> List[str]:
        statements = []
        current_stmt = []
        in_string = False
        parens = 0
        brackets = 0
        
        content = re.sub(r'//.*', '', content)
        content = re.sub(r'/\*.*?\*/', '', content, flags=re.DOTALL)
        
        lines = content.split('\n')
        for line in lines:
            line = line.strip()
            if not line: continue
            
            i = 0
            while i < len(line):
                char = line[i]
                current_stmt.append(char)
                
                if char == '"' and (i == 0 or line[i-1] != '\\'):
                    in_string = not in_string
                elif not in_string:
                    if char == '(': parens += 1
                    elif char == ')': parens -= 1
                    elif char == '[': brackets += 1
                    elif char == ']': brackets -= 1
                    elif char == ';' and parens == 0 and brackets == 0:
                        statements.append(''.join(current_stmt).strip(';').strip())
                        current_stmt = []
                    elif char == '{' or char == '}':
                        current_stmt = []
                i += 1
            if current_stmt:
                current_stmt.append(' ')
                
        return [s.strip() for s in statements if s.strip()]

    @staticmethod
    def _parse_value(val_str: str, parsed_ref: ParsedJavaClass, stmt: str, line_no: int) -> Any:
        val_str = val_str.strip()
        if not val_str: return None
        
        if "->" in val_str:
            parsed_ref.unsupported.append(UnsupportedConstruct(parsed_ref.source_path, line_no, parsed_ref.class_name, stmt, "Lambda", "Unsupported lambda expression", "WARNING", "Manual conversion"))
            return {"_type": "unsupported", "reason": "lambda"}
            
        return JavaParser._parse_expression(val_str, parsed_ref, stmt, line_no)
        
    @staticmethod
    def _parse_expression(expr: str, parsed_ref: ParsedJavaClass, stmt: str, line_no: int) -> Any:
        expr = expr.strip()
        
        if expr == "true": return True
        if expr == "false": return False
        if expr == "null": return None
        
        if expr.startswith('"') and expr.endswith('"'):
            return expr[1:-1]
            
        if expr.startswith('R.string.'): return expr.replace('R.string.', '')
        if expr.startswith('R.drawable.'): return expr.replace('R.drawable.', '')
            
        if expr.startswith('{') and expr.endswith('}'):
            inner = expr[1:-1]
            args = JavaParser._split_args(inner)
            return [JavaParser._parse_expression(a, parsed_ref, stmt, line_no) for a in args]
            
        new_match = re.match(r'^new\s+([\w<>]+)\s*\((.*)\)$', expr, re.DOTALL)
        if new_match:
            args = JavaParser._split_args(new_match.group(2))
            return {"_type": "new_object", "class": new_match.group(1), "args": [JavaParser._parse_expression(a, parsed_ref, stmt, line_no) for a in args]}
            
        super_match = re.match(r'^super\s*\((.*)\)$', expr, re.DOTALL)
        if super_match:
            args = JavaParser._split_args(super_match.group(1))
            return {"_type": "super_call", "args": [JavaParser._parse_expression(a, parsed_ref, stmt, line_no) for a in args]}
            
        this_match = re.match(r'^this\s*\((.*)\)$', expr, re.DOTALL)
        if this_match:
            args = JavaParser._split_args(this_match.group(1))
            return {"_type": "this_call", "args": [JavaParser._parse_expression(a, parsed_ref, stmt, line_no) for a in args]}
            
        enum_match = re.match(r'^([A-Z]\w+)\.([A-Z0-9_]+)$', expr)
        if enum_match:
            return {"_type": "enum_ref", "class": enum_match.group(1), "value": enum_match.group(2)}
            
        try:
            if 'f' in expr.lower() or 'd' in expr.lower() or '.' in expr:
                return float(expr.replace('f', '').replace('F', '').replace('d', '').replace('D', ''))
            return int(expr.replace('L', '').replace('l', ''))
        except ValueError:
            pass
            
        if '?' in expr and ':' in expr and not expr.startswith('"'):
            return {"_type": "ternary", "raw": expr}
            
        if re.search(r'[\+\-\*/]', expr) and not expr.startswith('"'):
            return {"_type": "arithmetic", "raw": expr}
            
        mcall = re.match(r'^([a-zA-Z0-9_\.]+)\s*\((.*)\)$', expr, re.DOTALL)
        if mcall:
            args = JavaParser._split_args(mcall.group(2))
            return {"_type": "method_call", "method": mcall.group(1), "args": [JavaParser._parse_expression(a, parsed_ref, stmt, line_no) for a in args]}
            
        return expr

    @staticmethod
    def _split_args(args_str: str) -> List[str]:
        args = []
        curr = []
        parens = 0
        brackets = 0
        braces = 0
        in_string = False
        
        i = 0
        while i < len(args_str):
            c = args_str[i]
            if c == '"' and (i == 0 or args_str[i-1] != '\\'):
                in_string = not in_string
            elif not in_string:
                if c == '(': parens += 1
                elif c == ')': parens -= 1
                elif c == '[': brackets += 1
                elif c == ']': brackets -= 1
                elif c == '{': braces += 1
                elif c == '}': braces -= 1
                elif c == ',' and parens == 0 and brackets == 0 and braces == 0:
                    args.append(''.join(curr).strip())
                    curr = []
                    i += 1
                    continue
            curr.append(c)
            i += 1
            
        if curr:
            args.append(''.join(curr).strip())
            
        return [a for a in args if a]
