using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Core;
using GuildMaster.Runtime.UI.Inventory;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.Models;
using GuildMaster.Definitions;

namespace GuildMaster.Runtime.UI.Character
{
    /// <summary>
    /// Character screen — shows owned adventurers, stats, equipment slots, and multi-party management.
    /// Player clicks a character card to select, then uses party tabs (Đội 1/2/3) to manage parties.
    /// Sources: AdventurersFragment.java, DialogSelectEquipment.java, DialogChooseAdventurer.java
    /// </summary>
    public class CharacterScreen : UIScreen
    {
        private ICharacterService  _characterService;
        private IEquipmentService  _equipmentService;
        private IPartyService      _partyService;
        private InventoryScreen    _inventoryScreen;
        [SerializeField] private EquipmentPopup _equipmentPopup;

        // ── Serialized references (wired by Apply tool) ──────────────────────────
        [SerializeField] private RectTransform _cardContainer;
        [SerializeField] private Text          _summaryText;
        [SerializeField] private Text          _detailText;
        [SerializeField] private Button        _addToPartyButton;
        [SerializeField] private Button        _removeFromPartyButton;
        [SerializeField] private Button        _unequipWeaponButton;
        [SerializeField] private Button        _unequipArmorButton;
        [SerializeField] private Button        _unequipAccessoryButton;
        [SerializeField] private Button        _equipButton;
        [SerializeField] private Button        _dismissButton;
        [SerializeField] private Text          _feedbackText;

        // ── Multi-Party UI references ────────────────────────────────────────────
        [SerializeField] private RectTransform _partyTabBar;
        [SerializeField] private RectTransform _partySlotContainer;

        private int _selectedIndex;

        public void Initialize(ServiceContainer services, InventoryScreen inventoryScreen = null)
        {
            _characterService = services.Character;
            _equipmentService  = services.Equipment;
            _partyService      = services.Party;
            _inventoryScreen   = inventoryScreen;
            
            if (_addToPartyButton != null) _addToPartyButton.onClick.AddListener(OnClickAddToParty);
            if (_removeFromPartyButton != null) _removeFromPartyButton.onClick.AddListener(OnClickRemoveFromParty);
            if (_dismissButton != null) _dismissButton.onClick.AddListener(OnClickDismissCharacter);
            if (_unequipWeaponButton != null) _unequipWeaponButton.onClick.AddListener(OnClickUnequipWeapon);
            if (_unequipArmorButton != null) _unequipArmorButton.onClick.AddListener(OnClickUnequipArmor);
            if (_unequipAccessoryButton != null) _unequipAccessoryButton.onClick.AddListener(OnClickUnequipAccessory);
            if (_equipButton != null) _equipButton.onClick.AddListener(OnClickEquipWeapon); // Legacy fallback
            
            if (_equipmentPopup != null) _equipmentPopup.Initialize(services);
        }

        public override void Show()
        {
            base.Show();
            Refresh();
        }

        /// <summary>Exposes the player's selected character for Inventory/Dungeon cross-screen actions.</summary>
        public CharacterRuntime GetSelectedCharacter()
        {
            var chars = _characterService?.GetAllCharacters();
            if (chars == null || chars.Count == 0) return null;
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, chars.Count - 1);
            return chars[_selectedIndex];
        }

        /// <summary>Returns the current party members for the active party tab.</summary>
        public List<string> GetPartyMemberIds()
        {
            if (_partyService == null) return new List<string>();
            var result = new List<string>(_partyService.GetPartyMembers());
            if (result.Count == 0)
            {
                var sel = GetSelectedCharacter();
                if (sel != null) result.Add(sel.InstanceId);
            }
            return result;
        }

        /// <summary>Returns party members for a specific party index (used by DungeonScreen).</summary>
        public List<string> GetPartyMemberIds(int partyIndex)
        {
            if (_partyService == null) return new List<string>();
            return new List<string>(_partyService.GetPartyMembers(partyIndex));
        }

        public bool IsInParty(string instanceId) => _partyService?.IsInAnyParty(instanceId) ?? false;

        public void Refresh()
        {
            if (_characterService == null) return;

            var chars = _characterService.GetAllCharacters();
            if (chars == null || chars.Count == 0)
            {
                BuildCards(null);
                if (_summaryText != null) _summaryText.text = "No adventurers recruited yet. Recruit from the Tavern.";
                if (_detailText  != null) _detailText.text  = "No character selected.";
                UpdateActionButtons(null);
                BuildPartyTabs();
                BuildPartySlots();
                return;
            }

            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, chars.Count - 1);
            BuildCards(chars);

