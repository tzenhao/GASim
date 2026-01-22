using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Zones
{
    /// <summary>
    /// Banishment zone in Grand Archive TCG.
    /// Cards removed from the game.
    /// Public zone - cards are typically face-up.
    /// </summary>
    public class Banishment : Zone
    {
        public override ZoneType ZoneType => ZoneType.Banishment;

        public override bool IsPublic => true;

        public override bool IsOrdered => false;

        public override void Add(ICard card)
        {
            base.Add(card);
            // Banished cards are typically face-up
            card.Visibility = CardVisibility.FaceUp;
        }

        /// <summary>
        /// Banish a card face-down (hidden from all players).
        /// </summary>
        public void BanishFaceDown(ICard card)
        {
            card.CurrentZone = ZoneType;
            card.Visibility = CardVisibility.FaceDown;
            _cards.Add(card);
        }

        /// <summary>
        /// Get all face-up banished cards.
        /// </summary>
        public IEnumerable<ICard> GetFaceUpCards()
        {
            return _cards.Where(c => c.Visibility == CardVisibility.FaceUp);
        }

        /// <summary>
        /// Get all face-down banished cards.
        /// </summary>
        public IEnumerable<ICard> GetFaceDownCards()
        {
            return _cards.Where(c => c.Visibility == CardVisibility.FaceDown);
        }

        /// <summary>
        /// Return a banished card to another zone (rare effect).
        /// </summary>
        public ICard? Return(ICard card)
        {
            if (Remove(card))
            {
                return card;
            }
            return null;
        }

        public override string ToString() => $"Banishment ({Count} cards)";
    }
}
