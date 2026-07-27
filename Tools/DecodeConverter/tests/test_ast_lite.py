import unittest
from src.java_parser import JavaParser
from src.models import ParsedJavaClass

class TestASTLite(unittest.TestCase):
    def setUp(self):
        self.parser = JavaParser()

    def test_parse_new_object(self):
        val = JavaParser._parse_value('new Object(1, "test")', ParsedJavaClass("", "", ""), "", 1)
        self.assertEqual(val["_type"], "new_object")
        self.assertEqual(val["class"], "Object")
        self.assertEqual(val["args"][0], 1)
        self.assertEqual(val["args"][1], "test")

    def test_parse_nested_object(self):
        val = JavaParser._parse_value('new Outer(new Inner(1))', ParsedJavaClass("", "", ""), "", 1)
        self.assertEqual(val["_type"], "new_object")
        self.assertEqual(val["args"][0]["_type"], "new_object")
        self.assertEqual(val["args"][0]["args"][0], 1)

    def test_parse_array(self):
        val = JavaParser._parse_value('{1, 2, 3}', ParsedJavaClass("", "", ""), "", 1)
        self.assertEqual(val, [1, 2, 3])

    def test_parse_ternary(self):
        val = JavaParser._parse_value('a > b ? 1 : 0', ParsedJavaClass("", "", ""), "", 1)
        self.assertEqual(val["_type"], "ternary")

    def test_parse_arithmetic(self):
        val = JavaParser._parse_value('a + b * c', ParsedJavaClass("", "", ""), "", 1)
        self.assertEqual(val["_type"], "arithmetic")

    def test_parse_super(self):
        val = JavaParser._parse_value('super(1, 2)', ParsedJavaClass("", "", ""), "", 1)
        self.assertEqual(val["_type"], "super_call")
        self.assertEqual(val["args"][0], 1)

    def test_unsupported_lambda(self):
        p = ParsedJavaClass("", "", "")
        val = JavaParser._parse_value('() -> { return 1; }', p, "", 1)
        self.assertEqual(val["_type"], "unsupported")
        self.assertTrue(len(p.unsupported) > 0)
