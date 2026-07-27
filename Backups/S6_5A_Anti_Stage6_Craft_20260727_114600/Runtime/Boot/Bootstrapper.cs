using System;
using UnityEngine;
using GuildMaster.Database;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Core;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Boot
{
    public class Bootstrapper : MonoBehaviour
    {
        public bool RuntimeReady { get; private set; }
        
        // Composition Root Instances
        public ServiceContainer Services { get; private set; }
        public GameDatabase Database { get; private set; }
        public RuntimeFactory Factory { get; private set; }
        public IFormulaService FormulaService { get; private set; }
        public ISaveService SaveService { get; private set; }

        private void Start()
        {
            InitializePipeline();
        }

        private void InitializePipeline()
        {
            try
            {
                Debug.Log("[Bootstrapper] Starting Pipeline...");

                // 1. Provider
                IGameDataProvider dataProvider;
#if UNITY_EDITOR
                dataProvider = new EditorExternalGameDataProvider();
#else
                dataProvider = new StreamingAssetsGameDataProvider();
#endif

                // 2. Serializer
                var serializer = new UnityJsonSerializer();

                // 3. Database
                Database = new GameDatabase();

                // 4. Definition Registry (Build Database)
                var builder = new DatabaseBuilder(dataProvider, serializer, Database);
                var report = builder.Build();

                if (report.hasFatalErrors)
                {
                    throw new Exception("Database build encountered fatal errors.");
                }

                // 5. ServiceContainer (Composition Root)
                Services = new ServiceContainer(Database);
                FormulaService = Services.Formula;
                SaveService = Services.Save;
                Factory = Services.Factory;

                // 6. Runtime Ready
                RuntimeReady = true;
                Debug.Log("[Bootstrapper] Runtime Ready with ServiceContainer. Waiting for Scene transition...");
            }
            catch (Exception ex)
            {
                // If Fatal
                RuntimeReady = false;
                Debug.LogError($"[Bootstrapper] FATAL BOOT ERROR: {ex.Message}\n{ex.StackTrace}");
                // Giữ nguyên trạng thái Boot, không chuyển sang Main
            }
        }
    }
}
