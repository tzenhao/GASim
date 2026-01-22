using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;

namespace MTGOnline.Zones
{
    /// <summary>
    /// Represents the command zone (used in Commander/EDH format).
    /// </summary>
    public class CommandZone : Zone
    {
        // Track how many times each commander has been cast
        private readonly Dictionary<Guid, int> _commanderCastCount = new();

        public CommandZone(IPlayer owner) : base(ZoneType.Command, ZoneVisibility.Public, owner)
        {
        }

        /// <summary>
        /// Gets the commander(s) in the command zone.
        /// </summary>
        public IReadOnlyList<ICard> Commanders => Cards;

        /// <summary>
        /// Adds a commander to the command zone.
        /// </summary>
        public void AddCommander(ICard commander)
        {
            Add(commander);
            if (!_commanderCastCount.ContainsKey(commander.Id))
            {
                _commanderCastCount[commander.Id] = 0;
            }
        }

        /// <summary>
        /// Gets the number of times a commander has been cast.
        /// </summary>
        public int GetCastCount(ICard commander)
        {
            return _commanderCastCount.TryGetValue(commander.Id, out int count) ? count : 0;
        }

        /// <summary>
        /// Gets the commander tax (additional mana cost) for casting a commander.
        /// </summary>
        public int GetCommanderTax(ICard commander)
        {
            return GetCastCount(commander) * 2;
        }

        /// <summary>
        /// Called when a commander is cast from the command zone.
        /// </summary>
        public void OnCommanderCast(ICard commander)
        {
            if (_commanderCastCount.ContainsKey(commander.Id))
            {
                _commanderCastCount[commander.Id]++;
            }
            Remove(commander);
        }

        /// <summary>
        /// Returns a commander to the command zone.
        /// </summary>
        public void ReturnCommander(ICard commander)
        {
            if (!Contains(commander))
            {
                Add(commander);
            }
        }

        /// <summary>
        /// Checks if a card is a commander in this zone.
        /// </summary>
        public bool IsCommander(ICard card)
        {
            return _commanderCastCount.ContainsKey(card.Id);
        }
    }
}
