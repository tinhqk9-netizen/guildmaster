import unittest
from src.semantic_tagger import SemanticTagger
import os

class TestSemanticTagger(unittest.TestCase):
    def setUp(self):
        self.tagger = SemanticTagger(os.path.join(os.getcwd(), "config", "semantic_tags_mapping.json"))

    def test_get_tag(self):
        self.assertEqual(self.tagger.get_tag("damage"), "STAT_DAMAGE")
        self.assertEqual(self.tagger.get_tag("healthBase"), "STAT_HP")
        self.assertEqual(self.tagger.get_tag("unknown_field"), "UNKNOWN")

    def test_tag_fields(self):
        fields = {"damage": 10, "health": 20}
        tagged = self.tagger.tag_fields(fields)
        self.assertEqual(tagged["damage"]["semanticTag"], "STAT_DAMAGE")
        self.assertEqual(tagged["damage"]["value"], 10)
        self.assertEqual(tagged["health"]["semanticTag"], "STAT_HP")