            var selected = chars[_selectedIndex];
            int activeIdx = _partyService?.ActivePartyIndex ?? 0;
            int partyCount = _partyService?.GetPartyMembers(activeIdx).Count ?? 0;
            int maxParty = _partyService?.MaxPartySize ?? 4;

            if (_summaryText != null)
                _summaryText.text = $"Adventurers: {chars.Count}  |  Party {activeIdx + 1}: {partyCount}/{maxParty}";

            if (_detailText != null)
                _detailText.text = BuildDetailText(selected);

            UpdateActionButtons(selected);
            BuildPartyTabs();
            BuildPartySlots();
        }

        private string BuildDetailText(CharacterRuntime c)
        {
            if (c == null) return "";
            string defId = c.Definition != null ? c.Definition.id : "Unknown";

            int partyIdx = _partyService?.GetPartyIndexOf(c.InstanceId) ?? -1;
            string partyTag;
            if (partyIdx >= 0)
                partyTag = $"<color=#4CAF50><b>[Party {partyIdx + 1}]</b></color>";
            else
                partyTag = "<color=#888888>[Not in Party]</color>";

            string text =
                $"<size=20><b><color=#FFD700>{defId}</color></b></size>  <color=#AAAAAA>Lv.{c.Level}</color>  {partyTag}\n" +
                $"<color=#E0E0E0>HP:</color> <color=#4CAF50>{c.CurrentHp}</color>  |  <color=#E0E0E0>XP:</color> <color=#2196F3>{c.Experience}</color>\n\n" +
                $"<b><color=#FFC107>── STATS ──</color></b>\n";

            if (_characterService != null)
            {
                text +=
                    $"  CON: <b>{_characterService.GetTotalStat(c, StatType.Constitution)}</b>    DEX: <b>{_characterService.GetTotalStat(c, StatType.Dexterity)}</b>    DEF: <b>{_characterService.GetTotalStat(c, StatType.Defense)}</b>\n" +
                    $"  INT: <b>{_characterService.GetTotalStat(c, StatType.Intelligence)}</b>    MGD: <b>{_characterService.GetTotalStat(c, StatType.MagicDefense)}</b>    IMM: <b>{_characterService.GetTotalStat(c, StatType.ImmunityToStatus)}</b>\n\n";
            }

            text += "<b><color=#FFC107>── EQUIPMENT ──</color></b>\n";
            text += $"  🗡️ <b>Weapon:</b>    {(c.Weapon != null    ? $"<color=#00E676>{c.Weapon.Definition?.id ?? c.Weapon.InstanceId}</color>" : "<color=#777777>(empty)</color>")}\n";
            text += $"  🛡️ <b>Armor:</b>     {(c.Armor != null     ? $"<color=#00E676>{c.Armor.Definition?.id ?? c.Armor.InstanceId}</color>"  : "<color=#777777>(empty)</color>")}\n";
            text += $"  💍 <b>Accessory:</b> {(c.Accessory != null ? $"<color=#00E676>{c.Accessory.Definition?.id ?? c.Accessory.InstanceId}</color>" : "<color=#777777>(empty)</color>")}\n";

            if (!string.IsNullOrEmpty(c.Trait))
            {
                text += $"\n<b>Trait:</b> <color=#9C27B0>{c.Trait}</color>\n";
            }

            return text;
        }

        private void UpdateActionButtons(CharacterRuntime selected)
        {
            bool hasSelected = selected != null;
            int activeIdx = _partyService?.ActivePartyIndex ?? 0;
            bool inActiveParty = hasSelected && (_partyService?.GetPartyMembers(activeIdx).Contains(selected.InstanceId) ?? false);
            bool inAnyParty = hasSelected && IsInParty(selected.InstanceId);
            bool partyFull   = (_partyService?.GetPartyMembers(activeIdx)?.Count ?? 0) >= (_partyService?.MaxPartySize ?? 4);

            if (_addToPartyButton != null)
            {
                _addToPartyButton.interactable = hasSelected && !inAnyParty && !partyFull;
                var txt = _addToPartyButton.GetComponentInChildren<Text>();
                if (txt != null) txt.text = $"Add to Party {activeIdx + 1}";
            }

            if (_removeFromPartyButton != null)
            {
                _removeFromPartyButton.interactable = hasSelected && inActiveParty;
                var txt = _removeFromPartyButton.GetComponentInChildren<Text>();
                if (txt != null) txt.text = $"Remove from Party {activeIdx + 1}";
            }

            if (_unequipWeaponButton   != null) _unequipWeaponButton.interactable   = hasSelected && selected.Weapon    != null;
            if (_unequipArmorButton    != null) _unequipArmorButton.interactable    = hasSelected && selected.Armor     != null;
            if (_unequipAccessoryButton != null) _unequipAccessoryButton.interactable = hasSelected && selected.Accessory != null;
            
            if (_dismissButton != null)
            {
                string dismissReason = null;
                bool canDismiss = hasSelected && _characterService.CanDismissCharacter(selected.InstanceId, out dismissReason);
                _dismissButton.interactable = canDismiss;
                var dismissText = _dismissButton.GetComponentInChildren<Text>();
                if (dismissText != null)
                {
                    if (!hasSelected)
                        dismissText.text = "Dismiss";
                    else if (canDismiss)
                        dismissText.text = "Dismiss ❌";
                    else
                        dismissText.text = $"⚠ {dismissReason}";
                }
            }

            if (_equipButton != null)
            {
                _equipButton.interactable = hasSelected;
            }
        }

        // ── Party Tab Bar (Party 1 / Party 2 / Party 3) ──────────────────────────────

        private void BuildPartyTabs()
        {
            if (_partyTabBar == null || _partyService == null) return;
            UICardFactory.ClearContainer(_partyTabBar);

            int activeIdx = _partyService.ActivePartyIndex;
            for (int i = 0; i < _partyService.PartyCount; i++)
            {
                int captured = i;
                int count = _partyService.GetPartyMembers(i).Count;
                bool isActive = (i == activeIdx);
                string label = $"Party {i + 1} ({count}/{_partyService.MaxPartySize})";
                UICardFactory.CreateCard(_partyTabBar, label, "", isActive, true, () => SelectPartyTab(captured), 36);
            }
        }

        private void SelectPartyTab(int index)
        {
            if (_partyService == null) return;
            _partyService.SetActivePartyIndex(index);
            Refresh();
        }

        // ── Party Slot Cards (4 slots showing members) ──────────────────────────

        private void BuildPartySlots()
        {
            if (_partySlotContainer == null || _partyService == null) return;
            UICardFactory.ClearContainer(_partySlotContainer);

            int activeIdx = _partyService.ActivePartyIndex;
            var members = _partyService.GetPartyMembers(activeIdx);
            var allChars = _characterService?.GetAllCharacters();

            for (int slot = 0; slot < _partyService.MaxPartySize; slot++)
            {
                if (slot < members.Count)
                {
                    string charId = members[slot];
                    string displayName = charId;

                    // Try to resolve display name from character runtime
                    if (allChars != null)
                    {
                        foreach (var c in allChars)
                        {
                            if (c.InstanceId == charId)
                            {
                                displayName = c.Definition?.id ?? charId;
                                break;
                            }
                        }
                    }
                    UICardFactory.CreateCard(_partySlotContainer, $"★ {displayName}", $"Slot {slot + 1}", true, false, null, 44);
                }
                else
                {
                    UICardFactory.CreateCard(_partySlotContainer, "[Empty]", $"Slot {slot + 1}", false, false, null, 44);
                }
            }
        }

        private void BuildCards(IReadOnlyList<CharacterRuntime> chars)
        {
            if (_cardContainer == null) return;
            UICardFactory.ClearContainer(_cardContainer);

            if (chars == null || chars.Count == 0) return;

            for (int i = 0; i < chars.Count; i++)
            {
                int captured = i;
                var c        = chars[i];
                string defId = c.Definition != null ? c.Definition.id : "Unknown";

                int partyIdx = _partyService?.GetPartyIndexOf(c.InstanceId) ?? -1;
                string partyTag = partyIdx >= 0 ? $" <color=#4CAF50>★ Party {partyIdx + 1}</color>" : "";
                string title = partyIdx >= 0 ? $"★ {defId}" : defId;
                string sub   = $"Lv.{c.Level}  HP:{c.CurrentHp} | XP:{c.Experience}{partyTag}";

                UICardFactory.CreateCard(_cardContainer, title, sub,
                    i == _selectedIndex, true, () => SelectIndex(captured), 58);
            }
        }

        /// <summary>Click-to-select — also callable from tests.</summary>
        public void SelectIndex(int index)
        {
            int count = _characterService?.GetAllCharacters().Count ?? 0;
            if (count == 0) return;
            _selectedIndex = Mathf.Clamp(index, 0, count - 1);
            Refresh();
        }

        public void OnClickSelectNext()     => CycleSelection(1);
        public void OnClickSelectPrevious() => CycleSelection(-1);

        private void CycleSelection(int dir)
        {
            int count = _characterService?.GetAllCharacters().Count ?? 0;
            if (count == 0) return;
            _selectedIndex = ((_selectedIndex + dir) % count + count) % count;
            Refresh();
        }

        public void OnClickAddToParty()
        {
            var target = GetSelectedCharacter();
            if (target == null) return;
            if (_partyService == null) return;

            int activeIdx = _partyService.ActivePartyIndex;

            if (_partyService.IsInAnyParty(target.InstanceId))
            {
                int existingParty = _partyService.GetPartyIndexOf(target.InstanceId);
                ShowFeedback($"{target.Definition?.id ?? target.InstanceId} is already in Party {existingParty + 1}.", false);
                return;
            }

            int currentPartyCount = _partyService.GetPartyMembers(activeIdx).Count;
            if (currentPartyCount >= _partyService.MaxPartySize)
            {
                ShowFeedback($"Party {activeIdx + 1} is full ({currentPartyCount}/{_partyService.MaxPartySize})!", false);
                return;
            }

            if (_partyService.AddToParty(target.InstanceId, activeIdx))
            {
                ShowFeedback($"{target.Definition?.id ?? target.InstanceId} added to Party {activeIdx + 1}.", true);
            }
            else
            {
                ShowFeedback($"Cannot add to Party {activeIdx + 1}.", false);
            }
            Refresh();
        }

        public void OnClickRemoveFromParty()
        {
            var target = GetSelectedCharacter();
            if (target == null) return;

            int activeIdx = _partyService?.ActivePartyIndex ?? 0;
            if (_partyService != null && _partyService.RemoveFromParty(target.InstanceId, activeIdx))
            {
                ShowFeedback($"{target.Definition?.id ?? target.InstanceId} removed from Party {activeIdx + 1}.", false);
            }
            Refresh();
        }

        public void OnClickDismissCharacter()
        {
            var target = GetSelectedCharacter();
            if (target == null) return;
            if (_characterService != null)
            {
                if (!_characterService.CanDismissCharacter(target.InstanceId, out string reason))
                {
                    ShowFeedback(reason, false);
                    return;
                }
                
                // Note: Double confirm should be implemented via a Dialog UI in a full setup.
                if (_characterService.DismissCharacter(target.InstanceId, out string error))
                {
                    ShowFeedback("Character dismissed.", true);
                    _selectedIndex = 0;
                }
                else
                {
                    ShowFeedback(error, false);
                }
                Refresh();
            }
        }

        // Public UI bindings to open the Equipment Popup for specific slots
        public void OnClickEquipWeapon()    => OpenEquipmentPopup(EquipmentSlot.Weapon);
        public void OnClickEquipArmor()     => OpenEquipmentPopup(EquipmentSlot.Armor);
        public void OnClickEquipAccessory() => OpenEquipmentPopup(EquipmentSlot.Accessory);

        private void OpenEquipmentPopup(EquipmentSlot slot)
        {
            var target = GetSelectedCharacter();
            if (target == null) return;
            
            if (_equipmentPopup != null)
            {
                _equipmentPopup.ShowForSlot(target, slot);
            }
        }

        // Old generic equip button fallback if kept
        public void OnClickEquipSelectedItem()
        {
            var target = GetSelectedCharacter();
            if (target == null) return;
            // E.g. open popup for weapon by default, or ignore.
            OpenEquipmentPopup(EquipmentSlot.Weapon);
        }

        public void OnClickUnequipWeapon()     => UnequipSlot(EquipmentSlot.Weapon);
        public void OnClickUnequipArmor()      => UnequipSlot(EquipmentSlot.Armor);
        public void OnClickUnequipAccessory()  => UnequipSlot(EquipmentSlot.Accessory);

        private void UnequipSlot(EquipmentSlot slot)
        {
            var target = GetSelectedCharacter();
            if (target != null)
            {
                _equipmentService?.Unequip(target, slot);
                ShowFeedback($"Unequipped {slot}.", true);
            }
            Refresh();
        }

        private void ShowFeedback(string msg, bool success)
        {
            if (_feedbackText == null) return;
            _feedbackText.text  = msg;
            _feedbackText.color = success ? UITemporaryTheme.SuccessColor : UITemporaryTheme.WarningColor;
        }
    }
}
