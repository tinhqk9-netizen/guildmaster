import xml.etree.ElementTree as ET
import re
import os

class StringsParser:
    def parse(self, xml_path: str):
        if not os.path.exists(xml_path):
            return {}
            
        tree = ET.parse(xml_path)
        root = tree.getroot()
        strings = {}
        
        for string_elem in root.findall('string'):
            key = string_elem.get('name')
            if not key: continue
            
            # Handle CDATA and formatting
            text = ET.tostring(string_elem, encoding='unicode', method='xml')
            text = re.sub(r'<string name=".*?">', '', text)
            text = re.sub(r'</string>', '', text)
            
            formatted = string_elem.get('formatted', 'true').lower() == 'true'
            format_args = len(re.findall(r'%[0-9]*\$?[a-zA-Z]', text))
            
            strings[key] = {
                "key": key,
                "text": text,
                "formatted": formatted,
                "formatArguments": format_args,
                "sourcePath": xml_path,
                "sourceType": "string"
            }
            
        for plural_elem in root.findall('plurals'):
            key = plural_elem.get('name')
            if not key: continue
            for item in plural_elem.findall('item'):
                quant = item.get('quantity')
                text = item.text or ""
                strings[f"{key}_{quant}"] = {
                    "key": f"{key}_{quant}",
                    "text": text,
                    "formatted": True,
                    "formatArguments": len(re.findall(r'%[0-9]*\$?[a-zA-Z]', text)),
                    "sourcePath": xml_path,
                    "sourceType": "plural"
                }
                
        for arr_elem in root.findall('string-array'):
            key = arr_elem.get('name')
            if not key: continue
            items = []
            for item in arr_elem.findall('item'):
                items.append(item.text or "")
            strings[key] = {
                "key": key,
                "text": items,
                "formatted": False,
                "formatArguments": 0,
                "sourcePath": xml_path,
                "sourceType": "array"
            }
            
        return strings
