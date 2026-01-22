using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;

namespace MTGOnline.Zones
{
    /// <summary>
    /// Represents a player's hand.
    /// </summary>
    public class Hand : Zone
    {
        public int MaxHandSize { get; set; } = 7;

        public Hand(IPlayer owner) : base(ZoneType.Hand, ZoneVisibility.OwnerOnly, owner)
        {
        }

        /// <summary>
        /// Adds a card to the hand (typically from drawing).
        /// </summary>
        public void AddCard(ICard card)
        {
            Add(card);
        }

        /// <summary>
        /// Discards a specific card from the hand.
        /// </summary>
        public bool Discard(ICard card)
        {
            return Remove(card);
        }

        /// <summary>
        /// Discards a random card from the hand.
        /// </summary>
        public ICard? DiscardRandom()
        {
            if (IsEmpty) return null;

            var random = new Random();
            int index = random.Next(_cards.Count);
            return RemoveAt(index);
        }

        /// <summary>
        /// Gets castable cards from the hand based on available mana.
        /// </summary>
        public IReadOnlyList<ICard> GetCastableCards(Mana.ManaPool manaPool)
        {
            return _cards.Where(card => manaPool.CanPay(card.ManaCost)).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets all land cards in the hand.
        /// </summary>
        public IReadOnlyList<ICard> GetLands()
        {
            return _cards.Where(card => card.HasCardType(CardType.Land)).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets all creature cards in the hand.
        /// </summary>
        public IReadOnlyList<ICard> GetCreatures()
        {
            return _cards.Where(card => card.HasCardType(CardType.Creature)).ToList().AsReadOnly();
        }

        /// <summary>
        /// Checks if the hand exceeds the maximum hand size.
        /// </summary>
        public bool ExceedsMaxHandSize => Count > MaxHandSize;

        /// <summary>
        /// Gets the number of cards that need to be discarded to reach max hand size.
        /// </summary>
        public int CardsToDiscard => Math.Max(0, Count - MaxHandSize);

        /// <summary>
        /// Reveals a specific card to all players.
        /// </summary>
        public void Reveal(ICard card)
        {
            // In a full implementation, this would trigger an event to notify all players
        }

        /// <summary>
        /// Reveals all cards in hand.
        /// </summary>
        public IReadOnlyList<ICard> RevealAll()
        {
            return Cards;
        }
    }
}
