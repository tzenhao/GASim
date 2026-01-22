using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;

namespace MTGOnline.Stack
{
    /// <summary>
    /// Represents a spell on the stack.
    /// </summary>
    public class SpellStackObject : IStackObject
    {
        public Guid Id { get; }
        public IPlayer Controller { get; }
        public ICard Source { get; }

        private readonly List<ITarget> _targets;
        public IReadOnlyList<ITarget> Targets => _targets.AsReadOnly();

        public bool IsSpell => true;
        public bool IsAbility => false;

        private readonly ISpell _spell;

        public SpellStackObject(ISpell spell, IPlayer controller, IReadOnlyList<ITarget>? targets = null)
        {
            Id = Guid.NewGuid();
            _spell = spell;
            Controller = controller;
            Source = spell as ICard ?? throw new InvalidOperationException("Spell must be a card");
            _targets = targets?.ToList() ?? new List<ITarget>();
        }

        public bool CanResolve(IGameState gameState)
        {
            // Check if all targets are still legal
            return HasLegalTargets(gameState);
        }

        public void Resolve(IGameState gameState)
        {
            if (CanResolve(gameState))
            {
                _spell.Resolve();

                // After resolution, spell goes to graveyard (for instants/sorceries)
                if (Source.HasCardType(CardType.Instant) || Source.HasCardType(CardType.Sorcery))
                {
                    Controller.Graveyard.Add(Source);
                }
                // Permanents would enter the battlefield instead
                else if (Source is IPermanent permanent)
                {
                    // Move to battlefield
                    permanent.Controller = Controller;
                    permanent.Owner = Controller;
                    gameState.Battlefield.AddPermanent(permanent);
                    permanent.OnEnterBattlefield(gameState);
                }
            }
            else
            {
                // Spell fizzles - goes to graveyard without effect
                Counter(gameState);
            }
        }

        public void Counter(IGameState gameState)
        {
            // Countered spell goes to graveyard
            Controller.Graveyard.Add(Source);
        }

        public bool HasLegalTargets(IGameState gameState)
        {
            // If spell has no targets, it's always legal
            if (_targets.Count == 0) return true;

            // At least one target must be legal (for most spells)
            // Some spells require ALL targets to be legal
            return _targets.Any(t => t.IsLegalTarget(gameState));
        }
    }

    /// <summary>
    /// Factory for creating spell stack objects.
    /// </summary>
    public static class SpellFactory
    {
        public static SpellStackObject CastSpell(
            ISpell spell,
            IPlayer controller,
            IGameState gameState,
            IReadOnlyList<ITarget>? targets = null)
        {
            var stackObject = new SpellStackObject(spell, controller, targets);

            // Put on stack
            gameState.Stack.Push(stackObject);

            // Raise cast event
            gameState.EventManager.RaiseEvent(
                new Events.CastSpellEvent(spell, controller, targets));

            return stackObject;
        }
    }
}
