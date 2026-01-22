namespace MTGOnline.Core.Enums
{
    /// <summary>
    /// Game zones where cards can exist.
    /// </summary>
    public enum ZoneType
    {
        Library,
        Hand,
        Battlefield,
        Graveyard,
        Stack,
        Exile,
        Command,
        Ante,        // Legacy zone, rarely used
        Sideboard
    }

    /// <summary>
    /// Visibility rules for zones.
    /// </summary>
    public enum ZoneVisibility
    {
        Hidden,          // Only owner can see (Library)
        OwnerOnly,       // Only owner can see (Hand)
        Public,          // Everyone can see (Battlefield, Graveyard, Exile, Stack)
        Revealed         // Temporarily revealed to all players
    }
}
