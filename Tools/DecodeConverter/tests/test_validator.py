import unittest
from src.validator import Validator
import os

class TestValidator(unittest.TestCase):
    def setUp(self):
        self.validator = Validator()
        # Mock file existence for test
        self.dummy_path = "test_dummy.java"
        with open(self.dummy_path, "w") as f:
            f.write("")

    def tearDown(self):
        if os.path.exists(self.dummy_path):
            os.remove(self.dummy_path)

    def test_duplicate_id_triggers_fatal(self):
        self.validator.validate_id("wood", self.dummy_path)
        self.validator.validate_id("wood", self.dummy_path)
        self.assertEqual(self.validator.report.issues_by_severity["FATAL"], 1)

    def test_empty_id_triggers_fatal(self):
        self.validator.validate_id("", self.dummy_path)
        self.assertEqual(self.validator.report.issues_by_severity["FATAL"], 1)

if __name__ == '__main__':
    unittest.main()
