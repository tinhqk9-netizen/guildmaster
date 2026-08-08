#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using GuildMaster.Runtime.Boot;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Character;

namespace GuildMaster.Tools.Phase1Verification
{
    /// <summary>
    /// Unity Editor Tool for manual testing of Phase 1 Character Progression:
    /// - Hero Class System
    /// - Promotion System
    /// - Common / Rare Traits
    /// - Skill Mapping
    /// - Character Detail UI
    /// </summary>
    public class Phase1VerificationWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private string _statusMessage = "Ready. Click buttons below to spawn/test Phase 1 Character Progression.";

        [MenuItem("Tools/GuildMaster/Phase 1 Character Verification Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<Phase1VerificationWindow>("Phase 1 Verification");
            window.minSize = new Vector2(420, 520);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("GuildMaster — Phase 1 Dev Verification Tool", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This Editor Tool allows testing Phase 1 Character Progression features (Class System, Promotion, Common/Rare Traits, Skill Mapping, Character Detail UI) in Unity Editor without altering real save data permanently.", MessageType.Info);
            EditorGUILayout.Space(10);

            bool isPlaying = Application.isPlaying;
            if (!isPlaying)
            {
                EditorGUILayout.HelpBox("Please enter Play Mode in Unity Editor to interact with live CharacterService & Character Detail UI.", MessageType.Warning);
            }

            EditorGUI.BeginDisabledGroup(!isPlaying);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Button 1: Spawn Basic Hero
            GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
            if (GUILayout.Button("1. [Spawn Basic Hero]", GUILayout.Height(36)))
            {
                var saveData = Phase1VerificationHelper.SpawnBasicHero();
                if (saveData != null)
                {
                    _statusMessage = $"[SUCCESS] Spawned Basic Hero: {saveData.DefinitionId} (ID: {saveData.InstanceId})";
                }
            }

            EditorGUILayout.Space(6);

            // Button 2: Spawn Double Trait Hero
            GUI.backgroundColor = new Color(0.8f, 1f, 0.8f);
            if (GUILayout.Button("2. [Spawn Double Trait Hero]", GUILayout.Height(36)))
            {
                var saveData = Phase1VerificationHelper.SpawnDoubleTraitHero();
                if (saveData != null)
                {
                    _statusMessage = $"[SUCCESS] Spawned Double Trait Hero: {saveData.DefinitionId} with Common='{saveData.TraitCommon}', Rare='{saveData.TraitRare}'";
                }
            }

            EditorGUILayout.Space(6);

            // Button 3: Spawn Promotion Test Hero
            GUI.backgroundColor = new Color(1f, 0.9f, 0.6f);
            if (GUILayout.Button("3. [Spawn Promotion Test Hero]", GUILayout.Height(36)))
            {
                var saveData = Phase1VerificationHelper.SpawnPromotionTestHero();
                if (saveData != null)
                {
                    _statusMessage = $"[SUCCESS] Spawned Promotion Test Hero: Apprentice Lv.20. Opened Character Detail UI to verify promotion.";
                }
            }

            EditorGUILayout.Space(6);

            // Button 4: Spawn Showcase Hero
            GUI.backgroundColor = new Color(1f, 0.8f, 0.9f);
            if (GUILayout.Button("4. [Spawn Showcase Hero]", GUILayout.Height(36)))
            {
                var saveData = Phase1VerificationHelper.SpawnShowcaseHero();
                if (saveData != null)
                {
                    _statusMessage = $"[SUCCESS] Spawned Showcase Hero: Mage Lv.15 with Equipment & Traits. Inspected in Character Detail UI.";
                }
            }

            EditorGUILayout.Space(16);
            GUI.backgroundColor = Color.white;

            // Button 5: Clear Test Data
            GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
            if (GUILayout.Button("5. [Clear Test Data]", GUILayout.Height(36)))
            {
                int count = Phase1VerificationHelper.ClearTestData();
                _statusMessage = $"[SUCCESS] Cleared {count} test heroes (prefix '{Phase1VerificationHelper.TestIdPrefix}'). Normal save data untouched.";
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(16);
            EditorGUILayout.LabelField("Status / Diagnostics:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(_statusMessage, MessageType.None);

            if (isPlaying)
            {
                EditorGUILayout.Space(12);
                EditorGUILayout.LabelField("Active Test Heroes (DEV_TEST_*):", EditorStyles.boldLabel);
                var services = Phase1VerificationHelper.GetRuntimeServices();
                if (services?.Character != null)
                {
                    var testHeroes = services.Character.GetAllCharacters()
                        .Where(c => c.InstanceId.StartsWith(Phase1VerificationHelper.TestIdPrefix))
                        .ToList();

                    if (testHeroes.Count == 0)
                    {
                        EditorGUILayout.LabelField("No active test heroes currently in memory.");
                    }
                    else
                    {
                        foreach (var hero in testHeroes)
                        {
                            EditorGUILayout.BeginHorizontal("box");
                            EditorGUILayout.LabelField($"{hero.Definition?.id} (Lv.{hero.Level})", GUILayout.Width(130));
                            EditorGUILayout.LabelField($"Traits: {hero.TraitCommon}/{hero.TraitRare}", GUILayout.Width(170));
                            if (GUILayout.Button("Inspect UI", GUILayout.Width(80)))
                            {
                                Phase1VerificationHelper.RefreshUI(services, hero);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUI.EndDisabledGroup();
        }
    }
}
#endif
