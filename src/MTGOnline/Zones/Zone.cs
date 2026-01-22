using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;

namespace MTGOnline.Zones
{
    /// <summary>
    /// Base implementation of a game zone.
    /// </summary>
    public class Zone : IZone
    {
        public ZoneType ZoneType { get; }
        public ZoneVisibility Visibility { get; protected set; }
        public IPlayer? Owner { get; }
        public int Count => _cards.Count;
        public bool IsEmpty => _cards.Count == 0;

        protected readonly List<ICard> _cards = new();
        public IReadOnlyList<ICard> Cards => _cards.AsReadOnly();

        public Zone(ZoneType zoneType, ZoneVisibility visibility, IPlayer? owner = null)
        {
            ZoneType = zoneType;
            Visibility = visibility;
            Owner = owner;
        }

        public virtual void Add(ICard card)
        {
            _cards.Add(card);
        }

        public virtual void AddToTop(ICard card)
        {
            _cards.Insert(0, card);
        }

        public virtual void AddToBottom(ICard card)
        {
            _cards.Add(card);
        }

        public virtual void AddAt(ICard card, int index)
        {
            if (index < 0) index = 0;
            if (index > _cards.Count) index = _cards.Count;
            _cards.Insert(index, card);
        }

        public virtual bool Remove(ICard card)
        {
            return _cards.Remove(card);
        }

        public virtual ICard? RemoveFromTop()
        {
            if (_cards.Count == 0) return null;
            var card = _cards[0];
            _cards.RemoveAt(0);
            return card;
        }

        public virtual ICard? RemoveFromBottom()
        {
            if (_cards.Count == 0) return null;
            var card = _cards[^1];
            _cards.RemoveAt(_cards.Count - 1);
            return card;
        }

        public virtual ICard? RemoveAt(int index)
        {
            if (index < 0 || index >= _cards.Count) return null;
            var card = _cards[index];
            _cards.RemoveAt(index);
            return card;
        }

        public bool Contains(ICard card)
        {
            return _cards.Contains(card);
        }

        public void Clear()
        {
            _cards.Clear();
        }

        public void Shuffle()
        {
            var random = new Random();
            int n = _cards.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                (_cards[k], _cards[n]) = (_cards[n], _cards[k]);
            }
        }

        public ICard? Peek()
        {
            return _cards.Count > 0 ? _cards[0] : null;
        }

        public IReadOnlyList<ICard> PeekTop(int count)
        {
            return _cards.Take(Math.Min(count, _cards.Count)).ToList().AsReadOnly();
        }

        public IReadOnlyList<ICard> GetAll(Func<ICard, bool> predicate)
        {
            return _cards.Where(predicate).ToList().AsReadOnly();
        }
    }
}
