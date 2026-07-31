## 28. Dữ Liệu Thực Tế (Extracted from JSON)

- **Adventurers (classes):** 129
- **Enemies:** 122
- **Dungeons/Areas:** 11
- **Items:** 607
- **Recipes:** 321
- **Skills:** 227 (active: 101, passive: 126)
- **Status Effects:** 25
- **Pets:** 21
- **Raids:** 12
- **Quests:** 56

### 28.1 Adventurer Classes — Chỉ Số Nền

| id | HP | CON | INT | DEX | DEF | MDEF |
|----|----|----|----|----|----|----|
| adept | 25 | 3 | 15 | 5 | 0 | 30 |
| adventurer | 100 | 10 | 10 | 10 | 5 | 5 |
| alchemist | 155 | 9 | 16 | 26 | 10 | 10 |
| ancient_lich | 125 | 8 | 40 | 10 | 0 | 30 |
| angel | 160 | 9 | 45 | 11 | 0 | 30 |
| angel_of_war | 380 | 40 | 20 | 12 | 20 | 40 |
| apprentice | 20 | 2 | 10 | 4 | 0 | 30 |
| archangel | 200 | 10 | 50 | 12 | 0 | 30 |
| archer | 30 | 4 | 4 | 8 | 10 | 10 |
| assassin | 90 | 15 | 7 | 15 | 10 | 10 |
| balrog | 380 | 36 | 36 | 36 | 36 | 36 |
| bard | 155 | 17 | 17 | 17 | 10 | 10 |
| black_idol | 200 | 10 | 50 | 12 | 0 | 30 |
| black_regent | 380 | 40 | 14 | 18 | 20 | 20 |
| blight | 195 | 10 | 16 | 32 | 10 | 10 |
| bone_horror | 315 | 72 | 1 | 17 | 50 | 0 |
| bone_hydra | 390 | 125 | 1 | 24 | 50 | 0 |
| bone_nightmare | 345 | 105 | 1 | 20 | 50 | 0 |
| celestial_rain | 290 | 12 | 20 | 40 | 10 | 10 |
| cleric | 35 | 4 | 20 | 6 | 0 | 30 |
| corrosive_wraith | 290 | 12 | 20 | 40 | 10 | 10 |
| cutthroat | 65 | 12 | 6 | 12 | 10 | 10 |
| dark_knight | 130 | 20 | 9 | 8 | 20 | 20 |
| dark_sorcerer | 35 | 4 | 20 | 6 | 0 | 30 |
| death_knight | 170 | 24 | 10 | 10 | 20 | 20 |
| demilich | 70 | 6 | 30 | 8 | 0 | 30 |
| demon | 215 | 24 | 24 | 24 | 24 | 24 |
| divine_champion | 380 | 40 | 14 | 18 | 20 | 20 |
| divine_duelist | 320 | 36 | 13 | 16 | 20 | 20 |
| doctrine | ? | ? | ? | ? | ? | ? |
| doctrine_ability | ? | ? | ? | ? | ? | ? |
| doctrine_of_affliction | ? | ? | ? | ? | ? | ? |
| doctrine_of_control | ? | ? | ? | ? | ? | ? |
| doctrine_of_fortitude | ? | ? | ? | ? | ? | ? |
| doctrine_of_grace | ? | ? | ? | ? | ? | ? |
| doctrine_of_illusion | ? | ? | ? | ? | ? | ? |
| doctrine_of_knowledge | ? | ? | ? | ? | ? | ? |
| doctrine_of_ruin | ? | ? | ? | ? | ? | ? |
| doctrine_of_war | ? | ? | ? | ? | ? | ? |
| drake_rider | 195 | 10 | 16 | 32 | 10 | 10 |
| eidolon | 290 | 23 | 26 | 23 | 10 | 10 |
| eldritch_alchemist | 290 | 12 | 28 | 32 | 10 | 10 |
| elemental_alchemist | 195 | 10 | 20 | 28 | 10 | 10 |
| empty_doctrine | ? | ? | ? | ? | ? | ? |
| esoteric_alchemist | 240 | 11 | 24 | 30 | 10 | 10 |
| eternal_fortress | 380 | 40 | 20 | 12 | 30 | 30 |
| fire_wizard | 35 | 4 | 20 | 6 | 0 | 30 |
| footman | 40 | 8 | 4 | 4 | 20 | 20 |
| fury | BARD_SHIELD | 8 | 12 | 24 | 10 | 10 |
| golden_rider | 240 | 11 | 18 | 36 | 10 | 10 |
| guard | 95 | 16 | 8 | 6 | 20 | 20 |
| hailstorm | 155 | 9 | 14 | 28 | 10 | 10 |
| heavenly_cantor | 240 | 21 | 23 | 21 | 10 | 10 |
| hellish_sculptor | 240 | 27 | 11 | 27 | 10 | 10 |
| holy_knight | 130 | 20 | 10 | 7 | 20 | 23 |
| horse_rider | 65 | 6 | 8 | 16 | 10 | 10 |
| huntress | 45 | 5 | 6 | 12 | 10 | 10 |
| hurricane | 240 | 11 | 18 | 36 | 10 | 10 |
| infernal_lord | 265 | 28 | 28 | 28 | 28 | 28 |
| infernal_prince | 320 | 32 | 32 | 32 | 32 | 32 |
| inferno | 200 | 10 | 50 | 12 | 0 | 30 |
| inquisitor | 265 | 32 | 16 | 10 | 20 | 33 |
| iron_defender | 170 | 24 | 12 | 8 | 24 | 24 |
| iron_warden | 130 | 20 | 10 | 7 | 22 | 22 |
| juggernaut | 215 | 28 | 14 | 9 | 25 | 25 |
| justiciar | 320 | 36 | 18 | 11 | 20 | 36 |
| kings_hand | 265 | 32 | 12 | 14 | 20 | 20 |
| knight | 95 | 16 | 8 | 6 | 20 | 20 |
| lich | 95 | 7 | 35 | 9 | 0 | 30 |
| light_disciple | 25 | 3 | 15 | 5 | 0 | 30 |
| lorekeeper | 195 | 19 | 20 | 19 | 10 | 10 |
| lorf_of_decay | 160 | 9 | 45 | 11 | 0 | 30 |
| marksman | 65 | 6 | 8 | 16 | 10 | 10 |
| meat_carver | 155 | 21 | 9 | 21 | 10 | 10 |
| melting_elder | 160 | 9 | 45 | 11 | 0 | 30 |
| minstrel | BARD_SHIELD | 15 | 14 | 15 | 10 | 10 |
| necromancer | 50 | 5 | 25 | 7 | 0 | 30 |
| night_blade | BARD_SHIELD | 18 | 8 | 18 | 10 | 10 |
| night_lament | 290 | 30 | 12 | 30 | 10 | 10 |
| night_specter | 155 | 21 | 9 | 21 | 10 | 10 |
| night_terror | 195 | 24 | 10 | 24 | 10 | 10 |
| night_veil | 240 | 27 | 11 | 27 | 10 | 10 |
| overlord | 320 | 36 | 13 | 16 | 20 | 20 |
| paladin | 170 | 24 | 12 | 8 | 20 | 26 |
| plague_spreader | 155 | 9 | 14 | 28 | 10 | 10 |
| poison_bow | 90 | 7 | 10 | 20 | 10 | 10 |
| potions_drank | ? | ? | ? | ? | ? | ? |
| radiant_elder | 125 | 8 | 40 | 10 | 0 | 30 |
| red_archmage | 70 | 6 | 30 | 8 | 0 | 30 |
| red_elder | 95 | 7 | 35 | 9 | 0 | 30 |
| red_mage | 50 | 5 | 25 | 7 | 0 | 30 |
| red_stalker | BARD_SHIELD | 18 | 8 | 18 | 10 | 10 |
| rogue | 30 | 6 | 4 | 6 | 10 | 10 |
| royal_captain | 215 | 28 | 11 | 12 | 20 | 20 |
| royal_guard | 130 | 20 | 9 | 8 | 20 | 20 |
| royal_swordsman | 170 | 24 | 10 | 10 | 20 | 20 |
| scorching_elder | 125 | 8 | 40 | 10 | 0 | 30 |
| scourge | 215 | 28 | 11 | 12 | 20 | 20 |
| shadow_crawler | 65 | 12 | 6 | 12 | 10 | 10 |
| shadow_dancer | 90 | 15 | 7 | 15 | 10 | 10 |
| silver_tongue | 90 | 13 | 11 | 13 | 10 | 10 |
| skeleton | 170 | 50 | 1 | 14 | 50 | 0 |
| spire_acolyte | 155 | 21 | 9 | 21 | 10 | 10 |
| spire_initiate | BARD_SHIELD | 18 | 8 | 18 | 10 | 10 |
| spire_leader | 195 | 24 | 10 | 24 | 10 | 10 |
| spire_sage | 240 | 27 | 11 | 27 | 10 | 10 |
| spirit_engraver | 290 | 30 | 12 | 30 | 10 | 10 |
| spitfang_rider | 155 | 9 | 14 | 28 | 10 | 10 |
| sureshot | 90 | 7 | 10 | 20 | 10 | 10 |
| tempest | 195 | 10 | 16 | 32 | 10 | 10 |
| templar | 215 | 28 | 14 | 9 | 20 | 30 |
| thief | 45 | 9 | 5 | 9 | 10 | 10 |
| titan | 265 | 32 | 16 | 10 | 27 | 27 |
| toxic_stalker | BARD_SHIELD | 8 | 12 | 24 | 10 | 10 |
| trickster | 65 | 11 | 8 | 11 | 10 | 10 |
| tyrant | 265 | 32 | 12 | 14 | 20 | 20 |
| unchained | 170 | 20 | 20 | 20 | 20 | 20 |
| undying_bastion | 320 | 36 | 18 | 11 | 29 | 29 |
| warrior | 65 | 12 | 6 | 5 | 20 | 20 |
| whisper | 290 | 30 | 12 | 30 | 10 | 10 |
| white_archmage | 70 | 6 | 30 | 8 | 0 | 30 |
| white_elder | 95 | 7 | 35 | 9 | 0 | 30 |
| white_mage | 50 | 5 | 25 | 7 | 0 | 30 |
| wolf_rider | 90 | 7 | 10 | 20 | 10 | 10 |
| worg_rider | BARD_SHIELD | 8 | 12 | 24 | 10 | 10 |
| wounds_weaver | 195 | 24 | 10 | 24 | 10 | 10 |
| wraith | 240 | 11 | 18 | 36 | 10 | 10 |
| wyrm_rider | 290 | 12 | 20 | 40 | 10 | 10 |
| zombie | 130 | 34 | 1 | 11 | 50 | 0 |

