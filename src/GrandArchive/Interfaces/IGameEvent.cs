using GrandArchive.Core.Enums;

namespace GrandArchive.Interfaces
{
    /// <summary>
    /// Interface for game events in Grand Archive TCG.
    /// </summary>
    public interface IGameEvent
    {
        /// <summary>Type of event</summary>
        GameEventType EventType { get; }

        /// <summary>The source of the event (card, ability, etc.)</summary>
        object? Source { get; }

        /// <summary>The player who caused the event</summary>
        IPlayer? Player { get; }

        /// <summary>Timestamp when the event occurred</summary>
        DateTime Timestamp { get; }
    }

    /// <summary>
    /// Types of game events.
    /// </summary>
    public enum GameEventType
    {
        // Game flow events
        GameStarted,
        GameEnded,
        TurnStarted,
        TurnEnded,
        PhaseStarted,
        PhaseEnded,

        // Card events
        CardDrawn,
        CardPlayed,
        CardActivated,
        CardResolved,
        CardDestroyed,
        CardBanished,
        CardEnteredZone,
        CardLeftZone,

        // Unit events
        UnitEnteredField,
        UnitLeftField,
        UnitWokeUp,
        UnitRested,
        UnitAttacked,
        UnitDefended,
        UnitDefeated,

        // Combat events
        CombatStarted,
        CombatEnded,
        AttackDeclared,
        InterceptionDeclared,
        RetaliationDeclared,
        DamageDealt,

        // Counter events
        CounterAdded,
        CounterRemoved,

        // Stack events
        StackObjectAdded,
        StackObjectResolved,
        StackObjectRemoved,

        // Player events
        PlayerGainedOpportunity,
        PlayerPassedOpportunity,
        PlayerLost
    }

    /// <summary>
    /// Event for card zone changes.
    /// </summary>
    public interface IZoneChangeEvent : IGameEvent
    {
        /// <summary>The card that changed zones</summary>
        ICard Card { get; }

        /// <summary>The zone the card came from</summary>
        ZoneType FromZone { get; }

        /// <summary>The zone the card went to</summary>
        ZoneType ToZone { get; }
    }

    /// <summary>
    /// Event for damage being dealt.
    /// </summary>
    public interface IDamageEvent : IGameEvent
    {
        /// <summary>The source of the damage</summary>
        ICard DamageSource { get; }

        /// <summary>The target receiving damage</summary>
        IUnit Target { get; }

        /// <summary>Amount of damage dealt</summary>
        int Amount { get; }

        /// <summary>Whether this is combat damage</summary>
        bool IsCombatDamage { get; }
    }
}
