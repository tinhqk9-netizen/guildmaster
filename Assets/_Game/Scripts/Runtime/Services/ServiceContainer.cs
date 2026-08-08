using System;
using GuildMaster.Database;
using GuildMaster.Runtime.Core;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    /// <summary>
    /// Central composition root container holding singletons for all runtime services.
    /// Guarantees that a single shared ISaveService, IFormulaService, and GameDatabase
    /// are injected across the entire game loop.
    /// </summary>
    public class ServiceContainer
    {
        public GameDatabase Database { get; }
        public RuntimeFactory Factory { get; }
        public IFormulaService Formula { get; }
        public ISaveService Save { get; }

        public IItemService Item { get; }
        public IInventoryService Inventory { get; }
        public ICharacterService Character { get; }
        public IEquipmentService Equipment { get; }
        public ISkillService Skill { get; }
        public IStatusEffectService StatusEffect { get; }
        public ICraftService Craft { get; }
        public IMerchantService Merchant { get; }
        public IDungeonService Dungeon { get; }
        public ICombatService Combat { get; }
        public ITargetSelectionService TargetSelection { get; }
        public ILootService Loot { get; }
        public IQuestService Quest { get; }
        public IDoctrineService Doctrine { get; }
        public IPromotionService Promotion { get; }
        public ITraitService Trait { get; }
        public ITavernService Tavern { get; }
        public ISettingsService Settings { get; }
        public IOfflineProgressService OfflineProgress { get; }
        public IGameLoopService GameLoop { get; }
        public IPetService Pet { get; }
        public IBestiaryService Bestiary { get; }
        public IRaidService Raid { get; }
        public IPartyService Party { get; private set; }

        public ServiceContainer(
            GameDatabase database, 
            ISaveService saveService = null, 
            IFormulaService formulaService = null,
            RuntimeFactory factory = null)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formula = formulaService ?? new FormulaService();
            
            if (saveService == null)
            {
                var newSave = new SaveService();
                newSave.Load(out _);
                Save = newSave;
            }
            else
            {
                Save = saveService;
            }

            Factory = factory ?? new RuntimeFactory(new DefaultInstanceIdGenerator());

            Item = new ItemService(Factory, Database);
            Inventory = new InventoryService(Save, Formula, Item, Database);
            Pet = new PetService(Save, Database, Inventory);
            Bestiary = new BestiaryService(Save, Database);
            // Phase 2A: Doctrine constructed before Character so CharacterService/CombatService
            // can read doctrine node bonuses (Threat, Counterattack, Crit, Lifesteal, ...).
            Doctrine = new DoctrineService(Save, Formula, Database);
            Character = new CharacterService(Save, Formula, Database, Factory, Inventory, Pet, Doctrine);
            Equipment = new EquipmentService(Inventory, Save);
            Skill = new SkillService(Database);
            Trait = new TraitService(Database);
            StatusEffect = new StatusEffectService();
            Quest = new QuestService(Save, Database, Doctrine);
            Craft = new CraftService(Database, Inventory, Save, Formula, Quest);
            Merchant = new MerchantService(Database, Inventory, Save, Formula, Quest);
            Combat = new CombatService(Character, Pet);
            TargetSelection = new TargetSelectionService();
            Loot = new LootService();

            Promotion = new PromotionService(Save, Database, Inventory, Character);
            Raid = new RaidService(Save, Database, Combat, Loot, Inventory, Character, Quest, Bestiary);
            Party = new PartyService(Save);
            // Dungeon drives the run loop, so it needs combat, loot, characters, inventory and quest.
            Dungeon = new DungeonService(Save, Database, Combat, Loot, Character, Inventory, Quest,
                bestiaryService: Bestiary, petService: Pet);
            Tavern = new TavernService(Save, Formula, Character, Inventory, Database, Trait);
            Settings = new SettingsService(Save);
            OfflineProgress = new OfflineProgressService(Save, Craft, Merchant, Dungeon);
            GameLoop = new GameLoopService(Save, Tavern, Merchant, Craft, Dungeon, Quest, OfflineProgress);
        }
    }
}
