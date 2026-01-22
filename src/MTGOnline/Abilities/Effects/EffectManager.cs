using MTGOnline.Interfaces;

namespace MTGOnline.Abilities.Effects
{
    /// <summary>
    /// Manages continuous effects and their application.
    /// </summary>
    public class EffectManager : IEffectManager
    {
        private readonly List<IContinuousEffect> _activeEffects = new();
        public IReadOnlyList<IContinuousEffect> ActiveEffects => _activeEffects.AsReadOnly();

        private readonly List<IReplacementEffect> _replacementEffects = new();
        private readonly List<IPreventionEffect> _preventionEffects = new();

        public void AddEffect(IContinuousEffect effect)
        {
            _activeEffects.Add(effect);
        }

        public void RemoveEffect(IContinuousEffect effect)
        {
            _activeEffects.Remove(effect);
        }

        public void AddReplacementEffect(IReplacementEffect effect)
        {
            _replacementEffects.Add(effect);
        }

        public void RemoveReplacementEffect(IReplacementEffect effect)
        {
            _replacementEffects.Remove(effect);
        }

        public void AddPreventionEffect(IPreventionEffect effect)
        {
            _preventionEffects.Add(effect);
        }

        public void RemovePreventionEffect(IPreventionEffect effect)
        {
            _preventionEffects.Remove(effect);
        }

        /// <summary>
        /// Applies layered effects according to MTG rules.
        /// </summary>
        public void ApplyLayeredEffects(IGameState gameState)
        {
            // Sort effects by layer, then by timestamp within each layer
            var sortedEffects = _activeEffects
                .OrderBy(e => e.Layer)
                .ThenBy(e => e.Timestamp)
                .ToList();

            // Apply each effect in order
            foreach (var effect in sortedEffects)
            {
                if (!effect.IsExpired)
                {
                    effect.Apply(gameState);
                }
            }
        }

        /// <summary>
        /// Removes expired effects.
        /// </summary>
        public void CleanupExpiredEffects()
        {
            var expiredEffects = _activeEffects.Where(e => e.IsExpired).ToList();
            foreach (var effect in expiredEffects)
            {
                _activeEffects.Remove(effect);
            }

            // Also cleanup replacement and prevention effects
            _replacementEffects.RemoveAll(e => e is IContinuousEffect ce && ce.IsExpired);
            _preventionEffects.RemoveAll(e => e is IContinuousEffect ce && ce.IsExpired);
        }

        /// <summary>
        /// Applies replacement effects to a game event.
        /// </summary>
        public IGameEvent ApplyReplacementEffects(IGameEvent gameEvent, IGameState gameState)
        {
            var applicableEffects = _replacementEffects
                .Where(e => e.CanReplace(gameEvent, gameState))
                .ToList();

            if (applicableEffects.Count == 0)
            {
                return gameEvent;
            }

            // If multiple replacement effects could apply, affected player/controller chooses
            // For simplicity, just apply the first one
            return applicableEffects[0].Replace(gameEvent, gameState);
        }

        /// <summary>
        /// Applies prevention effects to damage.
        /// </summary>
        public int ApplyPreventionEffects(IDamageEvent damageEvent, int damage, IGameState gameState)
        {
            if (!damageEvent.IsPreventable)
            {
                return damage;
            }

            var applicableEffects = _preventionEffects
                .Where(e => e.CanPrevent(damageEvent, gameState))
                .ToList();

            int remainingDamage = damage;

            foreach (var effect in applicableEffects)
            {
                if (remainingDamage <= 0) break;
                remainingDamage = effect.PreventDamage(damageEvent, remainingDamage, gameState);
            }

            return Math.Max(0, remainingDamage);
        }
    }
}
