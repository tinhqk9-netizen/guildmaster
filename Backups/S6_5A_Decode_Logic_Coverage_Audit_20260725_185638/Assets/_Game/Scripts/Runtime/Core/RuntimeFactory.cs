using System;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Core
{
    public class RuntimeFactory
    {
        private readonly IInstanceIdGenerator _idGenerator;

        public RuntimeFactory(IInstanceIdGenerator idGenerator)
        {
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        public ItemRuntime CreateItem(ItemDefinition def, int stackCount = 1)
        {
            return new ItemRuntime(_idGenerator.GenerateInstanceId("ITM"), def, stackCount);
        }

        public CharacterRuntime CreateCharacter(AdventurerDefinition def)
        {
            return new CharacterRuntime(_idGenerator.GenerateInstanceId("CHR"), def);
        }

        public EnemyRuntime CreateEnemy(EnemyDefinition def)
        {
            return new EnemyRuntime(_idGenerator.GenerateInstanceId("ENM"), def);
        }

        public SkillRuntime CreateSkill(SkillDefinition def, int level = 1)
        {
            return new SkillRuntime(def.id);
        }

        public QuestRuntime CreateQuest(QuestDefinition def)
        {
            return new QuestRuntime(_idGenerator.GenerateInstanceId("QST"), def);
        }

        public DungeonRuntime CreateDungeon(DungeonDefinition def)
        {
            return new DungeonRuntime(_idGenerator.GenerateInstanceId("DNG"), def);
        }
    }
}
