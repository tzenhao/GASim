using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Zones
{
    /// <summary>
    /// Base class for game zones in Grand Archive TCG.
    /// </summary>
    public abstract class Zone : IZone
    {
        protected readonly List<ICard> _cards = new();
        protected readonly Random _random = new();

        public abstract ZoneType ZoneType { get; }

        public IPlayer? Owner { get; init; }

        public abstract bool IsPublic { get; }

        public abstract bool IsOrdered { get; }

        public IReadOnlyList<ICard> Cards => _cards.AsReadOnly();

        public int Count => _cards.Count;

        public ICard? TopCard => _cards.Count > 0 ? _cards[^1] : null;

        public virtual void Add(ICard card)
        {
            card.CurrentZone = ZoneType;
            card.Visibility = IsPublic ? CardVisibility.FaceUp : CardVisibility.FaceDown;
            _cards.Add(card);
        }

        public virtual bool Remove(ICard card)
        {
            return _cards.Remove(card);
        }

        public bool Contains(ICard card)
        {
            return _cards.Contains(card);
        }

        public IEnumerable<ICard> Where(Func<ICard, bool> predicate)
        {
            return _cards.Where(predicate);
        }

        public virtual void Shuffle()
        {
            if (!IsOrdered)
            {
                // Fisher-Yates shuffle
                for (int i = _cards.Count - 1; i > 0; i--)
                {
                    int j = _random.Next(i + 1);
                    (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
                }
            }
        }

        public void Clear()
        {
            _cards.Clear();
        }

        /// <summary>
        /// Draw the top card from this zone.
        /// </summary>
        public ICard? DrawTop()
        {
            if (_cards.Count == 0) return null;

            var card = _cards[^1];
            _cards.RemoveAt(_cards.Count - 1);
            return card;
        }

        /// <summary>
        /// Add a card to the top of this zone.
        /// </summary>
        public void AddToTop(ICard card)
        {
            card.CurrentZone = ZoneType;
            card.Visibility = IsPublic ? CardVisibility.FaceUp : CardVisibility.FaceDown;
            _cards.Add(card);
        }

        /// <summary>
        /// Add a card to the bottom of this zone.
        /// </summary>
        public void AddToBottom(ICard card)
        {
            card.CurrentZone = ZoneType;
            card.Visibility = IsPublic ? CardVisibility.FaceUp : CardVisibility.FaceDown;
            _cards.Insert(0, card);
        }

        /// <summary>
        /// Look at the top N cards without removing them.
        /// </summary>
        public IReadOnlyList<ICard> PeekTop(int count)
        {
            count = Math.Min(count, _cards.Count);
            return _cards.Skip(_cards.Count - count).Take(count).ToList().AsReadOnly();
        }

        public override string ToString() => $"{ZoneType} ({Count} cards)";
    }
}
