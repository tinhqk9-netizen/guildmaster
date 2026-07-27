import json
import os
from src.models import ExportManifest

class Exporter:
    @staticmethod
    def export(data: dict, out_path: str, manifest: ExportManifest):
        dir_name = os.path.dirname(out_path)
        if dir_name:
            os.makedirs(dir_name, exist_ok=True)
        final_data = {
            "metadata": {
                "schemaVersion": manifest.schema_version,
                "definitionsVersion": manifest.definitions_version,
                "generatedAt": manifest.generated_at,
                "recordCount": manifest.record_count
            },
            "data": data
        }
        with open(out_path, 'w', encoding='utf-8') as f:
            json.dump(final_data, f, ensure_ascii=False, indent=2, sort_keys=True)
