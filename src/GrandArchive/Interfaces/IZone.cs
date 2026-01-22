using GrandArchive.Core.Enums;

namespace GrandArchive.Interfaces
{
    /// <summary>
    /// Interface for game zones in Grand Archive TCG.
    /// </summary>
    public interface IZone
    {
        /// <summary>Type of this zone</summary>
        ZoneType ZoneType { get; }

        /// <summary>The player who owns this zone (null for shared zones)</summary>
        IPlayer? Owner { get; }

        /// <summary>Whether this zone's contents are public information</summary>
        bool IsPublic { get; }

        /// <summary>Whether the order of cards in this zone can be changed</summary>
        bool IsOrdered { get; }

        /// <summary>All cards currently in this zone</summary>
        IReadOnlyList<ICard> Cards { get; }

        /// <summary>Number of cards in this zone</summary>
        int Count { get; }

        /// <summary>Add a card to this zone</summary>
        void Add(ICard card);

        /// <summary>Remove a card from this zone</summary>
        bool Remove(ICard card);

        /// <summary>Check if a card is in this zone</summary>
        bool Contains(ICard card);

        /// <summary>Get the top card (for deck-like zones)</summary>
        ICard? TopCard { get; }

        /// <summary>Get cards matching a predicate</summary>
        IEnumerable<ICard> Where(Func<ICard, bool> predicate);

        /// <summary>Shuffle the cards in this zone (if allowed)</summary>
        void Shuffle();

        /// <summary>Clear all cards from this zone</summary>
        void Clear();
    }
}
