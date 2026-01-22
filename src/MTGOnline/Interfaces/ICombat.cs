using MTGOnline.Core.Enums;

namespace MTGOnline.Interfaces
{
    /// <summary>
    /// Interface for managing combat.
    /// </summary>
    public interface ICombatManager
    {
        bool IsInCombat { get; }
        IPlayer? AttackingPlayer { get; }
        IPlayer? DefendingPlayer { get; }
        IReadOnlyList<IAttacker> Attackers { get; }
        IReadOnlyList<IBlocker> Blockers { get; }
        CombatStep CurrentCombatStep { get; }

        // Combat flow
        void BeginCombat(IPlayer attackingPlayer, IPlayer defendingPlayer);
        void EndCombat();

        // Declare attackers step
        bool CanAttack(ICreature creature);
        void DeclareAttacker(ICreature creature, IPlayer defendingPlayer);
        void DeclareAttacker(ICreature creature, IPlaneswalker targetPlaneswalker);
        void DeclareAttacker(ICreature creature, IBattle targetBattle);
        void ConfirmAttackers();

        // Declare blockers step
        bool CanBlock(ICreature blocker, ICreature attacker);
        void DeclareBlocker(ICreature blocker, ICreature attacker);
        void ConfirmBlockers();

        // Damage assignment
        void AssignDamageOrder(ICreature attacker, IReadOnlyList<ICreature> blockerOrder);
        void AssignCombatDamage();
        void ApplyFirstStrikeDamage();
        void ApplyRegularCombatDamage();

        // Queries
        IReadOnlyList<ICreature> GetBlockersFor(ICreature attacker);
        ICreature? GetBlockedCreatureFor(ICreature blocker);
        bool IsAttacking(ICreature creature);
        bool IsBlocking(ICreature creature);
        bool IsBlocked(ICreature attacker);
        bool IsUnblocked(ICreature attacker);
    }

    /// <summary>
    /// Combat step enumeration.
    /// </summary>
    public enum CombatStep
    {
        NotInCombat,
        BeginningOfCombat,
        DeclareAttackers,
        DeclareBlockers,
        FirstStrikeDamage,
        CombatDamage,
        EndOfCombat
    }

    /// <summary>
    /// Interface for an attacking creature.
    /// </summary>
    public interface IAttacker
    {
        ICreature Creature { get; }
        IPlayer? DefendingPlayer { get; }
        IPlaneswalker? TargetPlaneswalker { get; }
        IBattle? TargetBattle { get; }
        IReadOnlyList<ICreature> Blockers { get; }
        IReadOnlyList<ICreature> DamageAssignmentOrder { get; }
        bool IsBlocked { get; }
        bool MustBeBlocked { get; }
        bool CantBeBlocked { get; }

        void AddBlocker(ICreature blocker);
        void RemoveBlocker(ICreature blocker);
        void SetDamageAssignmentOrder(IReadOnlyList<ICreature> order);
        int CalculateCombatDamage();
    }

    /// <summary>
    /// Interface for a blocking creature.
    /// </summary>
    public interface IBlocker
    {
        ICreature Creature { get; }
        ICreature BlockedAttacker { get; }

        int CalculateCombatDamage();
    }

    /// <summary>
    /// Interface for battles (a card type introduced in March of the Machine).
    /// </summary>
    public interface IBattle : IPermanent
    {
        int DefenseCounters { get; }
        IPlayer Protector { get; }

        void DealDamage(int amount);
    }
}
