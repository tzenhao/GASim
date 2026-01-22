using MTGOnline.Core.Enums;
using MTGOnline.Cards.Base;
using MTGOnline.Interfaces;

namespace MTGOnline.Cards.Types
{
    /// <summary>
    /// Represents a planeswalker card in Magic: The Gathering.
    /// </summary>
    public class PlaneswalkerCard : Permanent, IPlaneswalker
    {
        public int StartingLoyalty { get; set; }
        public int CurrentLoyalty { get; private set; }

        private readonly List<ILoyaltyAbility> _loyaltyAbilities = new();
        public IReadOnlyList<ILoyaltyAbility> LoyaltyAbilities => _loyaltyAbilities.AsReadOnly();

        public bool HasActivatedLoyaltyThisTurn { get; private set; }

        public PlaneswalkerCard(string name, int startingLoyalty) : base(name)
        {
            CardTypes = CardType.Planeswalker;
            SuperTypes = SuperType.Legendary; // Most planeswalkers are legendary
            StartingLoyalty = startingLoyalty;
            CurrentLoyalty = startingLoyalty;
        }

        public void AddLoyalty(int amount)
        {
            if (amount > 0)
            {
                CurrentLoyalty += amount;
            }
        }

        public void RemoveLoyalty(int amount)
        {
            if (amount > 0)
            {
                CurrentLoyalty = Math.Max(0, CurrentLoyalty - amount);
            }
        }

        public void DealDamage(int amount)
        {
            RemoveLoyalty(amount);
        }

        public void AddLoyaltyAbility(ILoyaltyAbility ability)
        {
            _loyaltyAbilities.Add(ability);
        }

        public void ActivateLoyaltyAbility(ILoyaltyAbility ability, IGameState gameState)
        {
            if (HasActivatedLoyaltyThisTurn) return;

            int cost = ability.LoyaltyCost;
            if (cost > 0)
            {
                AddLoyalty(cost);
            }
            else if (cost < 0)
            {
                if (CurrentLoyalty < Math.Abs(cost)) return;
                RemoveLoyalty(Math.Abs(cost));
            }

            HasActivatedLoyaltyThisTurn = true;
        }

        public void ResetLoyaltyActivation()
        {
            HasActivatedLoyaltyThisTurn = false;
        }

        public override void OnEnterBattlefield(IGameState gameState)
        {
            base.OnEnterBattlefield(gameState);
            CurrentLoyalty = StartingLoyalty;
        }

        public override string ToString()
        {
            return $"{Name} [Loyalty: {CurrentLoyalty}]";
        }
    }
}
