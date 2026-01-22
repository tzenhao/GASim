using static Shared.Enums;

namespace Classes.Cards
{
    /// <summary>
    /// Base card class for Grand Archive TCG.
    /// This is a simplified version - see GrandArchive.Cards.Base.Card for the full implementation.
    /// </summary>
    public class Card
    {
        public Card()
        {
        }

        #region Properties

        /// <summary>
        /// Unique identifier for this card instance.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Name of the card.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Type of card (Champion, Ally, Action, Attack, etc.)
        /// </summary>
        public CardType CardType { get; set; }

        #region Casting Information

        /// <summary>
        /// Element of card (Fire, Water, Wind, etc.)
        /// </summary>
        public Elements Element { get; set; }

        /// <summary>
        /// Whether card is exalted (foil/special version).
        /// </summary>
        public bool IsExalted { get; set; }

        /// <summary>
        /// Numeric cost of card.
        /// </summary>
        public int Cost { get; set; }

        /// <summary>
        /// What kind of cost the card has (Reserve or Memory).
        /// </summary>
        public CostType CostType { get; set; }

        #endregion

        #region Stats (for units)

        /// <summary>
        /// Power stat (for units and attacks).
        /// </summary>
        public int Power { get; set; }

        /// <summary>
        /// Life stat (for units).
        /// </summary>
        public int Life { get; set; }

        /// <summary>
        /// Durability stat (for weapons).
        /// </summary>
        public int Durability { get; set; }

        #endregion

        #region Game State

        /// <summary>
        /// Current zone where this card exists.
        /// </summary>
        public ZoneType CurrentZone { get; set; }

        /// <summary>
        /// Current state for units (Awake or Rested).
        /// </summary>
        public UnitState State { get; set; } = UnitState.Awake;

        /// <summary>
        /// Number of damage counters on this card.
        /// </summary>
        public int DamageCounters { get; set; }

        /// <summary>
        /// Number of buff counters on this card.
        /// </summary>
        public int BuffCounters { get; set; }

        #endregion

        /// <summary>
        /// Rules text on the card.
        /// </summary>
        public string RulesText { get; set; } = string.Empty;

        #endregion

        public override string ToString() => $"{Name} ({CardType}, {Element})";
    }
}
