using MTGOnline.Core.Enums;
using MTGOnline.Cards.Base;
using MTGOnline.Interfaces;

namespace MTGOnline.Cards.Types
{
    /// <summary>
    /// Represents a sorcery card in Magic: The Gathering.
    /// </summary>
    public class SorceryCard : Card, ISpell
    {
        public IPlayer Controller { get; set; } = null!;

        private readonly List<ITarget> _targets = new();
        public IReadOnlyList<ITarget> Targets => _targets.AsReadOnly();

        public bool IsOnStack { get; private set; }

        public SorceryCard(string name) : base(name)
        {
            CardTypes = CardType.Sorcery;
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

        public bool CanCast(IGameState gameState)
        {
            // Sorceries can only be cast during your main phase when the stack is empty
            return gameState.Stack.IsEmpty &&
                   gameState.ActivePlayer == Controller &&
                   (gameState.CurrentPhase == PhaseType.PreCombatMain ||
                    gameState.CurrentPhase == PhaseType.PostCombatMain);
        }
    }
}
