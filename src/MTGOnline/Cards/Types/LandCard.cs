using MTGOnline.Core.Enums;
using MTGOnline.Cards.Base;
using MTGOnline.Interfaces;
using MTGOnline.Mana;

namespace MTGOnline.Cards.Types
{
    /// <summary>
    /// Represents a land card in Magic: The Gathering.
    /// </summary>
    public class LandCard : Permanent, ILand
    {
        private readonly List<IManaAbility> _manaAbilities = new();
        public IReadOnlyList<IManaAbility> ManaAbilities => _manaAbilities.AsReadOnly();

        public bool IsBasicLand => HasSuperType(SuperType.Basic);

        public LandCard(string name) : base(name)
        {
            CardTypes = CardType.Land;
            // Lands have no mana cost
            ManaCost = new ManaCost();
        }

        public void AddManaAbility(IManaAbility ability)
        {
            _manaAbilities.Add(ability);
        }

        public void RemoveManaAbility(IManaAbility ability)
        {
            _manaAbilities.Remove(ability);
        }
    }

    /// <summary>
    /// Represents a basic land card.
    /// </summary>
    public class BasicLandCard : LandCard
    {
        public ManaColor ProducedMana { get; }

        public BasicLandCard(string name, ManaColor producedMana) : base(name)
        {
            SuperTypes = SuperType.Basic;
            ProducedMana = producedMana;

            // Add the basic land subtype
            switch (producedMana)
            {
                case ManaColor.White:
                    AddSubType("Plains");
                    break;
                case ManaColor.Blue:
                    AddSubType("Island");
                    break;
                case ManaColor.Black:
                    AddSubType("Swamp");
                    break;
                case ManaColor.Red:
                    AddSubType("Mountain");
                    break;
                case ManaColor.Green:
                    AddSubType("Forest");
                    break;
            }
        }

        public static BasicLandCard CreatePlains() => new("Plains", ManaColor.White);
        public static BasicLandCard CreateIsland() => new("Island", ManaColor.Blue);
        public static BasicLandCard CreateSwamp() => new("Swamp", ManaColor.Black);
        public static BasicLandCard CreateMountain() => new("Mountain", ManaColor.Red);
        public static BasicLandCard CreateForest() => new("Forest", ManaColor.Green);
    }
}
