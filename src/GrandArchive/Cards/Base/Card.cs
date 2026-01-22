using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Cards.Base
{
    /// <summary>
    /// Base class for all cards in Grand Archive TCG.
    /// Cards consist of Name, Cost, Type, Class, Element, Speed, Power, Life, Durability, and Rules Text.
    /// </summary>
    public abstract class Card : ICard
    {
        public Guid Id { get; } = Guid.NewGuid();

        public string Name { get; init; } = string.Empty;

        public Element Element { get; init; } = Element.Norm;

        public abstract CardType CardType { get; }

        public Supertype Supertype { get; init; } = Supertype.None;

        public IReadOnlyList<Subtype> Subtypes { get; init; } = Array.Empty<Subtype>();

        public int Cost { get; init; }

        public CostType CostType { get; init; } = CostType.Reserve;

        public SpeedType Speed { get; init; } = SpeedType.Standard;

        public bool IsExalted { get; init; }

        public string RulesText { get; init; } = string.Empty;

        public Keyword Keywords { get; init; } = Keyword.None;

        public ZoneType CurrentZone { get; set; } = ZoneType.MainDeck;

        public CardVisibility Visibility { get; set; } = CardVisibility.FaceDown;

        public IPlayer? Owner { get; set; }

        public IPlayer? Controller { get; set; }

        /// <summary>
        /// List of abilities on this card.
        /// </summary>
        public List<IAbility> Abilities { get; init; } = new();

        /// <summary>
        /// Check if this card has a specific keyword.
        /// </summary>
        public bool HasKeyword(Keyword keyword) => (Keywords & keyword) == keyword;

        /// <summary>
        /// Check if this card has a specific subtype.
        /// </summary>
        public bool HasSubtype(Subtype subtype) => Subtypes.Contains(subtype);

        /// <summary>
        /// Check if this card can be played at the current game state.
        /// </summary>
        public virtual bool CanPlay(IGameState gameState)
        {
            // Base implementation - can be overridden by specific card types
            if (Controller == null) return false;

            // Check if player has opportunity
            if (!gameState.CanPlayerAct(Controller)) return false;

            // Check speed restrictions
            if (Speed == SpeedType.Standard && gameState.CurrentPhase != PhaseType.Main)
                return false;

            return true;
        }

        public override string ToString() => $"{Name} ({CardType}, {Element})";
    }
}
