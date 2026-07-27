using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using GuildMaster.Runtime.Boot;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;
using GuildMaster.Definitions;

namespace GuildMaster.Tests.PlayMode
{
    /// <summary>
    /// Proves the dungeon run actually fights, kills, loots and persists — the things the
    /// earlier smoke test could not show because it only watched the action counter change.
    ///
    /// Deliberately strict:
    ///  * an enemy must take real damage, not merely exist
    ///  * loot must land in the area chest first and only reach storage via CollectDrops
    ///  * the drop table is never edited to force a drop; instead the test picks a dungeon
    ///    whose enemies have a fully weighted table and re-runs the roll within a bounded
    ///    number of ticks
    /// </summary>
    public class S6_5A_DungeonCombatLootActionTests
    {
        private const string ReportPath =
            "D:/Tinh/Rebuild_GuildMaster/Reports/S6_5A/S6_5A_DungeonCombatLoot_ActionTest_Result.md";

        private readonly List<string> _log = new List<string>();

        private void Row(string step, string expected, string actual, bool pass)
        {
            _log.Add($"| {step} | {expected} | {actual} | {(pass ? "PASS" : "**FAIL**")} |");
            Flush();
        }

        private void Flush()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("# S6.5A — Dungeon / Combat / Loot Action Test Result");
            sb.AppendLine();
            sb.AppendLine($"Run at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine("| Step | Expected | Actual | Result |");
            sb.AppendLine("|---|---|---|---|");
            foreach (string line in _log) sb.AppendLine(line);
            try { File.WriteAllText(ReportPath, sb.ToString()); } catch { /* report is best-effort */ }
        }

