using GrandArchive.Core.Enums;

namespace GrandArchive.Interfaces
{
    /// <summary>
    /// Interface for abilities in Grand Archive TCG.
    /// </summary>
    public interface IAbility
    {
        /// <summary>Unique identifier for this ability</summary>
        Guid Id { get; }

        /// <summary>Name of the ability</summary>
        string Name { get; }

        /// <summary>The card this ability belongs to</summary>
        ICard Source { get; }

        /// <summary>Rules text describing the ability</summary>
        string RulesText { get; }

        /// <summary>Type of ability</summary>
        AbilityType AbilityType { get; }

        /// <summary>Whether this ability can currently be activated</summary>
        bool CanActivate(IGameState gameState);

        /// <summary>Activate this ability</summary>
        void Activate(IGameState gameState);

        /// <summary>Resolve this ability's effect</summary>
        void Resolve(IGameState gameState);
    }

    /// <summary>
    /// Types of abilities in Grand Archive TCG.
    /// </summary>
    public enum AbilityType
    {
        /// <summary>Activated ability - requires activation by player</summary>
        Activated,

        /// <summary>Triggered ability - triggers on game events</summary>
        Triggered,

        /// <summary>Static ability - always in effect</summary>
        Static,

        /// <summary>Mastery ability - champion's special ability</summary>
        Mastery,

        /// <summary>Class Bonus ability - active when class matches</summary>
        ClassBonus,

        /// <summary>Champion Bonus ability - active for specific champion</summary>
        ChampionBonus
    }

    /// <summary>
    /// Interface for triggered abilities.
    /// </summary>
    public interface ITriggeredAbility : IAbility
    {
        /// <summary>The trigger condition for this ability</summary>
        TriggerCondition TriggerCondition { get; }

        /// <summary>Check if this ability should trigger</summary>
        bool ShouldTrigger(IGameEvent gameEvent);
    }

    /// <summary>
    /// Trigger conditions for triggered abilities.
    /// </summary>
    public enum TriggerCondition
    {
        /// <summary>When this card enters the field</summary>
        OnEnter,

        /// <summary>When this card leaves the field</summary>
        OnLeave,

        /// <summary>When this card is destroyed</summary>
        OnDestroy,

        /// <summary>When this unit attacks</summary>
        OnAttack,

        /// <summary>When this unit deals damage</summary>
        OnDealDamage,

        /// <summary>When this unit takes damage</summary>
        OnTakeDamage,

        /// <summary>At the beginning of a phase</summary>
        AtPhaseStart,

        /// <summary>At the end of a phase</summary>
        AtPhaseEnd,

        /// <summary>When a card is drawn</summary>
        OnDraw,

        /// <summary>When a card is played</summary>
        OnPlay
    }
}
