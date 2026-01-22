using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;
using MTGOnline.Abilities.Base;

namespace MTGOnline.Abilities.Types
{
    /// <summary>
    /// Represents a planeswalker loyalty ability.
    /// </summary>
    public class LoyaltyAbility : Ability, ILoyaltyAbility
    {
        public override AbilityType AbilityType => AbilityType.Loyalty;

        public int LoyaltyCost { get; }  // Positive for +, negative for -, zero for 0

        public ICost ActivationCost => _loyaltyCost;
        private readonly LoyaltyCostComponent _loyaltyCost;

        public TimingRestriction TimingRestriction => TimingRestriction.Sorcery;

        private readonly List<ITarget> _targets = new();
        public IReadOnlyList<ITarget> Targets => _targets.AsReadOnly();

        public Action<IGameState, IReadOnlyList<ITarget>>? Effect { get; set; }

        private readonly IPlaneswalker _planeswalker;

        public LoyaltyAbility(IPlaneswalker planeswalker, IPlayer controller, int loyaltyCost, string rulesText = "")
            : base(planeswalker as ICard ?? throw new ArgumentException("Planeswalker must be a card"), controller, rulesText)
        {
            _planeswalker = planeswalker;
            LoyaltyCost = loyaltyCost;
            _loyaltyCost = new LoyaltyCostComponent(planeswalker, loyaltyCost);
        }

        public override bool CanActivate(IGameState gameState)
        {
            // Sorcery speed only
            if (gameState.ActivePlayer != Controller ||
                !gameState.Stack.IsEmpty ||
                (gameState.CurrentPhase != PhaseType.PreCombatMain &&
                 gameState.CurrentPhase != PhaseType.PostCombatMain))
                return false;

            // Can only activate one loyalty ability per turn
            if (_planeswalker.HasActivatedLoyaltyThisTurn) return false;

            // Check if cost can be paid
            return _loyaltyCost.CanPay(Controller, gameState);
        }

        public void Activate(IGameState gameState, IReadOnlyList<ITarget> targets)
        {
            if (!CanActivate(gameState)) return;

            // Pay the cost (add or remove loyalty)
            _loyaltyCost.Pay(Controller, gameState);

            // Add targets
            _targets.Clear();
            _targets.AddRange(targets);

            // Put ability on stack
            var stackObject = new AbilityStackObject(this, Controller, targets);
            gameState.Stack.Push(stackObject);
        }

        public override void Resolve(IGameState gameState)
        {
            Effect?.Invoke(gameState, _targets);
        }
    }

    /// <summary>
    /// Cost component for loyalty abilities.
    /// </summary>
    public class LoyaltyCostComponent : Cost
    {
        private readonly IPlaneswalker _planeswalker;
        private readonly int _loyaltyCost;

        public LoyaltyCostComponent(IPlaneswalker planeswalker, int loyaltyCost)
        {
            _planeswalker = planeswalker;
            _loyaltyCost = loyaltyCost;
        }

        public override bool CanPay(IPlayer player, IGameState gameState)
        {
            // For negative costs, need enough loyalty
            if (_loyaltyCost < 0)
            {
                return _planeswalker.CurrentLoyalty >= Math.Abs(_loyaltyCost);
            }
            // Positive and zero costs can always be paid
            return true;
        }

        public override void Pay(IPlayer player, IGameState gameState)
        {
            if (_loyaltyCost > 0)
            {
                _planeswalker.AddLoyalty(_loyaltyCost);
            }
            else if (_loyaltyCost < 0)
            {
                _planeswalker.RemoveLoyalty(Math.Abs(_loyaltyCost));
            }

            // Mark that a loyalty ability was activated this turn
            if (_planeswalker is Cards.Types.PlaneswalkerCard pw)
            {
                // This would be handled by the planeswalker's state
            }
        }

        public override string GetDescription()
        {
            if (_loyaltyCost > 0) return $"+{_loyaltyCost}";
            if (_loyaltyCost < 0) return $"{_loyaltyCost}";
            return "0";
        }
    }
}
