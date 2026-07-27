import unittest
import os
from parsers.strings_parser import StringsParser

class TestStringsParser(unittest.TestCase):
    def test_parse_empty(self):
        parser = StringsParser()
        result = parser.parse("nonexistent.xml")
        self.assertEqual(result, {})

if __name__ == '__main__':
    unittest.main()
