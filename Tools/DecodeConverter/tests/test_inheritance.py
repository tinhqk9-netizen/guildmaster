import unittest
from src.inheritance_resolver import InheritanceResolver
from src.models import ParsedJavaClass

class TestInheritance(unittest.TestCase):
    def setUp(self):
        self.resolver = InheritanceResolver()
        
    def test_simple_inheritance(self):
        c1 = ParsedJavaClass("path", "pkg", "Parent", None, fields={"hp": 10})
        c2 = ParsedJavaClass("path", "pkg", "Child", "Parent", fields={"dmg": 5})
        self.resolver.build_graph([c1, c2])
        res = self.resolver.resolve("Child")
        self.assertEqual(res["hp"], 10)
        self.assertEqual(res["dmg"], 5)
        
    def test_override_inheritance(self):
        c1 = ParsedJavaClass("path", "pkg", "Parent", None, fields={"hp": 10})
        c2 = ParsedJavaClass("path", "pkg", "Child", "Parent", fields={"hp": 20})
        self.resolver.build_graph([c1, c2])
        res = self.resolver.resolve("Child")
        self.assertEqual(res["hp"], 20)
        
    def test_circular_inheritance(self):
        c1 = ParsedJavaClass("path", "pkg", "A", "B")
        c2 = ParsedJavaClass("path", "pkg", "B", "A")
        self.resolver.build_graph([c1, c2])
        with self.assertRaises(RecursionError):
            self.resolver.resolve("A")
            
    def test_missing_parent(self):
        c1 = ParsedJavaClass("path", "pkg", "Child", "MissingParent", fields={"hp": 10})
        self.resolver.build_graph([c1])
        res = self.resolver.resolve("Child")
        self.assertEqual(res["hp"], 10) # Resolves but parent is ignored
