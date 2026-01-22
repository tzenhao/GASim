using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Zones
{
    /// <summary>
    /// Hand zone in Grand Archive TCG.
    /// Cards the player has drawn and can play.
    /// Private zone for the owner, hidden from opponents.
    /// </summary>
    public class Hand : Zone
    {
        public override ZoneType ZoneType => ZoneType.Hand;

        /// <summary>
        /// Hand is private to the owner.
        /// </summary>
        public override bool IsPublic => false;

        public override bool IsOrdered => false;

        /// <summary>
        /// Maximum hand size (typically 7 at end of turn).
        /// </summary>
        public int MaxHandSize { get; set; } = 7;

        /// <summary>
        /// Check if hand exceeds maximum size.
        /// </summary>
        public bool ExceedsMaxSize => _cards.Count > MaxHandSize;

        /// <summary>
        /// Number of cards over the maximum hand size.
        /// </summary>
        public int CardsOverMax => Math.Max(0, _cards.Count - MaxHandSize);

        /// <summary>
        /// Get playable cards (cards that can be played from hand).
        /// </summary>
        public IEnumerable<ICard> GetPlayableCards(IGameState gameState)
        {
            return _cards.Where(c => c is Cards.Base.Card card && card.CanPlay(gameState));
        }

        /// <summary>
        /// Discard a card from hand to graveyard.
        /// </summary>
        public ICard? Discard(ICard card)
        {
            if (Remove(card))
            {
                return card;
            }
            return null;
        }

        /// <summary>
        /// Discard a random card from hand.
        /// </summary>
        public ICard? DiscardRandom()
        {
            if (_cards.Count == 0) return null;

            var index = _random.Next(_cards.Count);
            var card = _cards[index];
            _cards.RemoveAt(index);
            return card;
        }

        /// <summary>
        /// Reveal a card in hand (make it visible to all players temporarily).
        /// </summary>
        public void Reveal(ICard card)
        {
            if (Contains(card))
            {
                card.Visibility = CardVisibility.FaceUp;
            }
        }

        /// <summary>
        /// Hide a revealed card.
        /// </summary>
        public void Hide(ICard card)
        {
            if (Contains(card))
            {
                card.Visibility = CardVisibility.FaceDown;
            }
        }

        public override string ToString() => $"Hand ({Count} cards)";
    }
}
