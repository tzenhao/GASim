using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;

namespace MTGOnline.Events
{
    /// <summary>
    /// Base class for game events.
    /// </summary>
    public abstract class GameEvent : IGameEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public bool WasReplaced { get; set; }
        public bool WasPrevented { get; set; }
    }

    /// <summary>
    /// Event raised when damage is dealt.
    /// </summary>
    public class DamageEvent : GameEvent, IDamageEvent
    {
        public ICard Source { get; }
        public int Amount { get; set; }
        public bool IsCombatDamage { get; }
        public bool IsPreventable { get; set; } = true;

        public ICreature? TargetCreature { get; }
        public IPlaneswalker? TargetPlaneswalker { get; }
        public IPlayer? TargetPlayer { get; }

        public DamageEvent(ICard source, int amount, IPlayer targetPlayer, bool isCombatDamage = false)
        {
            Source = source;
            Amount = amount;
            TargetPlayer = targetPlayer;
            IsCombatDamage = isCombatDamage;
        }

        public DamageEvent(ICard source, int amount, ICreature targetCreature, bool isCombatDamage = false)
        {
            Source = source;
            Amount = amount;
            TargetCreature = targetCreature;
            IsCombatDamage = isCombatDamage;
        }

        public DamageEvent(ICard source, int amount, IPlaneswalker targetPlaneswalker, bool isCombatDamage = false)
        {
            Source = source;
            Amount = amount;
            TargetPlaneswalker = targetPlaneswalker;
            IsCombatDamage = isCombatDamage;
        }
    }

    /// <summary>
    /// Event raised when a player gains life.
    /// </summary>
    public class LifeGainEvent : GameEvent, ILifeGainEvent
    {
        public IPlayer Player { get; }
        public int Amount { get; }
        public ICard? Source { get; }

        public LifeGainEvent(IPlayer player, int amount, ICard? source = null)
        {
            Player = player;
            Amount = amount;
            Source = source;
        }
    }

    /// <summary>
    /// Event raised when a player loses life.
    /// </summary>
    public class LifeLossEvent : GameEvent, ILifeLossEvent
    {
        public IPlayer Player { get; }
        public int Amount { get; }
        public ICard? Source { get; }

        public LifeLossEvent(IPlayer player, int amount, ICard? source = null)
        {
            Player = player;
            Amount = amount;
            Source = source;
        }
    }

    /// <summary>
    /// Event raised when a card is drawn.
    /// </summary>
    public class DrawCardEvent : GameEvent, IDrawCardEvent
    {
        public IPlayer Player { get; }
        public ICard Card { get; }
        public bool IsFirstDraw { get; }

        public DrawCardEvent(IPlayer player, ICard card, bool isFirstDraw = false)
        {
            Player = player;
            Card = card;
            IsFirstDraw = isFirstDraw;
        }
    }

    /// <summary>
    /// Event raised when a permanent enters the battlefield.
    /// </summary>
    public class EntersBattlefieldEvent : GameEvent, IEntersBattlefieldEvent
    {
        public IPermanent Permanent { get; }
        public ZoneType FromZone { get; }

        public EntersBattlefieldEvent(IPermanent permanent, ZoneType fromZone)
        {
            Permanent = permanent;
            FromZone = fromZone;
        }
    }

    /// <summary>
    /// Event raised when a permanent leaves the battlefield.
    /// </summary>
    public class LeavesBattlefieldEvent : GameEvent, ILeavesBattlefieldEvent
    {
        public IPermanent Permanent { get; }
        public ZoneType ToZone { get; }

        public LeavesBattlefieldEvent(IPermanent permanent, ZoneType toZone)
        {
            Permanent = permanent;
            ToZone = toZone;
        }
    }

    /// <summary>
    /// Event raised when a creature dies.
    /// </summary>
    public class DiesEvent : GameEvent, IDiesEvent
    {
        public ICreature Creature { get; }

        public DiesEvent(ICreature creature)
        {
            Creature = creature;
        }
    }

    /// <summary>
    /// Event raised when a spell is cast.
    /// </summary>
    public class CastSpellEvent : GameEvent, ICastSpellEvent
    {
        public ISpell Spell { get; }
        public IPlayer Controller { get; }

        private readonly List<ITarget> _targets;
        public IReadOnlyList<ITarget> Targets => _targets.AsReadOnly();

        public CastSpellEvent(ISpell spell, IPlayer controller, IReadOnlyList<ITarget>? targets = null)
        {
            Spell = spell;
            Controller = controller;
            _targets = targets?.ToList() ?? new List<ITarget>();
        }
    }

    /// <summary>
    /// Event raised when a spell or ability is countered.
    /// </summary>
    public class CounteredEvent : GameEvent, ICounteredEvent
    {
        public IStackObject CounteredObject { get; }
        public ICard? CounteringSource { get; }

        public CounteredEvent(IStackObject counteredObject, ICard? counteringSource = null)
        {
            CounteredObject = counteredObject;
            CounteringSource = counteringSource;
        }
    }

    /// <summary>
    /// Event raised at beginning of combat.
    /// </summary>
    public class BeginCombatEvent : GameEvent, IBeginCombatEvent
    {
        public IPlayer AttackingPlayer { get; }

        public BeginCombatEvent(IPlayer attackingPlayer)
        {
            AttackingPlayer = attackingPlayer;
        }
    }

    /// <summary>
    /// Event raised when attackers are declared.
    /// </summary>
    public class DeclareAttackersEvent : GameEvent, IDeclareAttackersEvent
    {
        private readonly List<ICreature> _attackers;
        public IReadOnlyList<ICreature> Attackers => _attackers.AsReadOnly();

        public DeclareAttackersEvent(IReadOnlyList<ICreature> attackers)
        {
            _attackers = attackers.ToList();
        }
    }

    /// <summary>
    /// Event raised when blockers are declared.
    /// </summary>
    public class DeclareBlockersEvent : GameEvent, IDeclareBlockersEvent
    {
        private readonly Dictionary<ICreature, IReadOnlyList<ICreature>> _blockAssignments;
        public IReadOnlyDictionary<ICreature, IReadOnlyList<ICreature>> BlockAssignments =>
            _blockAssignments;

        public DeclareBlockersEvent(Dictionary<ICreature, IReadOnlyList<ICreature>> blockAssignments)
        {
            _blockAssignments = blockAssignments;
        }
    }

    /// <summary>
    /// Event raised when a card changes zones.
    /// </summary>
    public class ZoneChangeEvent : GameEvent, IZoneChangeEvent
    {
        public ICard Card { get; }
        public ZoneType FromZone { get; }
        public ZoneType ToZone { get; }

        public ZoneChangeEvent(ICard card, ZoneType fromZone, ZoneType toZone)
        {
            Card = card;
            FromZone = fromZone;
            ToZone = toZone;
        }
    }

    /// <summary>
    /// Event raised at the beginning of a turn.
    /// </summary>
    public class TurnBeginEvent : GameEvent, ITurnBeginEvent
    {
        public IPlayer ActivePlayer { get; }
        public int TurnNumber { get; }

        public TurnBeginEvent(IPlayer activePlayer, int turnNumber)
        {
            ActivePlayer = activePlayer;
            TurnNumber = turnNumber;
        }
    }

    /// <summary>
    /// Event raised at the end of a turn.
    /// </summary>
    public class TurnEndEvent : GameEvent, ITurnEndEvent
    {
        public IPlayer ActivePlayer { get; }
        public int TurnNumber { get; }

        public TurnEndEvent(IPlayer activePlayer, int turnNumber)
        {
            ActivePlayer = activePlayer;
            TurnNumber = turnNumber;
        }
    }

    /// <summary>
    /// Event raised when counters are added or removed.
    /// </summary>
    public class CounterEvent : GameEvent, ICounterEvent
    {
        public CounterType CounterType { get; }
        public int Amount { get; }
        public bool WasAdded { get; }
        public IPermanent? Permanent { get; }
        public IPlayer? Player { get; }

        public CounterEvent(CounterType counterType, int amount, bool wasAdded, IPermanent permanent)
        {
            CounterType = counterType;
            Amount = amount;
            WasAdded = wasAdded;
            Permanent = permanent;
        }

        public CounterEvent(CounterType counterType, int amount, bool wasAdded, IPlayer player)
        {
            CounterType = counterType;
            Amount = amount;
            WasAdded = wasAdded;
            Player = player;
        }
    }
}
