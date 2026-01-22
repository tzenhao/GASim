namespace MTGOnline.Core.Enums
{
    /// <summary>
    /// The phases of a turn in Magic: The Gathering.
    /// </summary>
    public enum PhaseType
    {
        Beginning,
        PreCombatMain,
        Combat,
        PostCombatMain,
        Ending
    }

    /// <summary>
    /// The steps within each phase.
    /// </summary>
    public enum StepType
    {
        // Beginning Phase
        Untap,
        Upkeep,
        Draw,

        // Main Phase (no steps, just the phase itself)
        Main,

        // Combat Phase
        BeginningOfCombat,
        DeclareAttackers,
        DeclareBlockers,
        FirstStrikeDamage,
        CombatDamage,
        EndOfCombat,

        // Ending Phase
        End,
        Cleanup
    }
}
