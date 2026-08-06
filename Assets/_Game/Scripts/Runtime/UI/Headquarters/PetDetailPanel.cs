using System.Collections.Generic;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Legacy;
using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Runtime.UI.Headquarters
{
    /// <summary>
    /// Phase 5E Pet Detail overlay — a child panel of <see cref="ShelterDialog"/> (never a second
    /// AppShellController popup). Shows only real, populated fields: PetSaveData (InstanceId,
    /// DefinitionId, Level, Exp, EquippedToCharacterId) are all real. PetDefinition fields
    /// (PetName, BaseAttack/Defense/MaxHp/Speed, multipliers, SkillDefinitionId,
    /// EvolutionDefinitionId/Level) are declared on the model but pets.json only ever supplies
    /// `id` — every other PetDefinition field is its C# default for all 21 pets in the current
    /// data (verified). Per "không hiển thị N/A giả", those are shown only when non-default, which
    /// in the current data means they never render — see report "Known limitations". No actions:
    /// no Favourite/Autofeed/Feed/Breeding API exists anywhere in IPetService.
    /// </summary>
    public sealed class PetDetailPanel : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _infoText;
        [SerializeField] private Button _closeButton;

        private ServiceContainer _services;
        private System.Action _onClose;

        public void Setup(ServiceContainer services, System.Action onClose)
        {
            _services = services;
            _onClose = onClose;

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(() => _onClose?.Invoke());
            }
        }

        public void Show(PetSaveData pet)
        {
            if (pet == null) return;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Refresh(pet);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Refresh(PetSaveData pet)
        {
            if (_icon != null)
            {
                Sprite sprite = LegacySpriteRegistry.GetSprite(pet.DefinitionId);
                _icon.sprite = sprite;
                _icon.color = sprite != null ? Color.white : new Color(1f, 1f, 1f, 0.25f);
            }

            if (_nameText != null) _nameText.text = PetFormat.FormatId(pet.DefinitionId);

            bool hasDef = _services.Database.TryGet<PetDefinition>(pet.DefinitionId, out var def);

            var lines = new List<string>
            {
                $"Level: {pet.Level}",
                $"EXP: {pet.Exp} / {(hasDef && def.ExpToLevel > 0 ? def.ExpToLevel : 100)}"
            };

            if (hasDef)
            {
                string stats = BuildStatsLine(def);
                if (!string.IsNullOrEmpty(stats)) lines.Add($"Stats: {stats}");

                if (!string.IsNullOrEmpty(def.SkillDefinitionId))
                    lines.Add($"Skill: {PetFormat.FormatId(def.SkillDefinitionId)}");

                if (!string.IsNullOrEmpty(def.EvolutionDefinitionId) && def.EvolutionLevel > 0)
                    lines.Add($"Evolves into {PetFormat.FormatId(def.EvolutionDefinitionId)} at Lv.{def.EvolutionLevel}");
            }

            lines.Add(string.IsNullOrEmpty(pet.EquippedToCharacterId)
                ? "Not assigned to any adventurer"
                : $"Assigned to adventurer #{pet.EquippedToCharacterId}");

            if (_infoText != null) _infoText.text = string.Join("\n", lines);
        }

        private static string BuildStatsLine(PetDefinition def)
        {
            var parts = new List<string>();
            if (def.BaseAttack != 0) parts.Add($"ATK {def.BaseAttack}");
            if (def.BaseDefense != 0) parts.Add($"DEF {def.BaseDefense}");
            if (def.BaseMaxHp != 50) parts.Add($"HP {def.BaseMaxHp}"); // 50 is the unpopulated C# default
            if (def.BaseSpeed != 10) parts.Add($"SPD {def.BaseSpeed}"); // 10 is the unpopulated C# default
            return string.Join(", ", parts);
        }
    }
}
