namespace GrandArchive.Core.Enums
{
    /// <summary>
    /// Turn phases in Grand Archive TCG.
    /// Turn order: Wake Up → Materialize → Recollection → Draw → Main → End
    /// Combat phase occurs during Main phase when attacks are declared.
    /// </summary>
    public enum PhaseType
    {
        /// <summary>
        /// Wake Up Phase - all rested cards controlled by turn player wake up (turn face-up).
        /// First turn player skips this phase on their first turn.
        /// </summary>
        WakeUp,

        /// <summary>
        /// Materialize Phase - turn player may materialize one card from material deck.
        /// First turn player skips this phase on their first turn.
        /// </summary>
        Materialize,

        /// <summary>
        /// Recollection Phase - turn player returns cards from memory to hand.
        /// Turn player receives Opportunity at the beginning of this phase.
        /// First turn player skips this phase on their first turn.
        /// </summary>
        Recollection,

        /// <summary>
        /// Draw Phase - turn player draws a card.
        /// First turn player skips this phase on their first turn.
        /// </summary>
        Draw,

        /// <summary>
        /// Main Phase - turn player can play cards, activate abilities, and declare attacks.
        /// Turn player receives Opportunity at the beginning of this phase.
        /// </summary>
        Main,

        /// <summary>
        /// Combat Phase - occurs when an attack is declared during Main phase.
        /// </summary>
        Combat,

        /// <summary>
        /// End Phase - turn ends, "until end of turn" effects expire.
        /// Turn player receives Opportunity at the beginning of this phase.
        /// </summary>
        End
    }

    /// <summary>
    /// Combat phase steps in Grand Archive TCG.
    /// </summary>
    public enum CombatStep
    {
        /// <summary>Attack Declaration Step - attacker declares attack target</summary>
        AttackDeclaration,

        /// <summary>Interception Step - defending player may intercept with units</summary>
        Interception,

        /// <summary>Retaliation Step - defending units may retaliate</summary>
        Retaliation,

        /// <summary>Damage Step - damage is calculated and applied</summary>
        Damage,

        /// <summary>Combat End Step - combat concludes</summary>
        CombatEnd
    }
}
