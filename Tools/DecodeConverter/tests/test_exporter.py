import unittest
import os
from src.exporter import Exporter
from src.models import ExportManifest

class TestExporter(unittest.TestCase):
    def test_export_structure(self):
        manifest = ExportManifest("1.0", "1.0", "now", 0)
        out_path = "test_out.json"
        Exporter.export({"test": "data"}, out_path, manifest)
        self.assertTrue(os.path.exists(out_path))
        os.remove(out_path)

if __name__ == '__main__':
    unittest.main()
