import re

class IDNormalizer:
    @staticmethod
    def normalize(pascal_case: str) -> str:
        if not pascal_case:
            return ""
        s1 = re.sub('(.)([A-Z][a-z]+)', r'\1_\2', pascal_case)
        s2 = re.sub('([a-z0-9])([A-Z])', r'\1_\2', s1)
        s3 = re.sub('([a-zA-Z])([0-9])', r'\1_\2', s2)
        return s3.lower().strip()
