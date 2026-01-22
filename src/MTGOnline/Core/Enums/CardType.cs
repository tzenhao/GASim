namespace MTGOnline.Core.Enums
{
    /// <summary>
    /// Primary card types in Magic: The Gathering.
    /// </summary>
    [Flags]
    public enum CardType
    {
        None = 0,
        Land = 1 << 0,
        Creature = 1 << 1,
        Artifact = 1 << 2,
        Enchantment = 1 << 3,
        Planeswalker = 1 << 4,
        Instant = 1 << 5,
        Sorcery = 1 << 6,
        Tribal = 1 << 7,
        Battle = 1 << 8
    }

    /// <summary>
    /// Supertypes that can be applied to cards.
    /// </summary>
    [Flags]
    public enum SuperType
    {
        None = 0,
        Basic = 1 << 0,
        Legendary = 1 << 1,
        Snow = 1 << 2,
        World = 1 << 3,
        Ongoing = 1 << 4
    }
}
