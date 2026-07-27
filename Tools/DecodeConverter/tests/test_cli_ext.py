import unittest
from src.cli import CLI
import sys

class TestCLIExtended(unittest.TestCase):
    def test_convert_invalid_category(self):
        cli = CLI()
        # Should raise SystemExit because argparse will block invalid choices
        with self.assertRaises(SystemExit):
            cli.run(["convert-invalid"])
