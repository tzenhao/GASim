using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Zones
{
    /// <summary>
    /// Main Deck zone in Grand Archive TCG.
    /// Contains the player's main deck of cards.
    /// Private zone - cards are face-down.
    /// Order cannot be changed by players.
    /// </summary>
    public class MainDeck : Zone
    {
        public override ZoneType ZoneType => ZoneType.MainDeck;

        public override bool IsPublic => false;

        /// <summary>
        /// Main deck order is fixed (can't be rearranged by players).
        /// </summary>
        public override bool IsOrdered => true;

        /// <summary>
        /// Draw a card from the top of the deck.
        /// </summary>
        public ICard? Draw()
        {
            return DrawTop();
        }

        /// <summary>
        /// Draw multiple cards from the top of the deck.
        /// </summary>
        public List<ICard> Draw(int count)
        {
            var drawn = new List<ICard>();
            for (int i = 0; i < count && _cards.Count > 0; i++)
            {
                var card = DrawTop();
                if (card != null)
                {
                    drawn.Add(card);
                }
            }
            return drawn;
        }

        /// <summary>
        /// Glimpse - look at the top N cards of the deck.
        /// </summary>
        public IReadOnlyList<ICard> Glimpse(int count)
        {
            return PeekTop(count);
        }

        /// <summary>
        /// Put a card on top of the deck.
        /// </summary>
        public void PutOnTop(ICard card)
        {
            AddToTop(card);
        }

        /// <summary>
        /// Put a card on the bottom of the deck.
        /// </summary>
        public void PutOnBottom(ICard card)
        {
            AddToBottom(card);
        }

        /// <summary>
        /// Search the deck for cards matching a predicate.
        /// Note: This reveals the deck contents to the searching player.
        /// </summary>
        public IEnumerable<ICard> Search(Func<ICard, bool> predicate)
        {
            return _cards.Where(predicate);
        }

        /// <summary>
        /// Check if the deck is empty (player would lose if they need to draw).
        /// </summary>
        public bool IsEmpty => _cards.Count == 0;

        public override string ToString() => $"Main Deck ({Count} cards)";
    }
}
