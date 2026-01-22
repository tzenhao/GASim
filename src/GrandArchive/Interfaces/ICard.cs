using GrandArchive.Core.Enums;

namespace GrandArchive.Interfaces
{
    /// <summary>
    /// Interface for all cards in Grand Archive TCG.
    /// </summary>
    public interface ICard
    {
        /// <summary>Unique identifier for this card instance</summary>
        Guid Id { get; }

        /// <summary>Name of the card</summary>
        string Name { get; }

        /// <summary>The card's element</summary>
        Element Element { get; }

        /// <summary>The card's type</summary>
        CardType CardType { get; }

        /// <summary>The card's supertype (if any)</summary>
        Supertype Supertype { get; }

        /// <summary>The card's subtypes</summary>
        IReadOnlyList<Subtype> Subtypes { get; }

        /// <summary>Reserve cost to play the card</summary>
        int Cost { get; }

        /// <summary>Type of cost (Reserve or Memory)</summary>
        CostType CostType { get; }

        /// <summary>Card's speed (Standard or Fast)</summary>
        SpeedType Speed { get; }

        /// <summary>Whether this card is Exalted (foil/special)</summary>
        bool IsExalted { get; }

        /// <summary>The card's rules text</summary>
        string RulesText { get; }

        /// <summary>Keywords on this card</summary>
        Keyword Keywords { get; }

        /// <summary>Check if this card has a specific keyword</summary>
        bool HasKeyword(Keyword keyword);

        /// <summary>Current zone where this card exists</summary>
        ZoneType CurrentZone { get; set; }

        /// <summary>Current visibility state of the card</summary>
        CardVisibility Visibility { get; set; }

        /// <summary>The player who owns this card</summary>
        IPlayer? Owner { get; set; }

        /// <summary>The player who currently controls this card</summary>
        IPlayer? Controller { get; set; }
    }

    /// <summary>
    /// Interface for cards with stats (Power/Life).
    /// </summary>
    public interface IHasStats
    {
        /// <summary>Base power stat</summary>
        int BasePower { get; }

        /// <summary>Current power (after modifications)</summary>
        int Power { get; }

        /// <summary>Base life stat</summary>
        int BaseLife { get; }

        /// <summary>Current life (after modifications)</summary>
        int Life { get; }
    }

    /// <summary>
    /// Interface for cards with durability (Weapons).
    /// </summary>
    public interface IHasDurability
    {
        /// <summary>Base durability</summary>
        int BaseDurability { get; }

        /// <summary>Current durability counters</summary>
        int Durability { get; set; }
    }

    /// <summary>
    /// Interface for unit cards (Champions and Allies).
    /// </summary>
    public interface IUnit : ICard, IHasStats
    {
        /// <summary>Current state (Awake or Rested)</summary>
        UnitState State { get; set; }

        /// <summary>Number of buff counters on this unit</summary>
        int BuffCounters { get; set; }

        /// <summary>Number of damage counters on this unit</summary>
        int DamageCounters { get; set; }

        /// <summary>Wake up this unit (turn face-up/ready)</summary>
        void WakeUp();

        /// <summary>Rest this unit (turn face-down/exhausted)</summary>
        void Rest();

        /// <summary>Deal damage to this unit</summary>
        void TakeDamage(int amount);

        /// <summary>Check if this unit is defeated (damage >= life)</summary>
        bool IsDefeated { get; }
    }
}
