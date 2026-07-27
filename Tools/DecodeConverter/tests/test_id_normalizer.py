import unittest
from src.id_normalizer import IDNormalizer

class TestIDNormalizer(unittest.TestCase):
    def setUp(self):
        self.normalizer = IDNormalizer()

    def test_normalize_pascal_case(self):
        self.assertEqual(self.normalizer.normalize("AbyssalCutlass"), "abyssal_cutlass")
        self.assertEqual(self.normalizer.normalize("NPCMerchant"), "npc_merchant")
        self.assertEqual(self.normalizer.normalize("Fire2Spell"), "fire_2_spell")
        self.assertEqual(self.normalizer.normalize("StatusEffectType"), "status_effect_type")

if __name__ == '__main__':
    unittest.main()
