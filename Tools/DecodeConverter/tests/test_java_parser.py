import unittest
from src.java_parser import JavaParser

class TestJavaParser(unittest.TestCase):
    def setUp(self):
        self.parser = JavaParser()

    def test_parse_basic_class(self):
        content = '''
        package com.test;
        class AbyssalCutlass extends Weapon {
            public static final int maxLevel = 10;
            public AbyssalCutlass() {
                this.name = R.string.cutlass_name;
                this.icon = R.drawable.cutlass_icon;
            }
        }
        '''
        parsed = self.parser.parse("path/to/file.java", content)
        self.assertEqual(parsed.class_name, "AbyssalCutlass")
        self.assertEqual(parsed.parent_class, "Weapon")
        self.assertEqual(parsed.package_name, "com.test")
        self.assertIn("maxLevel", parsed.fields)
        self.assertEqual(parsed.fields["maxLevel"], 10)
        
        string_refs = [r.value for r in parsed.references if r.ref_type == 'string']
        self.assertIn("cutlass_name", string_refs)
        
        drawable_refs = [r.value for r in parsed.references if r.ref_type == 'drawable']
        self.assertIn("cutlass_icon", drawable_refs)

if __name__ == '__main__':
    unittest.main()
