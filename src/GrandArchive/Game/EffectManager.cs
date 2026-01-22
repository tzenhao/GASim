using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Game
{
    /// <summary>
    /// Manages continuous effects in Grand Archive TCG.
    /// </summary>
    public class EffectManager
    {
        private readonly GameState _gameState;
        private readonly List<ContinuousEffect> _continuousEffects = new();
        private readonly List<ContinuousEffect> _untilEndOfTurnEffects = new();

        public EffectManager(GameState gameState)
        {
            _gameState = gameState;
        }

        /// <summary>
        /// Register a continuous effect.
        /// </summary>
        public void RegisterEffect(ContinuousEffect effect)
        {
            _continuousEffects.Add(effect);

            if (effect.Duration == EffectDuration.UntilEndOfTurn)
            {
                _untilEndOfTurnEffects.Add(effect);
            }
        }

        /// <summary>
        /// Remove a continuous effect.
        /// </summary>
        public void RemoveEffect(ContinuousEffect effect)
        {
            _continuousEffects.Remove(effect);
            _untilEndOfTurnEffects.Remove(effect);
        }

        /// <summary>
        /// Process end of turn effects (remove "until end of turn" effects).
        /// </summary>
        public void ProcessEndOfTurnEffects()
        {
            foreach (var effect in _untilEndOfTurnEffects.ToList())
            {
                effect.Unapply(_gameState);
                RemoveEffect(effect);
            }
        }

        /// <summary>
        /// Apply all active continuous effects.
        /// </summary>
        public void ApplyAllEffects()
        {
            foreach (var effect in _continuousEffects)
            {
                if (effect.IsActive(_gameState))
                {
                    effect.Apply(_gameState);
                }
            }
        }

        /// <summary>
        /// Get all effects affecting a specific card.
        /// </summary>
        public IEnumerable<ContinuousEffect> GetEffectsAffecting(ICard card)
        {
            return _continuousEffects.Where(e => e.AffectsCard(card));
        }

        /// <summary>
        /// Calculate modified power for a unit.
        /// </summary>
        public int GetModifiedPower(IUnit unit)
        {
            int power = unit.BasePower + unit.BuffCounters;

            foreach (var effect in _continuousEffects)
            {
                if (effect.AffectsCard((ICard)unit) && effect.PowerModifier != 0)
                {
                    power += effect.PowerModifier;
                }
            }

            return Math.Max(0, power);
        }

        /// <summary>
        /// Calculate modified life for a unit.
        /// </summary>
        public int GetModifiedLife(IUnit unit)
        {
            int life = unit.BaseLife + unit.BuffCounters;

            foreach (var effect in _continuousEffects)
            {
                if (effect.AffectsCard((ICard)unit) && effect.LifeModifier != 0)
                {
                    life += effect.LifeModifier;
                }
            }

            return Math.Max(1, life);
        }
    }

    /// <summary>
    /// Represents a continuous effect in Grand Archive TCG.
    /// </summary>
    public class ContinuousEffect
    {
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// The source of this effect.
        /// </summary>
        public ICard Source { get; init; } = null!;

        /// <summary>
        /// Duration of this effect.
        /// </summary>
        public EffectDuration Duration { get; init; }

        /// <summary>
        /// Power modifier applied by this effect.
        /// </summary>
        public int PowerModifier { get; init; }

        /// <summary>
        /// Life modifier applied by this effect.
        /// </summary>
        public int LifeModifier { get; init; }

        /// <summary>
        /// Keywords granted by this effect.
        /// </summary>
        public Keyword GrantedKeywords { get; init; }

        /// <summary>
        /// Condition for this effect to be active.
        /// </summary>
        public Func<IGameState, bool>? Condition { get; init; }

        /// <summary>
        /// Filter for which cards this effect affects.
        /// </summary>
        public Func<ICard, bool>? AffectedCardsFilter { get; init; }

        /// <summary>
        /// Custom apply action.
        /// </summary>
        public Action<IGameState>? ApplyAction { get; init; }

        /// <summary>
        /// Custom unapply action.
        /// </summary>
        public Action<IGameState>? UnapplyAction { get; init; }

        /// <summary>
        /// Check if this effect is currently active.
        /// </summary>
        public bool IsActive(IGameState gameState)
        {
            return Condition?.Invoke(gameState) ?? true;
        }

        /// <summary>
        /// Check if this effect affects a specific card.
        /// </summary>
        public bool AffectsCard(ICard card)
        {
            return AffectedCardsFilter?.Invoke(card) ?? false;
        }

        /// <summary>
        /// Apply this effect.
        /// </summary>
        public void Apply(IGameState gameState)
        {
            ApplyAction?.Invoke(gameState);
        }

        /// <summary>
        /// Unapply this effect.
        /// </summary>
        public void Unapply(IGameState gameState)
        {
            UnapplyAction?.Invoke(gameState);
        }
    }

    /// <summary>
    /// Duration types for continuous effects.
    /// </summary>
    public enum EffectDuration
    {
        /// <summary>Effect lasts until end of turn</summary>
        UntilEndOfTurn,

        /// <summary>Effect lasts until source leaves field</summary>
        WhileSourceOnField,

        /// <summary>Effect lasts until a condition is met</summary>
        UntilCondition,

        /// <summary>Effect is permanent (until removed)</summary>
        Permanent
    }
}
