namespace MTGOnline.Core.Enums
{
    /// <summary>
    /// Evergreen and common keyword abilities.
    /// </summary>
    [Flags]
    public enum KeywordAbility : long
    {
        None = 0,

        // Evasion
        Flying = 1L << 0,
        Menace = 1L << 1,
        Trample = 1L << 2,
        Fear = 1L << 3,
        Intimidate = 1L << 4,
        Skulk = 1L << 5,
        Shadow = 1L << 6,
        Horsemanship = 1L << 7,

        // Combat
        FirstStrike = 1L << 8,
        DoubleStrike = 1L << 9,
        Deathtouch = 1L << 10,
        Lifelink = 1L << 11,
        Vigilance = 1L << 12,
        Reach = 1L << 13,
        Defender = 1L << 14,
        Haste = 1L << 15,
        Indestructible = 1L << 16,

        // Protection & Evasion
        Hexproof = 1L << 17,
        Shroud = 1L << 18,
        Ward = 1L << 19,

        // Other Evergreen
        Flash = 1L << 20,
        Equip = 1L << 21,
        Enchant = 1L << 22,

        // Card Flow
        Scry = 1L << 23,
        Mill = 1L << 24,

        // Returning Keywords
        Flashback = 1L << 25,
        Cycling = 1L << 26,
        Kicker = 1L << 27,

        // Creature Keywords
        Changeling = 1L << 28,
        Undying = 1L << 29,
        Persist = 1L << 30,
        Afflict = 1L << 31,
        Annihilator = 1L << 32,
        Wither = 1L << 33,
        Infect = 1L << 34,

        // Land Keywords
        Landwalk = 1L << 35,

        // Other
        Convoke = 1L << 36,
        Delve = 1L << 37,
        Cascade = 1L << 38,
        Storm = 1L << 39,
        Affinity = 1L << 40,
        Prowess = 1L << 41,
        Toxic = 1L << 42
    }
}
