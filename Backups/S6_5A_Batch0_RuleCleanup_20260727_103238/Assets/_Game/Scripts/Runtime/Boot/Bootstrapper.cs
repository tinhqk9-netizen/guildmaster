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

                // 5. Formula Service (must be initialized before SaveService if dependencies exist)
                FormulaService = new FormulaService();

                // 6. Save Service
                SaveService = new SaveService();
                if (!SaveService.Load(out Exception saveError))
                {
                    Debug.LogWarning($"[Bootstrapper] Save Load fallback triggered. Error: {saveError?.Message}");
                }

                // 7. Runtime Factory (Shared DI)
                var idGenerator = new DefaultInstanceIdGenerator();
                Factory = new RuntimeFactory(idGenerator);

                // 8. Runtime Ready
                RuntimeReady = true;
                Debug.Log("[Bootstrapper] Runtime Ready. Waiting for Scene transition...");
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
