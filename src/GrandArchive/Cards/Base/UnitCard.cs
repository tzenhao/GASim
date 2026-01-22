using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Cards.Base
{
    /// <summary>
    /// Base class for unit cards (Champions and Allies) in Grand Archive TCG.
    /// Units have Power and Life stats, and can be in Awake or Rested states.
    /// </summary>
    public abstract class UnitCard : Card, IUnit
    {
        public int BasePower { get; init; }

        public int BaseLife { get; init; }

        /// <summary>
        /// Current power after modifications (base + buff counters + effects).
        /// </summary>
        public int Power => BasePower + BuffCounters + PowerModifier;

        /// <summary>
        /// Current life after modifications (base + buff counters + effects).
        /// </summary>
        public int Life => BaseLife + BuffCounters + LifeModifier;

        /// <summary>
        /// Temporary power modifier from effects.
        /// </summary>
        public int PowerModifier { get; set; }

        /// <summary>
        /// Temporary life modifier from effects.
        /// </summary>
        public int LifeModifier { get; set; }

        public UnitState State { get; set; } = UnitState.Awake;

        public int BuffCounters { get; set; }

        public int DamageCounters { get; set; }

        /// <summary>
        /// A unit is defeated when damage counters >= life.
        /// </summary>
        public bool IsDefeated => DamageCounters >= Life;

        /// <summary>
        /// Wake up this unit (set to Awake state).
        /// </summary>
        public void WakeUp()
        {
            State = UnitState.Awake;
        }

        /// <summary>
        /// Rest this unit (set to Rested state).
        /// </summary>
        public void Rest()
        {
            State = UnitState.Rested;
        }

        /// <summary>
        /// Deal damage to this unit by adding damage counters.
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (amount > 0)
            {
                DamageCounters += amount;
            }
        }

        /// <summary>
        /// Remove damage counters from this unit.
        /// </summary>
        public void Heal(int amount)
        {
            if (amount > 0)
            {
                DamageCounters = Math.Max(0, DamageCounters - amount);
            }
        }

        /// <summary>
        /// Add buff counters to this unit (+1 power and +1 life each).
        /// </summary>
        public void AddBuffCounters(int count)
        {
            if (count > 0)
            {
                BuffCounters += count;
            }
        }

        /// <summary>
        /// Remove buff counters from this unit.
        /// </summary>
        public void RemoveBuffCounters(int count)
        {
            if (count > 0)
            {
                BuffCounters = Math.Max(0, BuffCounters - count);
            }
        }

        /// <summary>
        /// Check if this unit can attack.
        /// </summary>
        public virtual bool CanAttack()
        {
            return State == UnitState.Awake && !IsDefeated;
        }

        /// <summary>
        /// Check if this unit can intercept an attack.
        /// </summary>
        public virtual bool CanIntercept()
        {
            return State == UnitState.Awake && !IsDefeated && HasKeyword(Keyword.Intercept);
        }

        /// <summary>
        /// Check if this unit can retaliate.
        /// </summary>
        public virtual bool CanRetaliate()
        {
            return State == UnitState.Awake && !IsDefeated;
        }

        public override string ToString() => $"{Name} ({Power}/{Life}) [{State}]";
    }
}
