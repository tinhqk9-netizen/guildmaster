import unittest
import json
import os
from src.exporter import Exporter
from src.models import ExportManifest

class TestDeterministicSort(unittest.TestCase):
    def test_sort_keys(self):
        data = {"z": 1, "a": 2}
        manifest = ExportManifest("1.0", "1.0", "now", 2)
        out_path = "test_sort.json"
        Exporter.export(data, out_path, manifest)
        
        with open(out_path, 'r', encoding='utf-8') as f:
            content = f.read()
            # In json dump with sort_keys=True, "a" should appear before "z" inside data block
            idx_a = content.find('"a"')
            idx_z = content.find('"z"')
            self.assertTrue(idx_a < idx_z)
            
        os.remove(out_path)

if __name__ == '__main__':
    unittest.main()
