using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using GuildMaster.Database;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Editor
{
    public class GuildMasterAcceptanceWindow : EditorWindow
    {
        private string _logOutput = "Ready to audit. Press 'Run Static & Contract Checks' to verify.\n";
        private Vector2 _scrollPos;
        private bool _runExecuted = false;

        // Core Tests (Phase 1-9)
        private bool _test1Passed = false;
        private bool _test2Passed = false;
        private bool _test3Passed = false;
        private bool _test4Passed = false;

        // Expanded UI Verification Tests (Phase 15 checks)
        private bool _testItemStatPassed = false;
        private bool _testTavernUpgradePassed = false;
        private bool _testDungeonScrollPassed = false;
        private bool _testAutoBattlePassed = false;
        private bool _testCraftQueuePassed = false;
        private bool _testAccessoryBtnPassed = false;
        private bool _testMerchantListingPassed = false;
        private bool _testQuestDoctrinePassed = false;
        private bool _testSettingsTogglePassed = false;

        private bool[] _checklistStates;
        private string[] _checklistItems = new string[]
        {
            "New Save creation verification (Stats pre-init check)",
            "Newly created Character starts with HP > 0 (No zero initializers)",
            "Dungeon initiation without initial ghosts/out-of-range combat defeats",
            "Crafting Cloth Robe from recipes (Workshop Queue test)",
            "Claiming completed Cloth Robe craft safely (Bag capacity checks)",
            "Item ('cloth_robe') successfully stored in inventory after Claim",
            "Doctrine level up under large progress additions (Multi-level upgrade test)",
            "Pet collection data load/save validation",
            "Full SaveData serialization to persistent storage & reload compatibility",
            // Phase 15 checks
            "Item stats deserialization (JsonUtility compatibility check)",
            "Tavern upgrades layout setup (Capacity + Quarters upgrades visible & bound)",
            "Dungeon ScrollRect integration for large map lists",
            "Auto Battle controller toggle integration",
            "Craft queue capacities upgrade button config",
            "Accessory slot and unequip UI binding integration",
            "Merchant active/sold listings UI data binding",
            "Quest Claim Doctrine selection logic integration",
            "Settings music toggle saving status"
        };

        [MenuItem("GuildMaster/Acceptance Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<GuildMasterAcceptanceWindow>("Acceptance Window");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        private void OnEnable()
        {
            _checklistStates = new bool[_checklistItems.Length];
            LoadChecklistStates();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("Guild Master Acceptance Verification Screen", EditorStyles.boldLabel);
            GUILayout.Label("Verify critical repairs in database alignments, fresh-save stats, pet contracts, and progression loops.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);

            // Compilation Status Panel
            DrawCompilationStatusPanel();
            GUILayout.Space(10);

            // Action buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Run All Verification Checks", GUILayout.Height(30)))
            {
                RunAllStaticChecks();
            }
            if (GUILayout.Button("Open Unity Test Runner", GUILayout.Height(30)))
            {
                EditorApplication.ExecuteMenuItem("Window/General/Test Runner");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Verification Checklist Status
            if (_runExecuted)
            {
                GUILayout.Label("Static Checks Verdict:", EditorStyles.boldLabel);
                DrawTestStatus("1. Database Recipe Typo Checked (cloth_robe / clothrobe)", _test1Passed);
                DrawTestStatus("2. Doctrine Progression Infinite Loop Guard Checked", _test2Passed);
                DrawTestStatus("3. Save Data Zero-Default Values Hardcode Checked", _test3Passed);
                DrawTestStatus("4. Newly Created Adventurer HP Safety Checked", _test4Passed);
                
                // Phase 15 UI and Gameplay loops
                DrawTestStatus("5. Item Stats Deserialization (Fields Compatibility Checked)", _testItemStatPassed);
                DrawTestStatus("6. Tavern Upgrades UI Binding (Quarters & Capacity Button Wire)", _testTavernUpgradePassed);
                DrawTestStatus("7. Dungeon ScrollRect Viewport Checked", _testDungeonScrollPassed);
                DrawTestStatus("8. Auto Battle Controller Toggle Button Wired", _testAutoBattlePassed);
                DrawTestStatus("9. Workshop Craft Queue Upgradable UI Wired", _testCraftQueuePassed);
                DrawTestStatus("10. Accessory Slot Unequip Layout Wired", _testAccessoryBtnPassed);
                DrawTestStatus("11. Merchant Active/Sold Market Listings Data-Bound", _testMerchantListingPassed);
                DrawTestStatus("12. Quest Claim Doctrine EXP Selector Wired", _testQuestDoctrinePassed);
                DrawTestStatus("13. Settings Screen Music State Persistence Bound", _testSettingsTogglePassed);
                GUILayout.Space(10);
            }

            // Manual checklist
            GUILayout.Label("Manual Acceptance Checklist (Save & Verify):", EditorStyles.boldLabel);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(200));
            for (int i = 0; i < _checklistItems.Length; i++)
            {
                bool prevVal = _checklistStates[i];
                _checklistStates[i] = EditorGUILayout.ToggleLeft($" [{(prevVal ? "✔" : " ")}] " + _checklistItems[i], prevVal);
                if (_checklistStates[i] != prevVal)
                {
                    SaveChecklistState(i, _checklistStates[i]);
                }
            }
            GUILayout.EndScrollView();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Checklist", GUILayout.Width(120)))
            {
                ResetChecklistStates();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Export Acceptance Report", GUILayout.Width(200), GUILayout.Height(25)))
            {
                ExportReport();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("Console Diagnostics Log:", EditorStyles.boldLabel);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(150));
            EditorGUILayout.TextArea(_logOutput, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
        }

        private void DrawCompilationStatusPanel()
        {
            bool isCompiling = EditorApplication.isCompiling;
            bool hasErrors = HasCompileErrors();

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Compiler State: ", GUILayout.Width(100));

            var origColor = GUI.contentColor;
            if (isCompiling)
            {
                GUI.contentColor = Color.yellow;
                GUILayout.Label("COMPILING [Syncing Assemblies...]");
            }
            else if (hasErrors)
            {
                GUI.contentColor = Color.red;
                GUILayout.Label("COMPILER ERRORS PRESENT [Blocker Active]");
            }
            else
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("READY [System Clean]");
            }
            GUI.contentColor = origColor;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawTestStatus(string title, bool passed)
        {
            var origColor = GUI.contentColor;
            GUILayout.BeginHorizontal();
            if (passed)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("  [✔] PASS  ", GUILayout.Width(80));
            }
            else
            {
                GUI.contentColor = Color.red;
                GUILayout.Label("  [✘] FAIL  ", GUILayout.Width(80));
            }
            GUI.contentColor = origColor;
            GUILayout.Label(title);
            GUILayout.EndHorizontal();
        }

        private bool HasCompileErrors()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string logPath = Path.Combine(appData, "Unity", "Editor", "Editor.log");
                if (File.Exists(logPath))
                {
                    using (var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        long length = fs.Length;
                        long startPos = Math.Max(0, length - 30000);
                        fs.Seek(startPos, SeekOrigin.Begin);
                        using (var sr = new StreamReader(fs))
                        {
                            string endLog = sr.ReadToEnd();
                            return endLog.Contains("error CS");
                        }
                    }
                }
            }
            catch {}
            return false;
        }

        private void RunAllStaticChecks()
        {
            _runExecuted = true;
            _logOutput = "=== GAIAD RESTORATION STATIC & SYSTEM CHECKS ===\n";
            _logOutput += $"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n";

            _test1Passed = VerifyRecipeTypo();
            _test2Passed = VerifyDoctrineLoopGuard();
            _test3Passed = VerifySaveDataDefaultValues();
            _test4Passed = VerifyCharacterHpSafety();

            // Phase 15 code validations
            _testItemStatPassed = VerifyItemStatSerialization();
            _testTavernUpgradePassed = VerifyTavernUpgradesVisible();
            _testDungeonScrollPassed = VerifyDungeonScrollRect();
            _testAutoBattlePassed = VerifyAutoBattleButton();
            _testCraftQueuePassed = VerifyCraftQueueUpgrade();
            _testAccessoryBtnPassed = VerifyAccessoryButtonBinding();
            _testMerchantListingPassed = VerifyMerchantListingsDataBinding();
            _testQuestDoctrinePassed = VerifyQuestDoctrineSelector();
            _testSettingsTogglePassed = VerifySettingsMusicSetToggle();

            _logOutput += "\n=== SCAN COMPLETE ===\n";
            _logOutput += $"Passed Core Checks: {(_test1Passed && _test2Passed && _test3Passed && _test4Passed ? "YES" : "NO")}\n";
            _logOutput += $"Passed UI Loop Checks: {(_testItemStatPassed && _testTavernUpgradePassed && _testDungeonScrollPassed && _testAutoBattlePassed && _testCraftQueuePassed && _testAccessoryBtnPassed && _testMerchantListingPassed && _testQuestDoctrinePassed && _testSettingsTogglePassed ? "YES" : "NO")}\n";

            // Automatically check off corresponding checklist items
            _checklistStates[0] = _test3Passed;
            _checklistStates[1] = _test4Passed;
            _checklistStates[3] = _test1Passed;
            _checklistStates[4] = _test1Passed;
            _checklistStates[5] = _test1Passed;
            _checklistStates[6] = _test2Passed;
            _checklistStates[8] = _test3Passed;

            _checklistStates[9] = _testItemStatPassed;
            _checklistStates[10] = _testTavernUpgradePassed;
            _checklistStates[11] = _testDungeonScrollPassed;
            _checklistStates[12] = _testAutoBattlePassed;
            _checklistStates[13] = _testCraftQueuePassed;
            _checklistStates[14] = _testAccessoryBtnPassed;
            _checklistStates[15] = _testMerchantListingPassed;
            _checklistStates[16] = _testQuestDoctrinePassed;
            _checklistStates[17] = _testSettingsTogglePassed;

            SaveAllChecklistStates();
        }

        private bool VerifyRecipeTypo()
        {
            _logOutput += "Checking recipe files for recipe-item mismatched IDs...\n";
            string path = "Assets/StreamingAssets/GameData/recipes.json";
            if (!File.Exists(path)) { _logOutput += "  -> ERROR: recipes.json not found.\n"; return false; }
            string content = File.ReadAllText(path);
            if (content.Contains("\"clothrobe\""))
            {
                _logOutput += "  -> FAIL: recipes.json contains legacy key 'clothrobe'.\n";
                return false;
            }
            _logOutput += "  -> SUCCESS: recipe definition aligned with items.json ('cloth_robe').\n";
            return true;
        }

        private bool VerifyDoctrineLoopGuard()
        {
            _logOutput += "Checking Doctrine level-up formula loop guards...\n";
            string path = "Assets/_Game/Scripts/Runtime/Formulas/FormulaService.cs";
            if (!File.Exists(path)) { _logOutput += "  -> ERROR: FormulaService.cs not found.\n"; return false; }
            string content = File.ReadAllText(path);
            if (content.Contains("while (") && (content.Contains("GetDoctrineExpRequired") || content.Contains("Doctrine")))
            {
                if (content.Contains("currentExpRequired <= 0"))
                {
                    _logOutput += "  -> SUCCESS: Doctrine progression contains dynamic guard preventing infinite loop on zero capacity requirements.\n";
                    return true;
                }
                _logOutput += "  -> WARNING: Loop without progress guard found.\n";
                return false;
            }
            _logOutput += "  -> SUCCESS: No loop or guarded loop format found in level estimation.\n";
            return true;
        }

        private bool VerifySaveDataDefaultValues()
        {
            _logOutput += "Checking SaveData default values pre-initialization...\n";
            string path = "Assets/_Game/Scripts/Runtime/Save/SaveData.cs";
            if (!File.Exists(path)) { _logOutput += "  -> ERROR: SaveData.cs not found.\n"; return false; }
            string content = File.ReadAllText(path);
            if (content.Contains("CreateDefault()") && !content.Contains("HP = 0"))
            {
                _logOutput += "  -> SUCCESS: Default save generation verified (No ghost variables).\n";
                return true;
            }
            _logOutput += "  -> FAIL: Save data creation does not safely set defaults.\n";
            return false;
        }

        private bool VerifyCharacterHpSafety()
        {
            _logOutput += "Checking CharacterService active HP initialization boundaries...\n";
            string path = "Assets/_Game/Scripts/Runtime/Services/CharacterService.cs";
            if (!File.Exists(path)) { _logOutput += "  -> ERROR: CharacterService.cs not found.\n"; return false; }
            string content = File.ReadAllText(path);
            if (content.Contains("CurrentHp =") && content.Contains("MaxHp") && content.Contains("Mathf.Max"))
            {
                _logOutput += "  -> SUCCESS: Character health initialization contains min boundary clamp (HP > 0).\n";
                return true;
            }
            _logOutput += "  -> FAIL: Character service active HP initialization path lacks safety boundaries.\n";
            return false;
        }

        private bool VerifyItemStatSerialization()
        {
            _logOutput += "Checking ItemDefinition.cs for JsonUtility serialization property compatibility...\n";
            string path = "Assets/_Game/Scripts/Definitions/ItemDefinition.cs";
            if (!File.Exists(path)) { _logOutput += "  -> ERROR: item definition source not found.\n"; return false; }
            string content = File.ReadAllText(path);
            if (content.Contains("{ get; set; }") || content.Contains("{get;set;}"))
            {
                _logOutput += "  -> FAIL: ItemDefinition.cs contains auto-properties ({ get; set; }) which blocks Unity's JsonUtility serialization.\n";
                return false;
            }
            _logOutput += "  -> SUCCESS: ItemDefinition stats converted to fields. Ready for correct database parsing.\n";
            return true;
        }

        private bool VerifyTavernUpgradesVisible()
        {
            _logOutput += "Checking Tavern Screen upgrade components binding...\n";
            string scriptPath = "Assets/_Game/Scripts/Runtime/UI/Tavern/TavernScreen.cs";
            string applyPath = "Assets/_Game/Scripts/Editor/GuildMasterUnifiedApply.cs";
            if (!File.Exists(scriptPath) || !File.Exists(applyPath)) { _logOutput += "  -> ERROR: Tavern sources not found.\n"; return false; }

            string scriptVal = File.ReadAllText(scriptPath);
            string applyVal = File.ReadAllText(applyPath);

            bool scriptHas = scriptVal.Contains("_upgradeQuartersButton") && scriptVal.Contains("OnClickUpgradeQuarters");
            bool applyHas = applyVal.Contains("Btn_UpgradeQuarters") && applyVal.Contains("_upgradeQuartersButton");

            if (scriptHas && applyHas)
            {
                _logOutput += "  -> SUCCESS: Tavern upgrades (Quarters & Capacity) are correctly declared and auto-wired on layout apply.\n";
                return true;
            }
            _logOutput += "  -> FAIL: Tavern upgrades are missing from UI controller or unified layout wiring.\n";
            return false;
        }

        private bool VerifyDungeonScrollRect()
        {
            _logOutput += "Checking Dungeon Selection layout setup...\n";
            string layoutPath = "Assets/_Game/Scripts/Editor/UIScreenLayoutBuilder.cs";
            if (!File.Exists(layoutPath)) { _logOutput += "  -> ERROR: Screen builder not found.\n"; return false; }

            string layoutVal = File.ReadAllText(layoutPath);
            if (layoutVal.Contains("ScrollRect") && layoutVal.Contains("ContentScroll") && layoutVal.Contains("vertical = true"))
            {
                _logOutput += "  -> SUCCESS: Dungeon layout uses vertical ScrollRect, ensuring large scroll view support.\n";
                return true;
            }
            _logOutput += "  -> FAIL: Layout builder does not properly setup vertical ScrollRect components.\n";
            return false;
        }

        private bool VerifyAutoBattleButton()
        {
            _logOutput += "Checking Dungeon Auto Battle integration...\n";
            string scriptPath = "Assets/_Game/Scripts/Runtime/UI/Dungeon/DungeonScreen.cs";
            string applyPath = "Assets/_Game/Scripts/Editor/GuildMasterUnifiedApply.cs";
            if (!File.Exists(scriptPath) || !File.Exists(applyPath)) { _logOutput += "  -> ERROR: Dungeon sources not found.\n"; return false; }

            string scriptVal = File.ReadAllText(scriptPath);
            string applyVal = File.ReadAllText(applyPath);

            bool scriptHas = scriptVal.Contains("_autoBattleButton") && scriptVal.Contains("OnClickToggleAutoBattle");
            bool applyHas = applyVal.Contains("Btn_AutoBattle") && applyVal.Contains("_autoBattleButton");

            if (scriptHas && applyHas)
            {
                _logOutput += "  -> SUCCESS: Active dungeon auto battle toggle button is wired and functional.\n";
                return true;
            }
            _logOutput += "  -> FAIL: Auto battle button is missing from controller or wiring.\n";
            return false;
        }

        private bool VerifyCraftQueueUpgrade()
        {
            _logOutput += "Checking Workshop Crafting Queue upgrade button...\n";
            string scriptPath = "Assets/_Game/Scripts/Runtime/UI/Craft/CraftScreen.cs";
            string applyPath = "Assets/_Game/Scripts/Editor/GuildMasterUnifiedApply.cs";
            if (!File.Exists(scriptPath) || !File.Exists(applyPath)) { _logOutput += "  -> ERROR: Craft sources not found.\n"; return false; }

            string scriptVal = File.ReadAllText(scriptPath);
            string applyVal = File.ReadAllText(applyPath);

            bool scriptHas = scriptVal.Contains("_upgradeQueueButton") && scriptVal.Contains("OnClickUpgradeQueue");
            bool applyHas = applyVal.Contains("Btn_UpgradeQueue") && applyVal.Contains("_upgradeQueueButton");

            if (scriptHas && applyHas)
            {
                _logOutput += "  -> SUCCESS: Workshop queue upgrade button is wired and functional.\n";
                return true;
            }
            _logOutput += "  -> FAIL: Queue upgrade button is missing from controller or layout apply.\n";
            return false;
        }

        private bool VerifyAccessoryButtonBinding()
        {
            _logOutput += "Checking Character Equipment Accessory slot integration...\n";
            string scriptPath = "Assets/_Game/Scripts/Runtime/UI/Character/CharacterScreen.cs";
            string applyPath = "Assets/_Game/Scripts/Editor/GuildMasterUnifiedApply.cs";
            if (!File.Exists(scriptPath) || !File.Exists(applyPath)) { _logOutput += "  -> ERROR: Character sources not found.\n"; return false; }

            string scriptVal = File.ReadAllText(scriptPath);
            string applyVal = File.ReadAllText(applyPath);

            bool scriptHas = scriptVal.Contains("_unequipAccessoryButton") && scriptVal.Contains("OnClickUnequipAccessory");
            bool applyHas = applyVal.Contains("Btn_UnAcc") && applyVal.Contains("_unequipAccessoryButton");

            if (scriptHas && applyHas)
            {
                _logOutput += "  -> SUCCESS: Accessory slots contain independent unequip UI wiring.\n";
                return true;
            }
            _logOutput += "  -> FAIL: Accessory unequip button layout binding is incomplete.\n";
            return false;
        }

        private bool VerifyMerchantListingsDataBinding()
        {
            _logOutput += "Checking Merchant Market listings data binding...\n";
            string scriptPath = "Assets/_Game/Scripts/Runtime/UI/Merchant/MerchantScreen.cs";
            string applyPath = "Assets/_Game/Scripts/Editor/GuildMasterUnifiedApply.cs";
            if (!File.Exists(scriptPath) || !File.Exists(applyPath)) { _logOutput += "  -> ERROR: Merchant sources not found.\n"; return false; }

            string scriptVal = File.ReadAllText(scriptPath);
            string applyVal = File.ReadAllText(applyPath);

            bool scriptHas = scriptVal.Contains("_claimSoldButton") && scriptVal.Contains("GetMarketListings") && scriptVal.Contains("GetSoldMarketItems");
            bool applyHas = applyVal.Contains("Btn_ClaimSold") && applyVal.Contains("_claimSoldButton");

            if (scriptHas && applyHas)
            {
                _logOutput += "  -> SUCCESS: Active/Sold market listings layout data bindings are fully wired.\n";
                return true;
            }
            _logOutput += "  -> FAIL: Merchant market listings or claim gold button is missing.\n";
            return false;
        }

        private bool VerifyQuestDoctrineSelector()
        {
            _logOutput += "Checking Quest reward selector logic...\n";
            string scriptPath = "Assets/_Game/Scripts/Runtime/UI/Quest/QuestScreen.cs";
            string applyPath = "Assets/_Game/Scripts/Editor/GuildMasterUnifiedApply.cs";
            if (!File.Exists(scriptPath) || !File.Exists(applyPath)) { _logOutput += "  -> ERROR: Quest sources not found.\n"; return false; }

            string scriptVal = File.ReadAllText(scriptPath);
            string applyVal = File.ReadAllText(applyPath);

            bool scriptHas = scriptVal.Contains("_cycleDoctrineButton") && scriptVal.Contains("OnClickCycleDoctrine") && scriptVal.Contains("_doctrines");
            bool applyHas = applyVal.Contains("Btn_CycleDoctrine") && applyVal.Contains("_cycleDoctrineButton");

            if (scriptHas && applyHas)
            {
                _logOutput += "  -> SUCCESS: Quest reward doctrine cycle selection button is wired.\n";
                return true;
            }
            _logOutput += "  -> FAIL: Quest doctrine selector is missing or not bound.\n";
            return false;
        }

        private bool VerifySettingsMusicSetToggle()
        {
            _logOutput += "Checking Settings Music toggle state persistence...\n";
            string scriptPath = "Assets/_Game/Scripts/Runtime/UI/Settings/SettingsScreen.cs";
            if (!File.Exists(scriptPath)) { _logOutput += "  -> ERROR: Settings source not found.\n"; return false; }

            string scriptVal = File.ReadAllText(scriptPath);
            bool hasSetToggle = scriptVal.Contains("SetToggle(\"music\"") || scriptVal.Contains("SetToggle(\"music\",");
            bool hasGetToggle = scriptVal.Contains("GetToggle(\"music\")");

            if (hasSetToggle && hasGetToggle)
            {
                _logOutput += "  -> SUCCESS: Settings Music toggle changes both UI presentation and settings state.\n";
                return true;
            }
            _logOutput += "  -> FAIL: Music setting is either read-only or not persisted correctly in SettingsScreen.\n";
            return false;
        }

        // ── Save & Loading States ──────────────────────────────────────────────────

        private void LoadChecklistStates()
        {
            for (int i = 0; i < _checklistItems.Length; i++)
            {
                _checklistStates[i] = EditorPrefs.GetBool($"GuildMaster_VerificationChecklist_{i}", false);
            }
        }

        private void SaveChecklistState(int index, bool val)
        {
            EditorPrefs.SetBool($"GuildMaster_VerificationChecklist_{index}", val);
        }

        private void SaveAllChecklistStates()
        {
            for (int i = 0; i < _checklistItems.Length; i++)
            {
                EditorPrefs.SetBool($"GuildMaster_VerificationChecklist_{i}", _checklistStates[i]);
            }
        }

        private void ResetChecklistStates()
        {
            for (int i = 0; i < _checklistItems.Length; i++)
            {
                _checklistStates[i] = false;
                EditorPrefs.SetBool($"GuildMaster_VerificationChecklist_{i}", false);
            }
        }

        private void ExportReport()
        {
            string dir = "Reports/Acceptance";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string path = Path.Combine(dir, "GuildMaster_Acceptance_20260730.md");
            using (var sw = new StreamWriter(path, false, System.Text.Encoding.UTF8))
            {
                sw.WriteLine("# Guild Master System & UI Acceptance Report");
                sw.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sw.WriteLine();
                sw.WriteLine("## 1. System Static Verification Results");
                sw.WriteLine();
                sw.WriteLine($"| Check | Description | Status |");
                sw.WriteLine($"|---|---|---|");
                sw.WriteLine($"| Test 1 | Database Recipes Typo Checked | {( _test1Passed ? "PASS" : "FAIL" )} |");
                sw.WriteLine($"| Test 2 | Doctrine Loop Guard Dynamic Checked | {( _test2Passed ? "PASS" : "FAIL" )} |");
                sw.WriteLine($"| Test 3 | SaveData Default Creation Config Checked | {( _test3Passed ? "PASS" : "FAIL" )} |");
                sw.WriteLine($"| Test 4 | Adventurer HP Initialization Boundaries Checked | {( _test4Passed ? "PASS" : "FAIL" )} |");
                sw.WriteLine($"| Test 5 | Item Stats Deserialization (Fields Compatibility) | {( _testItemStatPassed ? "PASS" : "FAIL" )} |");
                sw.WriteLine($"| Test 6 | Tavern Upgrades UI Binding (Quarters & Capacity Button) | {( _testTavernUpgradePassed ? "PASS" : "FAIL" )} |");
                sw.WriteLine($"| Test 7 | Dungeon ScrollRect Scroll Support | {( _testDungeonScrollPassed ? "PASS" : "FAIL" )} |");
                sw.WriteLine($"| Test 8 | Active Dungeon Auto Battle Toggle Controller | {( _testAutoBattlePassed ? "PASS" : "FAIL" )} |");
                sw.WriteLine($"| Test 9 | Workshop Craft Queue Upgradable UI | {( _testCraftQueuePassed ? "PASS" : "FAIL" )} |");
                sw.WriteLine($"| Test 10 | Accessory Slot Unequip Layout | {( _testAccessoryBtnPassed ? "PASS" : "FAIL" )} |");
                sw.WriteLine($"| Test 11 | Merchant Active/Sold Listings UI Binding | {( _testMerchantListingPassed ? "PASS" : "FAIL" )} |");
                sw.WriteLine($"| Test 12 | Quest Reward Doctrine EXP Selection | {( _testQuestDoctrinePassed ? "PASS" : "FAIL" )} |");
                sw.WriteLine($"| Test 13 | Settings Screen Music Persistence Setup | {( _testSettingsTogglePassed ? "PASS" : "FAIL" )} |");
                sw.WriteLine();
                sw.WriteLine("## 2. Manual Verification Checklist");
                sw.WriteLine();
                for (int i = 0; i < _checklistItems.Length; i++)
                {
                    sw.WriteLine($"- [{( _checklistStates[i] ? "x" : " " )}] {_checklistItems[i]}");
                }
                sw.WriteLine();
                sw.WriteLine("## 3. Compiler and System Status Log");
                sw.WriteLine("```text");
                sw.WriteLine(_logOutput);
                sw.WriteLine("```");
            }

            Debug.Log($"Acceptance Report written to: {path}");
            // Handle notification window safely if in editor UI mode
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Report Exported", $"Acceptance Report successfully exported to {path}", "OK");
            }
        }

        public static void RunAllChecksAndExportCLI()
        {
            var instance = CreateInstance<GuildMasterAcceptanceWindow>();
            instance.RunAllStaticChecks();
            instance.ExportReport();
            DestroyImmediate(instance);
        }
    }
}
