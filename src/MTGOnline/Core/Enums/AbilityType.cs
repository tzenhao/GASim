namespace MTGOnline.Core.Enums
{
    /// <summary>
    /// Types of abilities in Magic: The Gathering.
    /// </summary>
    public enum AbilityType
    {
        Static,          // Continuous effects (e.g., "Creatures you control get +1/+1")
        Triggered,       // Triggers on events (e.g., "When this creature enters...")
        Activated,       // Costs to activate (e.g., "{T}: Add {G}")
        Spell,           // Spell abilities on instants/sorceries
        Mana,            // Mana abilities (don't use the stack)
        Loyalty          // Planeswalker loyalty abilities
    }

    /// <summary>
    /// Timing restrictions for abilities.
    /// </summary>
    public enum TimingRestriction
    {
        Instant,                    // Can be activated anytime you have priority
        Sorcery,                    // Only during your main phase with empty stack
        OncePerTurn,                // Can only be activated once per turn
        OnlyDuringCombat,           // Combat phase only
        OnlyDuringUpkeep,           // Upkeep step only
        OnlyDuringYourTurn,         // Your turn only
        OnlyDuringOpponentsTurn     // Opponent's turn only
    }
}
