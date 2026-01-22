using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;

namespace MTGOnline.Abilities.Base
{
    /// <summary>
    /// Base class for all abilities.
    /// </summary>
    public abstract class Ability : IAbility
    {
        public Guid Id { get; }
        public abstract AbilityType AbilityType { get; }
        public ICard Source { get; }
        public IPlayer Controller { get; protected set; }
        public string RulesText { get; protected set; }

        protected Ability(ICard source, IPlayer controller, string rulesText = "")
        {
            Id = Guid.NewGuid();
            Source = source;
            Controller = controller;
            RulesText = rulesText;
        }

        public abstract bool CanActivate(IGameState gameState);
        public abstract void Resolve(IGameState gameState);
    }
}
