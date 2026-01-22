using MTGOnline.Interfaces;

namespace MTGOnline.Combat
{
    /// <summary>
    /// Represents an attacking creature in combat.
    /// </summary>
    public class Attacker : IAttacker
    {
        public ICreature Creature { get; }
        public IPlayer? DefendingPlayer { get; }
        public IPlaneswalker? TargetPlaneswalker { get; }
        public IBattle? TargetBattle { get; }

        private readonly List<ICreature> _blockers = new();
        public IReadOnlyList<ICreature> Blockers => _blockers.AsReadOnly();

        private List<ICreature> _damageAssignmentOrder = new();
        public IReadOnlyList<ICreature> DamageAssignmentOrder => _damageAssignmentOrder.AsReadOnly();

        public bool IsBlocked => _blockers.Count > 0;

        public bool MustBeBlocked { get; set; }
        public bool CantBeBlocked { get; set; }

        public Attacker(ICreature creature, IPlayer defendingPlayer)
        {
            Creature = creature;
            DefendingPlayer = defendingPlayer;
        }

        public Attacker(ICreature creature, IPlaneswalker targetPlaneswalker)
        {
            Creature = creature;
            TargetPlaneswalker = targetPlaneswalker;
        }

        public Attacker(ICreature creature, IBattle targetBattle)
        {
            Creature = creature;
            TargetBattle = targetBattle;
        }

        public void AddBlocker(ICreature blocker)
        {
            if (!_blockers.Contains(blocker))
            {
                _blockers.Add(blocker);
                _damageAssignmentOrder.Add(blocker);
            }
        }

        public void RemoveBlocker(ICreature blocker)
        {
            _blockers.Remove(blocker);
            _damageAssignmentOrder.Remove(blocker);
        }

        public void SetDamageAssignmentOrder(IReadOnlyList<ICreature> order)
        {
            // Validate that order contains exactly the blockers
            if (order.Count != _blockers.Count ||
                !order.All(b => _blockers.Contains(b)))
            {
                throw new ArgumentException("Invalid damage assignment order");
            }

            _damageAssignmentOrder = order.ToList();
        }

        public int CalculateCombatDamage()
        {
            return Creature.Power;
        }
    }
}