### 28.2 Dungeons & Enemy Composition

- **barren_wastelands**: 5 enemies — banshee, celestial_destroyer, celestial_lancer, iconoclast, oculus
- **blackwater_port**: 6 enemies — deckhand, mimic, mysterious_tentacle, pirate, pirate_captain, pirate_lieutenant
- **enchanted_forest**: 7 enemies — boar, centaur, ent, forest_spirit, golden_rabbit, treant, wolf
- **eternal_battlefield**: 7 enemies — abomination, death_hound, ghoul, undead, undead_archer, undead_warlord, will_o_wisp
- **frostbite_peaks**: 6 enemies — ice_elemental, snow_wyvern, troll, troll_shaman, troll_warrior, troll_whelp
- **hidden_city_of_larox**: 6 enemies — archmage_of_larox, imp, magic_armor, nexus_researcher, wicked_tribute, wizard_of_larox
- **lost_lands**: 6 enemies — amanita_obscura, berserker, pterodactyl, smoldering_titan, stone_shaman, terrorsaurus
- **obsidian_mines**: 6 enemies — beholder, giant_spider, lost_miner, obsidian_golem, pale_hermit, vampire_bat
- **the_desert**: 7 enemies — djinn, sand_statue, sand_vulture, shahuri_archer, shahuri_mage, shahuri_warrior, wurm
- **the_golden_city**: 7 enemies — arcane_assassin, city_warden, imperial_guard, imperial_mage, insane_citizen, insane_merchant, insane_priest
- **the_southern_grove**: 6 enemies — ancient_ent, dryad, giant_moth, giant_tortoise, green_spitfang, primeval_wurm

