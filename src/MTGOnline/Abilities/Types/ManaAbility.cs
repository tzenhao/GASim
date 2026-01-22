using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;
using MTGOnline.Abilities.Base;
using MTGOnline.Mana;

namespace MTGOnline.Abilities.Types
{
    /// <summary>
    /// Represents a mana ability (doesn't use the stack).
    /// </summary>
    public class ManaAbility : Ability, IManaAbility
    {
        public override AbilityType AbilityType => AbilityType.Mana;

        public ICost ActivationCost { get; }
        public TimingRestriction TimingRestriction => TimingRestriction.Instant;
        public ManaOutput ManaProduced { get; }

        private readonly List<ITarget> _targets = new();
        public IReadOnlyList<ITarget> Targets => _targets.AsReadOnly();

        public ManaAbility(ICard source, IPlayer controller, ICost activationCost, ManaOutput manaProduced, string rulesText = "")
            : base(source, controller, rulesText)
        {
            ActivationCost = activationCost;
            ManaProduced = manaProduced;
        }

        public override bool CanActivate(IGameState gameState)
        {
            return ActivationCost.CanPay(Controller, gameState);
        }

        public void Activate(IGameState gameState, IReadOnlyList<ITarget> targets)
        {
            if (!CanActivate(gameState)) return;

            // Pay the cost
            ActivationCost.Pay(Controller, gameState);

            // Mana abilities don't use the stack - resolve immediately
            Resolve(gameState);
        }

        public override void Resolve(IGameState gameState)
        {
            // Add mana to controller's pool
            Controller.ManaPool.Add(ManaProduced);
        }
    }

    /// <summary>
    /// Factory for creating common mana abilities.
    /// </summary>
    public static class ManaAbilityFactory
    {
        public static ManaAbility CreateBasicLandAbility(ILand land, IPlayer controller, ManaColor color)
        {
            var tapCost = new TapCost(land);
            var manaOutput = new ManaOutput(color, 1);

            return new ManaAbility(land, controller, tapCost, manaOutput, $"{{T}}: Add {{{GetManaSymbol(color)}}}.");
        }

        private static string GetManaSymbol(ManaColor color) => color switch
        {
            ManaColor.White => "W",
            ManaColor.Blue => "U",
            ManaColor.Black => "B",
            ManaColor.Red => "R",
            ManaColor.Green => "G",
            ManaColor.Colorless => "C",
            _ => "?"
        };
    }
}
