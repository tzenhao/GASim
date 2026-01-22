namespace GrandArchive.Core.Enums
{
    /// <summary>
    /// Cost types in Grand Archive TCG.
    /// Cards are paid for by placing cards from hand into memory.
    /// </summary>
    public enum CostType
    {
        /// <summary>No cost - typically for champion/mastery cards</summary>
        None,

        /// <summary>Reserve cost - paid by placing cards into memory</summary>
        Reserve,

        /// <summary>Memory cost - paid by banishing cards from memory</summary>
        Memory
    }

    /// <summary>
    /// Champion classes in Grand Archive TCG.
    /// </summary>
    public enum ChampionClass
    {
        None,
        Assassin,
        Cleric,
        Guardian,
        Mage,
        Ranger,
        Tamer,
        Warrior,
        // Multi-class champions can have multiple classes
    }
}