### 28.3 Enemy Stats — Tổng Quan

- HP range: 30 – 1000000
- Sample: abomination (HP 1000, dmg 30-35, exp 196), amanita_obscura (HP 450, dmg 150-190, exp 188), ancient_ent (HP 3200, dmg 140-195, exp 240), arcane_assassin (HP 300, dmg 30-60, exp 92), archmage_of_larox (HP 770, dmg 180-195, exp 330), avatar_of_the_ancient (HP 6000, dmg 610-650, exp 10000), banshee (HP 1400, dmg 153-205, exp 250), beholder (HP 1100, dmg 38-46, exp 125), berserker (HP 1880, dmg 390-450, exp 175), bleak_deacon (HP 1700, dmg 200-350, exp 1000)

### 28.4 Items — Phân Bố Theo Loại

| Loại | Số lượng |
|------|----------|
| Item | 188 |
| Accessory | 118 |
| Food | 74 |
| HeavyArmor | 36 |
| Dagger | 28 |
| Sword | 27 |
| MediumArmor | 27 |
| Staff | 27 |
| LightArmor | 26 |
| Bow | 23 |
| Potion | 11 |
| Upgrade | 9 |
| it | 7 |
| Consumable | 6 |

### 28.5 Recipes — 321 công thức (78 có nguyên liệu)

