import unittest
from src.cli import CLI

class TestCLI(unittest.TestCase):
    def test_cli_init(self):
        cli = CLI()
        self.assertIsNotNone(cli.parser)

if __name__ == '__main__':
    unittest.main()
