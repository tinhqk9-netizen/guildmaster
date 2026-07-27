import unittest
from src.java_parser import JavaParser

class TestJavaParserAdvanced(unittest.TestCase):
    def setUp(self):
        self.parser = JavaParser()

    def test_multi_line_statement(self):
        content = '''
        class Test {
            public Test() {
                this.name = 
                    R.string.abc;
            }
        }
        '''
        p = self.parser.parse("src", content)
        self.assertEqual(p.assignments[0].value, "abc")
        
    def test_list_add(self):
        content = '''
        class Test {
            public Test() {
                list.add(new Object(1, 2));
            }
        }
        '''
        p = self.parser.parse("src", content)
        self.assertEqual(p.calls[0].method_name, "add")
        
    def test_map_put(self):
        content = '''
        class Test {
            public Test() {
                map.put("key", 100);
            }
        }
        '''
        p = self.parser.parse("src", content)
        self.assertEqual(p.calls[0].method_name, "put")
        
    def test_boolean_and_null(self):
        content = '''
        class Test {
            public Test() {
                this.isTrue = true;
                this.isFalse = false;
                this.isNull = null;
            }
        }
        '''
        p = self.parser.parse("src", content)
        vals = {a.field: a.value for a in p.assignments}
        self.assertEqual(vals["isTrue"], True)
        self.assertEqual(vals["isFalse"], False)
        self.assertIsNone(vals["isNull"])
        
    def test_numbers(self):
        content = '''
        class Test {
            public Test() {
                this.v1 = 10L;
                this.v2 = 1.5f;
            }
        }
        '''
        p = self.parser.parse("src", content)
        vals = {a.field: a.value for a in p.assignments}
        self.assertEqual(vals["v1"], 10)
        self.assertEqual(vals["v2"], 1.5)
        
    def test_enum(self):
        content = '''
        class Test {
            public Test() {
                this.type = Type.FIRE;
            }
        }
        '''
        p = self.parser.parse("src", content)
        self.assertEqual(p.assignments[0].value["value"], "FIRE")
        self.assertEqual(p.assignments[0].value["class"], "Type")

