using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;

namespace MTGOnline.Zones
{
    /// <summary>
    /// Represents a player's library (deck).
    /// </summary>
    public class Library : Zone
    {
        public Library(IPlayer owner) : base(ZoneType.Library, ZoneVisibility.Hidden, owner)
        {
        }

        /// <summary>
        /// Draws a card from the top of the library.
        /// </summary>
        public ICard? Draw()
        {
            return RemoveFromTop();
        }

        /// <summary>
        /// Draws multiple cards from the top of the library.
        /// </summary>
        public IReadOnlyList<ICard> Draw(int count)
        {
            var drawn = new List<ICard>();
            for (int i = 0; i < count && !IsEmpty; i++)
            {
                var card = Draw();
                if (card != null)
                {
                    drawn.Add(card);
                }
            }
            return drawn.AsReadOnly();
        }

        /// <summary>
        /// Reveals the top card without removing it.
        /// </summary>
        public ICard? RevealTop()
        {
            return Peek();
        }

        /// <summary>
        /// Reveals the top N cards without removing them.
        /// </summary>
        public IReadOnlyList<ICard> RevealTop(int count)
        {
            return PeekTop(count);
        }

        /// <summary>
        /// Searches the library for a card matching the predicate.
        /// </summary>
        public IReadOnlyList<ICard> Search(Func<ICard, bool> predicate)
        {
            return GetAll(predicate);
        }

        /// <summary>
        /// Puts a card on top of the library.
        /// </summary>
        public void PutOnTop(ICard card)
        {
            AddToTop(card);
        }

        /// <summary>
        /// Puts a card on the bottom of the library.
        /// </summary>
        public void PutOnBottom(ICard card)
        {
            AddToBottom(card);
        }

        /// <summary>
        /// Puts a card at a specific position from the top.
        /// </summary>
        public void PutNthFromTop(ICard card, int n)
        {
            AddAt(card, n);
        }
    }
}
