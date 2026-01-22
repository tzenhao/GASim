using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;
using MTGOnline.Mana;

namespace MTGOnline.Abilities.Base
{
    /// <summary>
    /// Base class for costs.
    /// </summary>
    public abstract class Cost : ICost
    {
        public abstract bool CanPay(IPlayer player, IGameState gameState);
        public abstract void Pay(IPlayer player, IGameState gameState);
        public abstract string GetDescription();
    }

    /// <summary>
    /// Mana cost component.
    /// </summary>
    public class ManaCostComponent : Cost
    {
        public ManaCost ManaCost { get; }

        public ManaCostComponent(ManaCost manaCost)
        {
            ManaCost = manaCost;
        }

        public override bool CanPay(IPlayer player, IGameState gameState)
        {
            return player.ManaPool.CanPay(ManaCost);
        }

        public override void Pay(IPlayer player, IGameState gameState)
        {
            player.ManaPool.Pay(ManaCost);
        }

        public override string GetDescription() => ManaCost.ToString();
    }

    /// <summary>
    /// Tap cost component.
    /// </summary>
    public class TapCost : Cost
    {
        public IPermanent Permanent { get; }

        public TapCost(IPermanent permanent)
        {
            Permanent = permanent;
        }

        public override bool CanPay(IPlayer player, IGameState gameState)
        {
            return !Permanent.IsTapped;
        }

        public override void Pay(IPlayer player, IGameState gameState)
        {
            Permanent.Tap();
        }

        public override string GetDescription() => "{T}";
    }

    /// <summary>
    /// Life payment cost.
    /// </summary>
    public class PayLifeCost : Cost
    {
        public int Amount { get; }

        public PayLifeCost(int amount)
        {
            Amount = amount;
        }

        public override bool CanPay(IPlayer player, IGameState gameState)
        {
            return player.Life > Amount;  // Can't pay if it would kill you
        }

        public override void Pay(IPlayer player, IGameState gameState)
        {
            player.LoseLife(Amount);
        }

        public override string GetDescription() => $"Pay {Amount} life";
    }

    /// <summary>
    /// Sacrifice cost.
    /// </summary>
    public class SacrificeCost : Cost
    {
        public IPermanent? SpecificPermanent { get; }
        public Func<IPermanent, bool>? PermanentFilter { get; }
        public int Count { get; }

        public SacrificeCost(IPermanent permanent)
        {
            SpecificPermanent = permanent;
            Count = 1;
        }

        public SacrificeCost(Func<IPermanent, bool> filter, int count = 1)
        {
            PermanentFilter = filter;
            Count = count;
        }

        public override bool CanPay(IPlayer player, IGameState gameState)
        {
            if (SpecificPermanent != null)
            {
                return gameState.Battlefield.Permanents.Contains(SpecificPermanent) &&
                       SpecificPermanent.Controller == player;
            }

            if (PermanentFilter != null)
            {
                return gameState.Battlefield.GetPermanentsControlledBy(player)
                    .Count(PermanentFilter) >= Count;
            }

            return false;
        }

        public override void Pay(IPlayer player, IGameState gameState)
        {
            // In full implementation, would move to graveyard
        }

        public override string GetDescription() => "Sacrifice a permanent";
    }

    /// <summary>
    /// Discard cost.
    /// </summary>
    public class DiscardCost : Cost
    {
        public int Count { get; }
        public CardType? RequiredType { get; }

        public DiscardCost(int count = 1, CardType? requiredType = null)
        {
            Count = count;
            RequiredType = requiredType;
        }

        public override bool CanPay(IPlayer player, IGameState gameState)
        {
            if (RequiredType.HasValue)
            {
                return player.Hand.Cards.Count(c => c.HasCardType(RequiredType.Value)) >= Count;
            }
            return player.Hand.Count >= Count;
        }

        public override void Pay(IPlayer player, IGameState gameState)
        {
            // In full implementation, player would choose cards
            player.DiscardCards(Count);
        }

        public override string GetDescription() => Count == 1 ? "Discard a card" : $"Discard {Count} cards";
    }

    /// <summary>
    /// Composite cost (multiple costs combined).
    /// </summary>
    public class CompositeCost : Cost
    {
        private readonly List<ICost> _costs = new();

        public void AddCost(ICost cost)
        {
            _costs.Add(cost);
        }

        public override bool CanPay(IPlayer player, IGameState gameState)
        {
            return _costs.All(c => c.CanPay(player, gameState));
        }

        public override void Pay(IPlayer player, IGameState gameState)
        {
            foreach (var cost in _costs)
            {
                cost.Pay(player, gameState);
            }
        }

        public override string GetDescription()
        {
            return string.Join(", ", _costs.Select(c => c.GetDescription()));
        }
    }
}
