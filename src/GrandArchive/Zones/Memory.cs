using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Zones
{
    /// <summary>
    /// Memory zone in Grand Archive TCG.
    /// Cards placed face-down to pay costs.
    /// Private zone - cards are face-down.
    /// </summary>
    public class Memory : Zone
    {
        public override ZoneType ZoneType => ZoneType.Memory;

        public override bool IsPublic => false;

        public override bool IsOrdered => false;

        /// <summary>
        /// Place a card into memory (to pay costs).
        /// </summary>
        public void PlaceInMemory(ICard card)
        {
            card.Visibility = CardVisibility.FaceDown;
            Add(card);
        }

        /// <summary>
        /// Recollect a card from memory (return to hand).
        /// Used during Recollection phase.
        /// </summary>
        public ICard? Recollect(ICard card)
        {
            if (Remove(card))
            {
                return card;
            }
            return null;
        }

        /// <summary>
        /// Recollect multiple cards from memory.
        /// </summary>
        public List<ICard> Recollect(int count)
        {
            var recollected = new List<ICard>();
            for (int i = 0; i < count && _cards.Count > 0; i++)
            {
                var card = _cards[0];
                _cards.RemoveAt(0);
                recollected.Add(card);
            }
            return recollected;
        }

        /// <summary>
        /// Banish a card from memory (move to banishment).
        /// Used to pay memory costs.
        /// </summary>
        public ICard? Banish(ICard card)
        {
            if (Remove(card))
            {
                return card;
            }
            return null;
        }

        /// <summary>
        /// Get the total reserve value in memory (for paying costs).
        /// Each card in memory provides 1 reserve.
        /// </summary>
        public int TotalReserve => _cards.Count;

        /// <summary>
        /// Check if there's enough reserve to pay a cost.
        /// </summary>
        public bool CanPayCost(int cost) => TotalReserve >= cost;

        public override string ToString() => $"Memory ({Count} cards, {TotalReserve} reserve)";
    }
}
