using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public interface IEquipmentService
    {
        bool CanEquip(CharacterRuntime character, ItemRuntime item, EquipmentSlot slot);
        bool Equip(CharacterRuntime character, string itemInstanceId, EquipmentSlot slot);
        bool Unequip(CharacterRuntime character, EquipmentSlot slot);
    }
}
