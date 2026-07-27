import unittest
from src.reference_resolver import ReferenceResolver
from src.validator import Validator

class TestReferenceResolver(unittest.TestCase):
    def setUp(self):
        self.val = Validator()
        alias = {"items": {"old_id": "new_id"}}
        mig = {"removed_id": {"remove": True}, "renamed_id": {"rename": "final_id"}}
        self.resolver = ReferenceResolver(alias, mig, self.val)
        
    def test_alias_resolution(self):
        self.assertEqual(self.resolver.resolve_id("old_id", "items", "src"), "new_id")
        self.assertEqual(self.resolver.resolve_id("other_id", "items", "src"), "other_id")
        
    def test_migration_resolution(self):
        self.assertIsNone(self.resolver.resolve_id("removed_id", "items", "src"))
        self.assertEqual(self.resolver.resolve_id("renamed_id", "items", "src"), "final_id")
