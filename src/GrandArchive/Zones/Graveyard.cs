using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Zones
{
    /// <summary>
    /// Graveyard zone in Grand Archive TCG.
    /// Where destroyed/used cards go.
    /// Public zone - cards are face-up.
    /// </summary>
    public class Graveyard : Zone
    {
        public override ZoneType ZoneType => ZoneType.Graveyard;

        public override bool IsPublic => true;

        public override bool IsOrdered => false;

        public override void Add(ICard card)
        {
            base.Add(card);
            // Graveyard cards are always face-up
            card.Visibility = CardVisibility.FaceUp;
        }

        /// <summary>
        /// Get all cards of a specific type in the graveyard.
        /// </summary>
        public IEnumerable<ICard> GetCardsOfType(CardType cardType)
        {
            return _cards.Where(c => c.CardType == cardType);
        }

        /// <summary>
        /// Get all cards of a specific element in the graveyard.
        /// </summary>
        public IEnumerable<ICard> GetCardsOfElement(Element element)
        {
            return _cards.Where(c => c.Element == element);
        }

        /// <summary>
        /// Retrieve a card from the graveyard (for recursion effects).
        /// </summary>
        public ICard? Retrieve(ICard card)
        {
            if (Remove(card))
            {
                return card;
            }
            return null;
        }

        /// <summary>
        /// Banish a card from the graveyard.
        /// </summary>
        public ICard? Banish(ICard card)
        {
            if (Remove(card))
            {
                return card;
            }
            return null;
        }

        /// <summary>
        /// Count cards of a specific type.
        /// </summary>
        public int CountCardsOfType(CardType cardType)
        {
            return _cards.Count(c => c.CardType == cardType);
        }

        public override string ToString() => $"Graveyard ({Count} cards)";
    }
}
