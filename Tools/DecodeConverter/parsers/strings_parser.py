import xml.etree.ElementTree as ET
import os

class StringsParser:
    @staticmethod
    def parse(path: str) -> dict:
        result = {}
        if not os.path.exists(path):
            return result
        try:
            tree = ET.parse(path)
            root = tree.getroot()
            for child in root.findall('string'):
                name = child.get('name')
                text = child.text
                if name and text:
                    result[name] = text
        except Exception as e:
            pass
        return result
