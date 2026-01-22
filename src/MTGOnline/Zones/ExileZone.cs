using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;

namespace MTGOnline.Zones
{
    /// <summary>
    /// Represents the exile zone.
    /// </summary>
    public class ExileZone : Zone
    {
        // Some cards are exiled face-down or with special properties
        private readonly Dictionary<Guid, ExileProperties> _exileProperties = new();

        public ExileZone() : base(ZoneType.Exile, ZoneVisibility.Public, null)
        {
        }

        /// <summary>
        /// Exiles a card with optional properties.
        /// </summary>
        public void ExileCard(ICard card, ExileProperties? properties = null)
        {
            Add(card);
            if (properties != null)
            {
                _exileProperties[card.Id] = properties;
            }
        }

        /// <summary>
        /// Gets the properties of an exiled card.
        /// </summary>
        public ExileProperties? GetProperties(ICard card)
        {
            return _exileProperties.TryGetValue(card.Id, out var props) ? props : null;
        }

        /// <summary>
        /// Gets all cards exiled by a specific source.
        /// </summary>
        public IReadOnlyList<ICard> GetCardsExiledBy(ICard source)
        {
            return _cards.Where(c =>
                _exileProperties.TryGetValue(c.Id, out var props) &&
                props.ExiledByCardId == source.Id).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets all face-down exiled cards.
        /// </summary>
        public IReadOnlyList<ICard> GetFaceDownCards()
        {
            return _cards.Where(c =>
                _exileProperties.TryGetValue(c.Id, out var props) &&
                props.IsFaceDown).ToList().AsReadOnly();
        }

        /// <summary>
        /// Returns an exiled card to the battlefield.
        /// </summary>
        public ICard? ReturnToBattlefield(ICard card)
        {
            if (Remove(card))
            {
                _exileProperties.Remove(card.Id);
                return card;
            }
            return null;
        }

        /// <summary>
        /// Returns an exiled card to the owner's hand.
        /// </summary>
        public ICard? ReturnToHand(ICard card)
        {
            if (Remove(card))
            {
                _exileProperties.Remove(card.Id);
                return card;
            }
            return null;
        }
    }

    /// <summary>
    /// Properties for exiled cards.
    /// </summary>
    public class ExileProperties
    {
        public Guid? ExiledByCardId { get; set; }
        public bool IsFaceDown { get; set; }
        public bool CanBeCastFromExile { get; set; }
        public bool CanBePlayedAsLand { get; set; }
        public DateTime ExiledAt { get; set; } = DateTime.UtcNow;
        public string? SpecialNote { get; set; }

        // For temporary exile effects (e.g., "exile until end of turn")
        public bool IsTemporary { get; set; }
        public DateTime? ReturnsAt { get; set; }
    }
}
