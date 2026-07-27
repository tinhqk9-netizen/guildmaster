import os

class JavaReader:
    @staticmethod
    def read(path: str) -> str:
        if not os.path.exists(path):
            return ""
        with open(path, 'r', encoding='utf-8-sig') as f:
            return f.read()
