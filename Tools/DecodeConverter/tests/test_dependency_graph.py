import unittest
from src.dependency_graph import DependencyGraph

class TestDependencyGraph(unittest.TestCase):
    def test_no_cycle(self):
        g = DependencyGraph()
        g.add_dependency("A", "B")
        g.add_dependency("B", "C")
        self.assertEqual(len(g.find_cycles()), 0)
        
    def test_simple_cycle(self):
        g = DependencyGraph()
        g.add_dependency("A", "B")
        g.add_dependency("B", "A")
        cycles = g.find_cycles()
        self.assertTrue(len(cycles) > 0)
        
    def test_deep_cycle(self):
        g = DependencyGraph()
        g.add_dependency("A", "B")
        g.add_dependency("B", "C")
        g.add_dependency("C", "A")
        cycles = g.find_cycles()
        self.assertTrue(len(cycles) > 0)
        self.assertIn("A", cycles[0])