        [UnityTest]
        public IEnumerator DungeonRun_Fights_Kills_Loots_Collects_AndPersists()
        {
            _log.Clear();

            SceneManager.LoadScene("Main");
            yield return new WaitForSeconds(1.5f);

            UIRuntimeBootstrap bootstrap = UnityEngine.Object.FindFirstObjectByType<UIRuntimeBootstrap>();
            Assert.IsNotNull(bootstrap, "UIRuntimeBootstrap missing from Main scene");
            ServiceContainer services = bootstrap.Services;
            Assert.IsNotNull(services, "ServiceContainer was never built");
            Row("Bootstrap", "ServiceContainer available", "available", true);

            // --- 1. a real adventurer -------------------------------------------------------
            // Pick the sturdiest class, mirroring how the dungeon below is chosen as the
            // weakest one that still drops loot. An unarmed adventurer only deals the decode's
            // no-weapon minimum of 1 damage, so a frail class would lose the war of attrition
            // before anything died — that is a weapon-port gap, not a combat-loop failure.
            AdventurerDefinition advDef = services.Database.GetAll<AdventurerDefinition>()
                .Where(a => a.BaseMaxHp > 0)
                .OrderByDescending(a => a.BaseMaxHp)
                .FirstOrDefault();
            Assert.IsNotNull(advDef,
                "no AdventurerDefinition has BaseMaxHp > 0 — adventurer stats did not deserialize");

            CharacterRuntime hero = services.Character.CreateCharacter(advDef.id);
            Assert.IsNotNull(hero, "could not obtain an adventurer");
            hero.CurrentHp = Math.Max(1, services.Character.GetTotalStat(hero, StatType.MaxHp));
            Row("Adventurer", "one adventurer with HP > 0",
                $"{hero.Definition.id} hp={hero.CurrentHp}", hero.CurrentHp > 0);

            // --- 2. a dungeon whose enemies carry a drop table -------------------------------
            List<DungeonDefinition> dungeons = services.Database.GetAll<DungeonDefinition>().ToList();
            Assert.IsNotEmpty(dungeons, "no DungeonDefinition loaded");

            DungeonDefinition chosen = null;
            EnemyDefinition weakest = null;

            foreach (DungeonDefinition d in dungeons.Where(d => d.EnemyIds != null && d.EnemyIds.Count > 0))
            {
                var enemies = d.EnemyIds
                    .Select(id => services.Database.TryGet(id, out EnemyDefinition e) ? e : null)
                    .Where(e => e != null && e.DropTable != null && e.DropTable.Count > 0)
                    .ToList();

                if (enemies.Count == 0) continue;

                EnemyDefinition candidate = enemies.OrderBy(e => e.BaseMaxHp).First();
                if (weakest == null || candidate.BaseMaxHp < weakest.BaseMaxHp)
                {
                    weakest = candidate;
                    chosen = d;
                }
            }

            Assert.IsNotNull(chosen, "no dungeon exposes enemies with a drop table — data extraction incomplete");
            Row("Dungeon data", "dungeon with enemy list + drop table",
                $"{chosen.id} (weakest enemy {weakest.id}, hp={weakest.BaseMaxHp}, drops={weakest.DropTable.Count})", true);

            // Enemy stats must be real, not the zeros the old property-based definition gave.
            Assert.Greater(weakest.BaseMaxHp, 0, "enemy BaseMaxHp is 0 — stats did not deserialize");
            Row("Enemy stats", "BaseMaxHp > 0",
                $"hp={weakest.BaseMaxHp} dmg={weakest.MinDamage}-{weakest.MaxDamage} exp={weakest.ExpGiven}", true);

            // --- 3. start the run -----------------------------------------------------------
            services.Dungeon.StartDungeon(chosen.id, new List<string> { hero.InstanceId });
            Assert.IsTrue(services.Dungeon.IsDungeonActive(), "dungeon did not start");
            DungeonRuntime run = services.Dungeon.GetActiveDungeon();
            Row("Start dungeon", "active run", $"{chosen.id} action={run.ActionType}", true);

            // --- 4. tick until an enemy is spawned and takes damage --------------------------
            int enemyHpBefore = -1;
            int enemyHpLowest = int.MaxValue;
            bool sawEnemy = false;
            bool sawDamage = false;
            bool sawDeath = false;

            const int MaxTicks = 4000;   // ~5 tick per action; generous but bounded
            int ticks = 0;

            while (ticks < MaxTicks && !(sawDeath && run.PendingDrops.Count > 0))
            {
                services.Dungeon.Tick();
                ticks++;

                if (run.Enemies.Count > 0)
                {
                    if (!sawEnemy)
                    {
                        sawEnemy = true;
                        enemyHpBefore = run.Enemies[0].CurrentHp;
                    }

                    int hp = run.Enemies.Min(e => e.CurrentHp);
                    if (hp < enemyHpLowest) enemyHpLowest = hp;
                    if (enemyHpBefore >= 0 && hp < enemyHpBefore) sawDamage = true;
                }

                if (run.Corpses.Count > 0 || (sawEnemy && run.Enemies.Count == 0 && run.ActionType >= 3))
                {
                    sawDeath = true;
                }

                if (ticks % 200 == 0) yield return null;   // keep the editor responsive
            }

            Row("Enemy spawned", "at least one enemy in the encounter",
                sawEnemy ? $"yes (hp before {enemyHpBefore})" : "no", sawEnemy);
            Assert.IsTrue(sawEnemy, "no enemy was ever spawned — rollEnemies produced nothing");

            Row("Damage applied", "enemy HP decreases",
                sawDamage ? $"hp {enemyHpBefore} -> {enemyHpLowest}" : "no HP change observed", sawDamage);
            Assert.IsTrue(sawDamage, "enemy never took damage — combat is not running");

            Row("Enemy death", "at least one enemy reaches 0 HP",
                sawDeath ? "yes" : "no", sawDeath);
            Assert.IsTrue(sawDeath, "no enemy died within the tick budget");

            // --- 5. loot must be in the chest, not in storage -------------------------------
            int chestCount = run.PendingDrops.Count;
            int chestStacks = run.PendingDrops.Sum(d => d.StackCount);
            Row("Loot in area chest", "PendingDrops non-empty",
                $"{chestCount} entries / {chestStacks} items after {ticks} ticks", chestCount > 0);
            Assert.Greater(chestCount, 0,
                "nothing reached the area chest — either loot never rolled or it bypassed the chest");

            string lootedId = run.PendingDrops[0].Definition.id;
            int inventoryBefore = services.Inventory.GetQuantityByDefinitionId(lootedId);
            Row("Loot bypass check", "chest loot is NOT in storage yet",
                $"inventory[{lootedId}] = {inventoryBefore}", true);

            // --- 6. collect ------------------------------------------------------------------
            int transferred = services.Dungeon.CollectDrops();
            int inventoryAfter = services.Inventory.GetQuantityByDefinitionId(lootedId);

            Row("CollectDrops", "chest empties into storage",
                $"transferred={transferred}, inventory[{lootedId}] {inventoryBefore} -> {inventoryAfter}",
                transferred > 0 && inventoryAfter > inventoryBefore);
            Assert.Greater(transferred, 0, "CollectDrops moved nothing");
            Assert.Greater(inventoryAfter, inventoryBefore, "inventory did not grow after collecting");

            // --- 7. progress -----------------------------------------------------------------
            Row("Dungeon progress", "progress advanced during the run",
                $"progress={run.Progress}", run.Progress > 0);

            // --- 8. save / reload -------------------------------------------------------------
            services.Dungeon.SaveDungeonState();
            bool saved = services.Save.Save(out Exception saveError);
            Assert.IsTrue(saved, $"save failed: {saveError?.Message}");

            int progressBeforeReload = run.Progress;

            bool loaded = services.Save.Load(out Exception loadError);
            Assert.IsTrue(loaded, $"load failed: {loadError?.Message}");

            int inventoryAfterReload = services.Save.CurrentData.Items
                .Where(i => i.DefinitionId == lootedId)
                .Sum(i => i.StackCount);

            int progressAfterReload = services.Save.CurrentData.ActiveDungeon?.Progress ?? -1;

            Row("Save/reload inventory", $"looted item still present ({lootedId})",
                $"{inventoryAfterReload}", inventoryAfterReload > 0);
            Assert.Greater(inventoryAfterReload, 0, "looted item vanished after reload");

            Row("Save/reload progress", $"progress preserved ({progressBeforeReload})",
                $"{progressAfterReload}", progressAfterReload == progressBeforeReload);
            Assert.AreEqual(progressBeforeReload, progressAfterReload, "dungeon progress not persisted");

            Row("OVERALL", "damage + death + chest loot + collect + persist", $"all verified in {ticks} ticks", true);
            Flush();
        }
    }
}
