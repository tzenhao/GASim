using GrandArchive.Cards.Base;
using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Cards.Types
{
    /// <summary>
    /// Regalia card in Grand Archive TCG.
    /// Regalias are special champion equipment from the material deck.
    /// They are materialized during the Materialize phase.
    /// </summary>
    public class RegaliaCard : Card, IHasDurability
    {
        public override CardType CardType => CardType.Regalia;

        /// <summary>
        /// Base durability of this regalia.
        /// </summary>
        public int BaseDurability { get; init; }

        /// <summary>
        /// Current durability counters.
        /// </summary>
        public int Durability { get; set; }

        /// <summary>
        /// The champion this regalia is equipped to.
        /// </summary>
        public IChampion? EquippedTo { get; set; }

        /// <summary>
        /// The effect this regalia provides.
        /// </summary>
        public Action<IGameState>? Effect { get; init; }

        /// <summary>
        /// The activated ability of this regalia (if any).
        /// </summary>
        public IAbility? ActivatedAbility { get; init; }

        public RegaliaCard()
        {
            // Regalias are in the material deck and have no cost
            CurrentZone = ZoneType.MaterialDeck;
            CostType = CostType.None;
        }

        /// <summary>
        /// Initialize durability when entering the field.
        /// </summary>
        public void InitializeDurability()
        {
            Durability = BaseDurability;
        }

        /// <summary>
        /// Use this regalia (lose 1 durability if applicable).
        /// </summary>
        public bool Use()
        {
            if (BaseDurability > 0 && Durability > 0)
            {
                Durability--;
                return true;
            }
            return BaseDurability == 0; // No durability = always usable
        }

        /// <summary>
        /// Check if this regalia is broken (no durability).
        /// </summary>
        public bool IsBroken => BaseDurability > 0 && Durability <= 0;

        /// <summary>
        /// Equip this regalia to a champion.
        /// </summary>
        public void EquipTo(IChampion champion)
        {
            EquippedTo = champion;
        }

        /// <summary>
        /// Unequip this regalia.
        /// </summary>
        public void Unequip()
        {
            EquippedTo = null;
        }

        public override string ToString()
        {
            if (BaseDurability > 0)
                return $"{Name} (Regalia, Durability: {Durability}/{BaseDurability})";
            return $"{Name} (Regalia)";
        }
    }
}
