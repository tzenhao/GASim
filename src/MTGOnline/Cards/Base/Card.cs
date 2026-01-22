using MTGOnline.Core.Enums;
using MTGOnline.Mana;
using MTGOnline.Interfaces;
using MTGOnline.Abilities.Base;

namespace MTGOnline.Cards.Base
{
    /// <summary>
    /// Base class for all cards in Magic: The Gathering.
    /// </summary>
    public abstract class Card : ICard
    {
        public Guid Id { get; }
        public string Name { get; protected set; }
        public ManaCost ManaCost { get; protected set; }
        public virtual int ConvertedManaCost => ManaCost?.TotalMana ?? 0;
        public CardType CardTypes { get; protected set; }
        public SuperType SuperTypes { get; protected set; }

        private readonly List<string> _subTypes = new();
        public IReadOnlyList<string> SubTypes => _subTypes.AsReadOnly();

        public string RulesText { get; protected set; }
        public ManaColor ColorIdentity { get; protected set; }

        private readonly List<IAbility> _abilities = new();
        public IReadOnlyList<IAbility> Abilities => _abilities.AsReadOnly();

        // Card metadata
        public string? SetCode { get; set; }
        public string? CollectorNumber { get; set; }
        public CardRarity Rarity { get; set; }
        public string? FlavorText { get; set; }
        public string? Artist { get; set; }

        protected Card(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            ManaCost = new ManaCost();
            RulesText = string.Empty;
        }

        public bool HasCardType(CardType type) => (CardTypes & type) != 0;
        public bool HasSuperType(SuperType type) => (SuperTypes & type) != 0;
        public bool HasSubType(string subType) => _subTypes.Contains(subType, StringComparer.OrdinalIgnoreCase);

        public void AddSubType(string subType)
        {
            if (!_subTypes.Contains(subType, StringComparer.OrdinalIgnoreCase))
            {
                _subTypes.Add(subType);
            }
        }

        public void RemoveSubType(string subType)
        {
            _subTypes.RemoveAll(s => s.Equals(subType, StringComparison.OrdinalIgnoreCase));
        }

        protected void AddAbility(IAbility ability)
        {
            _abilities.Add(ability);
        }

        protected void RemoveAbility(IAbility ability)
        {
            _abilities.Remove(ability);
        }

        public override string ToString() => Name;

        public override bool Equals(object? obj)
        {
            if (obj is Card other)
            {
                return Id == other.Id;
            }
            return false;
        }

        public override int GetHashCode() => Id.GetHashCode();
    }

    /// <summary>
    /// Card rarity levels.
    /// </summary>
    public enum CardRarity
    {
        Common,
        Uncommon,
        Rare,
        MythicRare,
        Special,
        Bonus
    }
}
