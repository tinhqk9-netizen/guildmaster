import unittest
import os
import sys
import tempfile
import json
import shutil
from src.cli import CLI
from src.config_loader import ConfigLoader
from src.strings_parser import StringsParser
from src.asset_scanner import AssetScanner
from parsers.recipe_parser import RecipeParser
from parsers.base_parser import BaseParser

class MockContext:
    def __init__(self):
        self.unsupported_constructs = []

class TestS006Review(unittest.TestCase):
    def test_resource_root_correct(self):
        config = ConfigLoader.load("config/production_profile.json")
        res = config.get("resourcesRoot")
        self.assertEqual(res, "resources/res")
        
    def test_localization_not_empty(self):
        parser = StringsParser()
        xml_path = r"D:\Tinh\Guild Master - Idle Dungeons\resources\res\values\strings.xml"
        if os.path.exists(xml_path):
            strings = parser.parse(xml_path)
            self.assertGreater(len(strings), 0)
            
    def test_recipe_false_positive_filtering(self):
        content = """
        public enum Recipes {
            true_item(Item.getInstance("ingredient_1", 1)),
            another_item(Item.getInstance("ingredient_2", 2));
            
            // Methods after semi-colon
            public static void someHelper() {
                Item.getInstance("false_positive", 1);
            }
        }
        """
        parser = RecipeParser()
        with tempfile.NamedTemporaryFile("w", encoding="utf-8-sig", delete=False, suffix="Recipes.java") as f:
            f.write(content)
            fpath = f.name
            
        try:
            records = parser.parse_files(MockContext(), [fpath])
            self.assertEqual(len(records), 2)
            self.assertEqual(records[0]["outputItemId"], "true_item")
            self.assertEqual(records[0]["ingredients"][0]["itemId"], "ingredient_1")
            self.assertEqual(records[0]["ingredients"][0]["amount"], 1)
        finally:
            os.remove(fpath)
            
    def test_parseStatus_required_fields(self):
        class DummyParser(BaseParser):
            def get_category_name(self): return "items"
            def parse_files(self, ctx, files): pass
        
        parser = DummyParser()
        # Item without className
        rec1 = {"id": "item1"}
        rec1 = parser._finalize_record(rec1)
        self.assertEqual(rec1["parseStatus"], "partial")
        self.assertIn("MISSING_CLASSNAME", rec1["parseReasons"])
        
        # Item with className
        rec2 = {"id": "item2", "className": "Weapon"}
        rec2 = parser._finalize_record(rec2)
        self.assertEqual(rec2["parseStatus"], "full")
        self.assertEqual(rec2["parseReasons"], [])
        
    def test_webp_short_header(self):
        scanner = AssetScanner()
        with tempfile.NamedTemporaryFile("wb", delete=False) as f:
            f.write(b"RIFF\x14\x00\x00\x00WEBPVP8 ") # Short header
            fpath = f.name
            
        try:
            w, h, fmt, status = scanner._read_dimensions(fpath, "short.webp")
            self.assertEqual(status, "FAILED_SHORT_FILE")
        finally:
            os.remove(fpath)
            
    def test_corrupt_jpeg(self):
        scanner = AssetScanner()
        with tempfile.NamedTemporaryFile("wb", delete=False) as f:
            f.write(b"\xff\xd8\xff\xc0\x00" + b"\x00" * 20) # Corrupt but long enough
            fpath = f.name
            
        try:
            w, h, fmt, status = scanner._read_dimensions(fpath, "corrupt.jpg")
            self.assertIn(status, ["FAILED_EXCEPTION", "FAILED_UNKNOWN"])
        finally:
            os.remove(fpath)
            
if __name__ == '__main__':
    unittest.main()



