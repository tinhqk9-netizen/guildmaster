using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;

namespace GuildMaster.Tests.EditMode
{
    /// <summary>
    /// Guards the data the dungeon loop depends on. Runs before the PlayMode combat test so a
    /// data regression fails here — with a precise reason — instead of surfacing later as a
    /// confusing "no dungeon found".
    ///
    /// The failure this suite was written for: enemy stats and drop tables lived in
    /// StreamingAssets, but the editor provider was reading the converter's staging folder, so
    /// every enemy loaded with 0 HP and no drops.
    /// </summary>
    public class S6_5A_DungeonDataIntegrityTests
    {
        private GameDatabase _db;

        [SetUp]
        public void SetUp()
        {
            _db = new GameDatabase();
            var provider = new EditorExternalGameDataProvider();
            var report = new DatabaseBuilder(provider, new UnityJsonSerializer(), _db).Build();

            Assert.IsFalse(report.hasFatalErrors,
                $"database build failed: {string.Join(" | ", report.errors)}");
        }

        [Test]
        public void Enemies_HaveRealStats()
        {
            var enemies = _db.GetAll<EnemyDefinition>().ToList();
            Assert.IsNotEmpty(enemies, "no EnemyDefinition loaded at all");

            int withHp = enemies.Count(e => e.BaseMaxHp > 0);
            Assert.Greater(withHp, 0,
                $"all {enemies.Count} enemies have BaseMaxHp = 0 — stats did not deserialize " +
                "(check that EnemyDefinition uses public fields, not properties, and that the " +
                "provider is reading StreamingAssets/GameData)");

            EnemyDefinition sample = enemies.First(e => e.BaseMaxHp > 0);
            TestContext.WriteLine($"enemies with BaseMaxHp > 0: {withHp}/{enemies.Count}");
            TestContext.WriteLine($"sample: {sample.id} hp={sample.BaseMaxHp} def={sample.BaseDefense} exp={sample.ExpGiven}");
        }

        [Test]
        public void Enemies_HaveAttackDamageRange()
        {
            var enemies = _db.GetAll<EnemyDefinition>().ToList();
            int withDamage = enemies.Count(e => e.MaxDamage > 0);

            Assert.Greater(withDamage, 0,
                "no enemy has MaxDamage > 0 — rollAttackDamage would always deal 0");

            EnemyDefinition sample = enemies.First(e => e.MaxDamage > 0);
            TestContext.WriteLine($"enemies with MaxDamage > 0: {withDamage}/{enemies.Count}");
            TestContext.WriteLine($"sample: {sample.id} damage {sample.MinDamage}-{sample.MaxDamage}");
        }

        [Test]
        public void Enemies_HaveDropTables()
        {
            var enemies = _db.GetAll<EnemyDefinition>().ToList();
            int withDrops = enemies.Count(e => e.DropTable != null && e.DropTable.Count > 0);

            Assert.Greater(withDrops, 0,
                "no enemy has a drop table — EnemyDropTableLoader did not run, or the JSON " +
                "being read has no Drops object");

            EnemyDefinition sample = enemies.First(e => e.DropTable.Count > 0);
            string entries = string.Join(", ", sample.DropTable.Select(d => $"{d.ItemId} w={d.Weight} x{d.StackCount}"));
            TestContext.WriteLine($"enemies with drop table: {withDrops}/{enemies.Count}");
            TestContext.WriteLine($"sample: {sample.id} -> {entries}");
        }

        [Test]
        public void Dungeons_HaveEnemyLists()
        {
            var dungeons = _db.GetAll<DungeonDefinition>().ToList();
            Assert.IsNotEmpty(dungeons, "no DungeonDefinition loaded at all");

            int withEnemies = dungeons.Count(d => d.EnemyIds != null && d.EnemyIds.Count > 0);
            Assert.Greater(withEnemies, 0,
                "no dungeon lists any enemy — EnemyIds is missing from dungeons.json, or the " +
                "provider is reading a copy that predates the extraction");

            DungeonDefinition sample = dungeons.First(d => d.EnemyIds.Count > 0);
            TestContext.WriteLine($"dungeons with enemy list: {withEnemies}/{dungeons.Count}");
            TestContext.WriteLine($"sample: {sample.id} -> {string.Join(", ", sample.EnemyIds)}");
        }

        [Test]
        public void DungeonEnemyIds_AllResolveToDefinitions()
        {
            var dungeons = _db.GetAll<DungeonDefinition>().ToList();
            var unresolved = new List<string>();

            foreach (DungeonDefinition d in dungeons)
            {
                if (d.EnemyIds == null) continue;
                foreach (string enemyId in d.EnemyIds)
                {
                    EnemyDefinition found;
                    if (!_db.TryGet(enemyId, out found))
                    {
                        unresolved.Add($"{d.id} -> {enemyId}");
                    }
                }
            }

            TestContext.WriteLine($"unresolved dungeon enemy references: {unresolved.Count}");
            Assert.IsEmpty(unresolved,
                "some dungeon enemy ids do not match any EnemyDefinition id: " +
                string.Join(", ", unresolved.Take(10)));
        }

        [Test]
        public void AtLeastOneDungeon_HasAnEnemyWithADropTable()
        {
            var dungeons = _db.GetAll<DungeonDefinition>().ToList();

            DungeonDefinition usable = dungeons.FirstOrDefault(d =>
                d.EnemyIds != null &&
                d.EnemyIds.Any(id =>
                {
                    EnemyDefinition e;
                    return _db.TryGet(id, out e) && e.DropTable != null && e.DropTable.Count > 0;
                }));

            Assert.IsNotNull(usable,
                "no dungeon exposes an enemy with a drop table — the combat/loot PlayMode test " +
                "cannot possibly pass until this is fixed");

            EnemyDefinition looter = usable.EnemyIds
                .Select(id =>
                {
                    EnemyDefinition e;
                    return _db.TryGet(id, out e) ? e : null;
                })
                .First(e => e != null && e.DropTable != null && e.DropTable.Count > 0);

            TestContext.WriteLine($"usable dungeon: {usable.id}, enemy with drops: {looter.id} " +
                                  $"(hp={looter.BaseMaxHp}, drops={looter.DropTable.Count})");
        }

        [Test]
        public void DropTableItemIds_ResolveToItemDefinitions()
        {
            var enemies = _db.GetAll<EnemyDefinition>().ToList();
            var unresolved = new List<string>();
            int checkedEntries = 0;

            foreach (EnemyDefinition e in enemies)
            {
                if (e.DropTable == null) continue;
                foreach (EnemyDropEntry entry in e.DropTable)
                {
                    checkedEntries++;
                    ItemDefinition item;
                    if (!_db.TryGet(entry.ItemId, out item))
                    {
                        unresolved.Add($"{e.id} -> {entry.ItemId}");
                    }
                }
            }

            TestContext.WriteLine($"drop entries checked: {checkedEntries}, unresolved: {unresolved.Count}");
            Assert.IsEmpty(unresolved,
                "drop table references items that do not exist: " + string.Join(", ", unresolved.Take(10)));
        }
    }
}
