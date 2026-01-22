using GrandArchive.Cards.Base;
using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Cards.Types
{
    /// <summary>
    /// Weapon card in Grand Archive TCG.
    /// Weapons are equippable cards that enhance attacks.
    /// They have durability and lose counters when used.
    /// </summary>
    public class WeaponCard : Card, IHasStats, IHasDurability
    {
        public override CardType CardType => CardType.Weapon;

        /// <summary>
        /// Base power bonus this weapon provides.
        /// </summary>
        public int BasePower { get; init; }

        /// <summary>
        /// Current power (after modifications).
        /// </summary>
        public int Power => BasePower + PowerModifier;

        /// <summary>
        /// Temporary power modifier from effects.
        /// </summary>
        public int PowerModifier { get; set; }

        /// <summary>
        /// Weapons don't have life.
        /// </summary>
        public int BaseLife => 0;
        public int Life => 0;

        /// <summary>
        /// Base durability of this weapon.
        /// </summary>
        public int BaseDurability { get; init; }

        /// <summary>
        /// Current durability counters.
        /// </summary>
        public int Durability { get; set; }

        /// <summary>
        /// The unit this weapon is equipped to.
        /// </summary>
        public IUnit? EquippedTo { get; set; }

        /// <summary>
        /// Whether this is an Aetherwing weapon (can load cards with Aethercalling).
        /// </summary>
        public bool IsAetherwing => HasSubtype(Subtype.Aetherwing);

        /// <summary>
        /// Cards loaded into this weapon (for Aetherwing weapons).
        /// </summary>
        public List<ICard> LoadedCards { get; } = new();

        public WeaponCard()
        {
            CostType = CostType.Reserve;
        }

        /// <summary>
        /// Initialize durability when entering the field.
        /// </summary>
        public void InitializeDurability()
        {
            Durability = BaseDurability;
        }

        /// <summary>
        /// Use this weapon (lose 1 durability).
        /// </summary>
        public bool Use()
        {
            if (Durability > 0)
            {
                Durability--;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if this weapon is broken (no durability).
        /// </summary>
        public bool IsBroken => Durability <= 0;

        /// <summary>
        /// Equip this weapon to a unit.
        /// </summary>
        public void EquipTo(IUnit unit)
        {
            EquippedTo = unit;
        }

        /// <summary>
        /// Unequip this weapon.
        /// </summary>
        public void Unequip()
        {
            EquippedTo = null;
        }

        /// <summary>
        /// Load a card into this weapon (for Aetherwing weapons).
        /// </summary>
        public bool LoadCard(ICard card)
        {
            if (!IsAetherwing)
                return false;

            if (!card.HasKeyword(Keyword.Aethercalling))
                return false;

            LoadedCards.Add(card);
            return true;
        }

        public override string ToString() => $"{Name} (Weapon, Power: +{Power}, Durability: {Durability}/{BaseDurability})";
    }
}
