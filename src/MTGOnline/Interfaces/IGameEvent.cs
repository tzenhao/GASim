using MTGOnline.Core.Enums;

namespace MTGOnline.Interfaces
{
    /// <summary>
    /// Base interface for game events.
    /// </summary>
    public interface IGameEvent
    {
        Guid EventId { get; }
        DateTime Timestamp { get; }
        bool WasReplaced { get; }
        bool WasPrevented { get; }
    }

    /// <summary>
    /// Event for damage being dealt.
    /// </summary>
    public interface IDamageEvent : IGameEvent
    {
        ICard Source { get; }
        int Amount { get; }
        bool IsCombatDamage { get; }
        bool IsPreventable { get; }

        // Target can be creature, planeswalker, or player
        ICreature? TargetCreature { get; }
        IPlaneswalker? TargetPlaneswalker { get; }
        IPlayer? TargetPlayer { get; }
    }

    /// <summary>
    /// Event for life gain.
    /// </summary>
    public interface ILifeGainEvent : IGameEvent
    {
        IPlayer Player { get; }
        int Amount { get; }
        ICard? Source { get; }
    }

    /// <summary>
    /// Event for life loss.
    /// </summary>
    public interface ILifeLossEvent : IGameEvent
    {
        IPlayer Player { get; }
        int Amount { get; }
        ICard? Source { get; }
    }

    /// <summary>
    /// Event for drawing cards.
    /// </summary>
    public interface IDrawCardEvent : IGameEvent
    {
        IPlayer Player { get; }
        ICard Card { get; }
        bool IsFirstDraw { get; }  // First draw of turn
    }

    /// <summary>
    /// Event for a permanent entering the battlefield.
    /// </summary>
    public interface IEntersBattlefieldEvent : IGameEvent
    {
        IPermanent Permanent { get; }
        ZoneType FromZone { get; }
    }

    /// <summary>
    /// Event for a permanent leaving the battlefield.
    /// </summary>
    public interface ILeavesBattlefieldEvent : IGameEvent
    {
        IPermanent Permanent { get; }
        ZoneType ToZone { get; }
    }

    /// <summary>
    /// Event for a creature dying.
    /// </summary>
    public interface IDiesEvent : IGameEvent
    {
        ICreature Creature { get; }
    }

    /// <summary>
    /// Event for casting a spell.
    /// </summary>
    public interface ICastSpellEvent : IGameEvent
    {
        ISpell Spell { get; }
        IPlayer Controller { get; }
        IReadOnlyList<ITarget> Targets { get; }
    }

    /// <summary>
    /// Event for a spell or ability being countered.
    /// </summary>
    public interface ICounteredEvent : IGameEvent
    {
        IStackObject CounteredObject { get; }
        ICard? CounteringSource { get; }
    }

    /// <summary>
    /// Event for combat beginning.
    /// </summary>
    public interface IBeginCombatEvent : IGameEvent
    {
        IPlayer AttackingPlayer { get; }
    }

    /// <summary>
    /// Event for attackers being declared.
    /// </summary>
    public interface IDeclareAttackersEvent : IGameEvent
    {
        IReadOnlyList<ICreature> Attackers { get; }
    }

    /// <summary>
    /// Event for blockers being declared.
    /// </summary>
    public interface IDeclareBlockersEvent : IGameEvent
    {
        IReadOnlyDictionary<ICreature, IReadOnlyList<ICreature>> BlockAssignments { get; }
    }

    /// <summary>
    /// Event for a zone transfer.
    /// </summary>
    public interface IZoneChangeEvent : IGameEvent
    {
        ICard Card { get; }
        ZoneType FromZone { get; }
        ZoneType ToZone { get; }
    }

    /// <summary>
    /// Event for beginning of turn.
    /// </summary>
    public interface ITurnBeginEvent : IGameEvent
    {
        IPlayer ActivePlayer { get; }
        int TurnNumber { get; }
    }

    /// <summary>
    /// Event for end of turn.
    /// </summary>
    public interface ITurnEndEvent : IGameEvent
    {
        IPlayer ActivePlayer { get; }
        int TurnNumber { get; }
    }

    /// <summary>
    /// Event for phase/step changes.
    /// </summary>
    public interface IPhaseStepEvent : IGameEvent
    {
        PhaseType Phase { get; }
        StepType Step { get; }
        IPlayer ActivePlayer { get; }
    }

    /// <summary>
    /// Event for counters being added or removed.
    /// </summary>
    public interface ICounterEvent : IGameEvent
    {
        CounterType CounterType { get; }
        int Amount { get; }
        bool WasAdded { get; }

        // Can be on a permanent or player
        IPermanent? Permanent { get; }
        IPlayer? Player { get; }
    }
}
