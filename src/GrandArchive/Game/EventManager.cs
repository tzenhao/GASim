using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Game
{
    /// <summary>
    /// Manages game events and triggers in Grand Archive TCG.
    /// </summary>
    public class EventManager
    {
        private readonly GameState _gameState;
        private readonly List<ITriggeredAbility> _registeredTriggers = new();

        // Events
        public event Action<IPlayer>? OnGameStart;
        public event Action<IPlayer>? OnGameEnd;
        public event Action<IPlayer>? OnTurnStart;
        public event Action<IPlayer>? OnTurnEnd;
        public event Action<PhaseType>? OnPhaseStart;
        public event Action<PhaseType>? OnPhaseEnd;
        public event Action? OnCombatStart;
        public event Action? OnCombatEnd;
        public event Action<ICard, ZoneType, ZoneType>? OnZoneChange;
        public event Action<ICard>? OnCardPlayed;
        public event Action<ICard>? OnCardDestroyed;
        public event Action<IUnit, int>? OnDamageDealt;
        public event Action<IUnit>? OnUnitDefeated;

        public EventManager(GameState gameState)
        {
            _gameState = gameState;
        }

        /// <summary>
        /// Register a triggered ability to listen for events.
        /// </summary>
        public void RegisterTrigger(ITriggeredAbility trigger)
        {
            _registeredTriggers.Add(trigger);
        }

        /// <summary>
        /// Unregister a triggered ability.
        /// </summary>
        public void UnregisterTrigger(ITriggeredAbility trigger)
        {
            _registeredTriggers.Remove(trigger);
        }

        /// <summary>
        /// Check and queue triggered abilities for an event.
        /// </summary>
        private void CheckTriggers(IGameEvent gameEvent)
        {
            foreach (var trigger in _registeredTriggers.ToList())
            {
                if (trigger.ShouldTrigger(gameEvent))
                {
                    // Queue the trigger on the stack
                    var activation = new Zones.AbilityActivation(
                        trigger,
                        trigger.Source.Controller ?? _gameState.TurnPlayer
                    );
                    _gameState.EffectsStackZone.Push(activation);
                }
            }
        }

        public void TriggerGameStart()
        {
            OnGameStart?.Invoke(_gameState.TurnPlayer);
        }

        public void TriggerGameEnd(IPlayer winner)
        {
            OnGameEnd?.Invoke(winner);
        }

        public void TriggerTurnStart(IPlayer player)
        {
            OnTurnStart?.Invoke(player);
            CheckTriggers(new GameEvent(GameEventType.TurnStarted, null, player));
        }

        public void TriggerTurnEnd(IPlayer player)
        {
            OnTurnEnd?.Invoke(player);
            CheckTriggers(new GameEvent(GameEventType.TurnEnded, null, player));
        }

        public void TriggerPhaseStart(PhaseType phase)
        {
            OnPhaseStart?.Invoke(phase);
            CheckTriggers(new PhaseEvent(GameEventType.PhaseStarted, phase));
        }

        public void TriggerPhaseEnd(PhaseType phase)
        {
            OnPhaseEnd?.Invoke(phase);
            CheckTriggers(new PhaseEvent(GameEventType.PhaseEnded, phase));
        }

        public void TriggerCombatStart()
        {
            OnCombatStart?.Invoke();
            CheckTriggers(new GameEvent(GameEventType.CombatStarted, null, _gameState.TurnPlayer));
        }

        public void TriggerCombatEnd()
        {
            OnCombatEnd?.Invoke();
            CheckTriggers(new GameEvent(GameEventType.CombatEnded, null, _gameState.TurnPlayer));
        }

        public void TriggerZoneChange(ICard card, ZoneType fromZone, ZoneType toZone)
        {
            OnZoneChange?.Invoke(card, fromZone, toZone);
            CheckTriggers(new ZoneChangeEvent(card, fromZone, toZone));

            // Special triggers for entering/leaving field
            if (toZone == ZoneType.Field)
            {
                CheckTriggers(new GameEvent(GameEventType.CardEnteredZone, card, card.Controller));
            }
            if (fromZone == ZoneType.Field)
            {
                CheckTriggers(new GameEvent(GameEventType.CardLeftZone, card, card.Controller));
            }
        }

        public void TriggerCardPlayed(ICard card)
        {
            OnCardPlayed?.Invoke(card);
            CheckTriggers(new GameEvent(GameEventType.CardPlayed, card, card.Controller));
        }

        public void TriggerCardDestroyed(ICard card)
        {
            OnCardDestroyed?.Invoke(card);
            CheckTriggers(new GameEvent(GameEventType.CardDestroyed, card, card.Controller));
        }

        public void TriggerDamageDealt(IUnit target, int amount, ICard source, bool isCombatDamage)
        {
            OnDamageDealt?.Invoke(target, amount);
            CheckTriggers(new DamageEvent(source, target, amount, isCombatDamage));
        }

        public void TriggerUnitDefeated(IUnit unit)
        {
            OnUnitDefeated?.Invoke(unit);
            CheckTriggers(new GameEvent(GameEventType.UnitDefeated, unit as ICard, ((ICard)unit).Controller));
        }
    }

    /// <summary>
    /// Basic game event implementation.
    /// </summary>
    public class GameEvent : IGameEvent
    {
        public GameEventType EventType { get; }
        public object? Source { get; }
        public IPlayer? Player { get; }
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        public GameEvent(GameEventType eventType, object? source, IPlayer? player)
        {
            EventType = eventType;
            Source = source;
            Player = player;
        }
    }

    /// <summary>
    /// Phase-related event.
    /// </summary>
    public class PhaseEvent : GameEvent
    {
        public PhaseType Phase { get; }

        public PhaseEvent(GameEventType eventType, PhaseType phase)
            : base(eventType, null, null)
        {
            Phase = phase;
        }
    }

    /// <summary>
    /// Zone change event implementation.
    /// </summary>
    public class ZoneChangeEvent : GameEvent, IZoneChangeEvent
    {
        public ICard Card { get; }
        public ZoneType FromZone { get; }
        public ZoneType ToZone { get; }

        public ZoneChangeEvent(ICard card, ZoneType fromZone, ZoneType toZone)
            : base(GameEventType.CardEnteredZone, card, card.Controller)
        {
            Card = card;
            FromZone = fromZone;
            ToZone = toZone;
        }
    }

    /// <summary>
    /// Damage event implementation.
    /// </summary>
    public class DamageEvent : GameEvent, IDamageEvent
    {
        public ICard DamageSource { get; }
        public IUnit Target { get; }
        public int Amount { get; }
        public bool IsCombatDamage { get; }

        public DamageEvent(ICard source, IUnit target, int amount, bool isCombatDamage)
            : base(GameEventType.DamageDealt, source, source.Controller)
        {
            DamageSource = source;
            Target = target;
            Amount = amount;
            IsCombatDamage = isCombatDamage;
        }
    }
}
