namespace GrandArchive.Core.Enums
{
    /// <summary>
    /// Keywords and abilities in Grand Archive TCG.
    /// </summary>
    [Flags]
    public enum Keyword
    {
        None = 0,

        // Combat Keywords
        /// <summary>Ambush - unit may retaliate against attackers while not defending</summary>
        Ambush = 1 << 0,

        /// <summary>Cleave - attack all attackable objects a chosen player controls</summary>
        Cleave = 1 << 1,

        /// <summary>Intercept - can block attacks targeting other units</summary>
        Intercept = 1 << 2,

        /// <summary>Stealth - can't be intercepted</summary>
        Stealth = 1 << 3,

        /// <summary>True Sight - can intercept units with Stealth</summary>
        TrueSight = 1 << 4,

        /// <summary>Floating - can't be attacked by non-Floating units without ranged attacks</summary>
        Floating = 1 << 5,

        /// <summary>Ranged - can attack Floating units</summary>
        Ranged = 1 << 6,

        // Triggered Keywords
        /// <summary>On Enter - triggers when entering the field</summary>
        OnEnter = 1 << 7,

        /// <summary>On Death - triggers when destroyed</summary>
        OnDeath = 1 << 8,

        /// <summary>On Attack - triggers when attacking</summary>
        OnAttack = 1 << 9,

        // Static Keywords
        /// <summary>Fast - can be activated at fast speed</summary>
        Fast = 1 << 10,

        /// <summary>Inherited - ability is passed to equipped unit</summary>
        Inherited = 1 << 11,

        /// <summary>Flux - can be activated from memory</summary>
        Flux = 1 << 12,

        /// <summary>Efficiency - reduced cost under certain conditions</summary>
        Efficiency = 1 << 13,

        /// <summary>Glimpse - look at top cards of deck</summary>
        Glimpse = 1 << 14,

        /// <summary>Aethercalling - can be loaded into Aetherwing weapon while glimpsing</summary>
        Aethercalling = 1 << 15,

        // Restriction Keywords
        /// <summary>Pride - can only attack if certain conditions are met</summary>
        Pride = 1 << 16,

        /// <summary>Unique - only one copy allowed in deck</summary>
        Unique = 1 << 17,

        /// <summary>Divine Relic - only one in material deck</summary>
        DivineRelic = 1 << 18,

        // Special Keywords
        /// <summary>Command - an ally performs this attack instead of champion</summary>
        Command = 1 << 19,

        /// <summary>Brew - alternative cost using ingredients</summary>
        Brew = 1 << 20,

        /// <summary>Mastery - special champion ability</summary>
        Mastery = 1 << 21,

        /// <summary>Lineage - champion lineage ability</summary>
        Lineage = 1 << 22,

        /// <summary>Class Bonus - bonus when champion class matches</summary>
        ClassBonus = 1 << 23,

        /// <summary>Champion Bonus - bonus when specific champion is used</summary>
        ChampionBonus = 1 << 24
    }

    /// <summary>
    /// Card speed types in Grand Archive TCG.
    /// </summary>
    public enum SpeedType
    {
        /// <summary>Standard speed - can only be played during your main phase</summary>
        Standard,

        /// <summary>Fast speed - can be played when you have Opportunity</summary>
        Fast
    }

    /// <summary>
    /// Unit states in Grand Archive TCG.
    /// </summary>
    public enum UnitState
    {
        /// <summary>Awake - unit is ready and can act</summary>
        Awake,

        /// <summary>Rested - unit has been used and cannot act until woken</summary>
        Rested
    }
}
