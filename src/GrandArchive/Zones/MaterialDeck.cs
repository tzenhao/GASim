using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Zones
{
    /// <summary>
    /// Material Deck zone in Grand Archive TCG.
    /// Contains champion, regalia, and other material cards.
    /// Public zone - cards are face-up.
    /// </summary>
    public class MaterialDeck : Zone
    {
        public override ZoneType ZoneType => ZoneType.MaterialDeck;

        public override bool IsPublic => true;

        public override bool IsOrdered => false;

        public override void Add(ICard card)
        {
            base.Add(card);
            // Material deck cards are always face-up
            card.Visibility = CardVisibility.FaceUp;
        }

        /// <summary>
        /// Get the champion card from the material deck.
        /// </summary>
        public ICard? GetChampion()
        {
            return _cards.FirstOrDefault(c => c.CardType == CardType.Champion);
        }

        /// <summary>
        /// Get all regalia cards from the material deck.
        /// </summary>
        public IEnumerable<ICard> GetRegalias()
        {
            return _cards.Where(c => c.CardType == CardType.Regalia);
        }

        /// <summary>
        /// Materialize a card from the material deck (move to field).
        /// </summary>
        public ICard? Materialize(Func<ICard, bool> predicate)
        {
            var card = _cards.FirstOrDefault(predicate);
            if (card != null)
            {
                Remove(card);
            }
            return card;
        }

        /// <summary>
        /// Materialize a specific card from the material deck.
        /// </summary>
        public bool Materialize(ICard card)
        {
            return Remove(card);
        }

        public override string ToString() => $"Material Deck ({Count} cards)";
    }
}
