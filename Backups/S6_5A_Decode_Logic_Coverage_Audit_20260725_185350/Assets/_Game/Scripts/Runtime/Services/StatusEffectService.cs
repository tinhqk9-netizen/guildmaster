using System.Collections.Generic;
using System.Linq;
using GuildMaster.Definitions;
using GuildMaster.Definitions.Enums;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public interface IStatusEffectService
    {
        void AddStatusEffect(CharacterRuntime character, StatusEffectDefinition definition, string sourceId, int turnsLeft);
        void AddStatusEffect(EnemyRuntime enemy, StatusEffectDefinition definition, string sourceId, int turnsLeft);
    }

    public class StatusEffectService : IStatusEffectService
    {
        // Re-use logic by accepting the lists directly
        private void InternalAddStatusEffect(
            List<StatusEffectRuntime> positiveList, 
            List<StatusEffectRuntime> negativeList, 
            StatusEffectDefinition definition, 
            string sourceId, 
            int turnsLeft)
        {
            var targetList = definition.IsNegative ? negativeList : positiveList;
            
            var existingEffect = targetList.FirstOrDefault(e => e.Type == definition.Type);

            if (definition.Type == StatusEffectType.BLEED)
            {
                if (existingEffect != null)
                {
                    existingEffect.TurnsLeft += turnsLeft;
                }
                else
                {
                    targetList.Add(new StatusEffectRuntime
                    {
                        Type = definition.Type,
                        SourceInstanceId = sourceId,
                        TurnsLeft = turnsLeft,
                        Probability = 1.0 // S2 defaults
                    });
                }
            }
            else
            {
                if (existingEffect != null)
                {
                    if (existingEffect.TurnsLeft < turnsLeft)
                    {
                        existingEffect.TurnsLeft = turnsLeft;
                        existingEffect.SourceInstanceId = sourceId;
                    }
                }
                else
                {
                    targetList.Add(new StatusEffectRuntime
                    {
                        Type = definition.Type,
                        SourceInstanceId = sourceId,
                        TurnsLeft = turnsLeft,
                        Probability = 1.0
                    });
                }
            }
        }

        public void AddStatusEffect(CharacterRuntime character, StatusEffectDefinition definition, string sourceId, int turnsLeft)
        {
            // Note: Immunity check is deferred to S3 or handled if immunity lists are added
            InternalAddStatusEffect(character.PositiveStatusEffects, character.NegativeStatusEffects, definition, sourceId, turnsLeft);
        }

        public void AddStatusEffect(EnemyRuntime enemy, StatusEffectDefinition definition, string sourceId, int turnsLeft)
        {
            InternalAddStatusEffect(enemy.PositiveStatusEffects, enemy.NegativeStatusEffects, definition, sourceId, turnsLeft);
        }
    }
}
