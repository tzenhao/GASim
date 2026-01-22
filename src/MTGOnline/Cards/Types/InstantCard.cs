using MTGOnline.Core.Enums;
using MTGOnline.Cards.Base;
using MTGOnline.Interfaces;

namespace MTGOnline.Cards.Types
{
    /// <summary>
    /// Represents an instant card in Magic: The Gathering.
    /// </summary>
    public class InstantCard : Card, ISpell
    {
        public IPlayer Controller { get; set; } = null!;

        private readonly List<ITarget> _targets = new();
        public IReadOnlyList<ITarget> Targets => _targets.AsReadOnly();

        public bool IsOnStack { get; private set; }

        // Optional keyword abilities
        public bool HasFlash => true; // Instants can always be cast at instant speed

        public InstantCard(string name) : base(name)
        {
            CardTypes = CardType.Instant;
        }

        public void AddTarget(ITarget target)
        {
            _targets.Add(target);
        }

        public void ClearTargets()
        {
            _targets.Clear();
        }

        public void PutOnStack()
        {
            IsOnStack = true;
        }

        public void RemoveFromStack()
        {
            IsOnStack = false;
        }

        public virtual void Resolve()
        {
            IsOnStack = false;
            // Override in specific cards to implement effect
        }

        public void Counter()
        {
            IsOnStack = false;
            // Card goes to graveyard when countered
        }
    }
}
