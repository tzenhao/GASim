using MTGOnline.Interfaces;

namespace MTGOnline.Combat
{
    /// <summary>
    /// Represents a blocking creature in combat.
    /// </summary>
    public class Blocker : IBlocker
    {
        public ICreature Creature { get; }
        public ICreature BlockedAttacker { get; }

        public Blocker(ICreature creature, ICreature blockedAttacker)
        {
            Creature = creature;
            BlockedAttacker = blockedAttacker;
        }

        public int CalculateCombatDamage()
        {
            return Creature.Power;
        }
    }
}
