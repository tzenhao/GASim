namespace MTGOnline.Core.Enums
{
    /// <summary>
    /// Types of targets that can be selected.
    /// </summary>
    [Flags]
    public enum TargetType
    {
        None = 0,
        Player = 1 << 0,
        Creature = 1 << 1,
        Artifact = 1 << 2,
        Enchantment = 1 << 3,
        Planeswalker = 1 << 4,
        Land = 1 << 5,
        Spell = 1 << 6,          // On the stack
        Ability = 1 << 7,        // On the stack
        Card = 1 << 8,           // In graveyard/hand/library
        Permanent = Creature | Artifact | Enchantment | Planeswalker | Land,
        Any = Player | Permanent,
        AnyTarget = Player | Creature | Planeswalker  // "Any target" in modern rules text
    }

    /// <summary>
    /// Controller/owner restrictions for targeting.
    /// </summary>
    public enum TargetController
    {
        Any,
        You,
        Opponent,
        AnotherPlayer
    }
}
