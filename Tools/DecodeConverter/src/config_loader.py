import json
import os

class ConfigLoader:
    @staticmethod
    def load(path: str) -> dict:
        if not os.path.exists(path):
            return {}
        with open(path, 'r', encoding='utf-8-sig') as f:
            return json.load(f)