Sample:

- recipe_absolutezero → absolutezero (no ingredients)
- recipe_abyssalcompendium → abyssalcompendium (missingpagex50)
- recipe_abyssalcutlass → abyssalcutlass (no ingredients)
- recipe_abyssalgoo → abyssalgoo (no ingredients)
- recipe_abyssalingot → abyssalingot (no ingredients)
- recipe_aegismechanica → aegismechanica (no ingredients)
- recipe_amuletofresurrection → amuletofresurrection (phoenixfeatherx6)
- recipe_amuletoftheswordsman → amuletoftheswordsman (no ingredients)

### 28.6 Skills — Mẫu

Active: active_annihilate, active_arcane_barrage, active_arcane_diffusion, active_arcane_strike, active_at_the_stake, active_backstab_i, active_backstab_ii, active_backstab_iii, active_barrage_i, active_barrage_ii, active_barrage_iii, active_barrage_iv, active_barrage_v, active_barrage_vi, active_barrage_vii, active_barrage_viii, active_botched_sacrifice, active_bounce, active_choking_powder, active_conde

Passive: passive_ablaze, passive_absurd_genealogy, passive_animated_guardian, passive_behemoth, passive_bend_reality, passive_berserker_rage, passive_bioenhanced, passive_bioenhanced_ii, passive_blind_rage, passive_blinding_i, passive_blinding_ii, passive_blinding_iii, passive_blinding_iv, passive_blinding_v, passive_chaotic, passive_clairvoyance, passive_corrosive_blood, passive_cosmic_projection, passive

### 28.7 Status Effects

abhorrent_curse, ablaze, anointed, bleed, curse, defensive_stance, delirium, exalt, false_life, feeble_tether, frenzy, frozen, greater_curse, inspire, lesser_curse, ominous_curse, petrify, poison, regeneration, silence, skeleton_key, stun, stun_not_cleansable, taunt, terrify

### 28.8 Pets

beetle, crocodile, dove, eagle, floating_eye, floating_seed, golem, holy_tree, lizard, mosquito, owl, rat, red_wolf, rockling, squirrel, tarantula, tentacle_tangle, tesseract, thing_from_the_abyss, tree_frog, walking_bush

### 28.9 Raids

ancient_grave_digging, celestial_mothership, divine_archeology, imperial_rescue, kaunis, sleeping_planet, the_cultist_rebels, the_dire_descent, the_dreadful_ascent, the_lost_expedition, the_slime_pond, the_tower

### 28.10 Quests

active_deterrent, and_stay_dead, annihilator, botched_ritual, clash_of_titans, conqueror, coup_d_etat, critical_hit, crystal_clear, darkness_within, delirious, eldritch_horror, endless_agony, exorcism, expert_duelist, falling_apart, fast_learner, from_hell, god_feared, heavy_armor, hit_or_miss, ice_breaker, innocence, its_a_trap, laroxian_power, light_bringer, long_march, lucky_roll, marathon, master_crafter