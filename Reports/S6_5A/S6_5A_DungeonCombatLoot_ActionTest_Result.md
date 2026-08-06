# S6.5A — Dungeon / Combat / Loot Action Test Result

Run at: 2026-08-05 18:40:08

| Step | Expected | Actual | Result |
|---|---|---|---|
| Bootstrap | ServiceContainer available | available | PASS |
| Adventurer | one adventurer with HP > 0 | bone_hydra hp=390 | PASS |
| Dungeon data | dungeon with enemy list + drop table | enchanted_forest (weakest enemy golden_rabbit, hp=40, drops=3) | PASS |
| Enemy stats | BaseMaxHp > 0 | hp=40 dmg=1-2 exp=120 | PASS |
| Start dungeon | active run | enchanted_forest action=0 | PASS |
| Enemy spawned | at least one enemy in the encounter | yes (hp before 100) | PASS |
| Damage applied | enemy HP decreases | hp 100 -> 1 | PASS |
| Enemy death | at least one enemy reaches 0 HP | yes | PASS |
| Loot in area chest | PendingDrops non-empty | 1 entries / 1 items after 428 ticks | PASS |
| Loot bypass check | chest loot is NOT in storage yet | inventory[wood] = 0 | PASS |
| CollectDrops | chest empties into storage | transferred=1, inventory[wood] 0 -> 1 | PASS |
| Dungeon progress | progress advanced during the run | progress=1 | PASS |
| Save/reload inventory | looted item still present (wood) | 1 | PASS |
| Save/reload progress | progress preserved (1) | 1 | PASS |
| OVERALL | damage + death + chest loot + collect + persist | all verified in 428 ticks | PASS |
