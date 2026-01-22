using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;
using MTGOnline.Abilities.Base;

namespace MTGOnline.Abilities.Types
{
    /// <summary>
    /// Represents a static ability (continuous effect).
    /// </summary>
    public class StaticAbility : Ability, IStaticAbility
    {
        public override AbilityType AbilityType => AbilityType.Static;

        private readonly List<IContinuousEffect> _effects = new();
        public IReadOnlyList<IContinuousEffect> Effects => _effects.AsReadOnly();

        public StaticAbility(ICard source, IPlayer controller, string rulesText = "")
            : base(source, controller, rulesText)
        {
        }

        public void AddEffect(IContinuousEffect effect)
        {
            _effects.Add(effect);
        }

        public override bool CanActivate(IGameState gameState)
        {
            // Static abilities are always "active" while the source is on the battlefield
            return true;
        }

        public void Apply(IGameState gameState)
        {
            foreach (var effect in _effects)
            {
                effect.Apply(gameState);
            }
        }

        public void Remove(IGameState gameState)
        {
            foreach (var effect in _effects)
            {
                effect.Remove(gameState);
            }
        }

        public override void Resolve(IGameState gameState)
        {
            // Static abilities don't resolve - they just apply their effects continuously
        }
    }
}
