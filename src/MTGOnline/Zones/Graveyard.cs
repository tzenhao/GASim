using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;

namespace MTGOnline.Zones
{
    /// <summary>
    /// Represents a player's graveyard.
    /// </summary>
    public class Graveyard : Zone
    {
        public Graveyard(IPlayer owner) : base(ZoneType.Graveyard, ZoneVisibility.Public, owner)
        {
        }

        /// <summary>
        /// Puts a card into the graveyard (on top).
        /// </summary>
        public void PutCard(ICard card)
        {
            AddToTop(card);
        }

        /// <summary>
        /// Gets all creature cards in the graveyard.
        /// </summary>
        public IReadOnlyList<ICard> GetCreatures()
        {
            return _cards.Where(card => card.HasCardType(CardType.Creature)).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets all instant and sorcery cards in the graveyard.
        /// </summary>
        public IReadOnlyList<ICard> GetInstantsAndSorceries()
        {
            return _cards.Where(card =>
                card.HasCardType(CardType.Instant) ||
                card.HasCardType(CardType.Sorcery)).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets all land cards in the graveyard.
        /// </summary>
        public IReadOnlyList<ICard> GetLands()
        {
            return _cards.Where(card => card.HasCardType(CardType.Land)).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets cards with flashback ability.
        /// </summary>
        public IReadOnlyList<ICard> GetCardsWithFlashback()
        {
            // In full implementation, would check for flashback ability
            return new List<ICard>().AsReadOnly();
        }

        /// <summary>
        /// Exiles a card from the graveyard.
        /// </summary>
        public ICard? ExileCard(ICard card)
        {
            if (Remove(card))
            {
                return card;
            }
            return null;
        }

        /// <summary>
        /// Returns a card from the graveyard to hand.
        /// </summary>
        public ICard? ReturnToHand(ICard card)
        {
            if (Remove(card))
            {
                return card;
            }
            return null;
        }

        /// <summary>
        /// Returns a card from the graveyard to the battlefield.
        /// </summary>
        public ICard? ReturnToBattlefield(ICard card)
        {
            if (Remove(card))
            {
                return card;
            }
            return null;
        }

        /// <summary>
        /// Returns a card from the graveyard to the top of the library.
        /// </summary>
        public ICard? ReturnToLibrary(ICard card)
        {
            if (Remove(card))
            {
                return card;
            }
            return null;
        }
    }
}
