namespace MTGOnline.Core.Enums
{
    /// <summary>
    /// Status of the game.
    /// </summary>
    public enum GameStatus
    {
        NotStarted,
        InProgress,
        Paused,
        Completed
    }

    /// <summary>
    /// Result of a game for a player.
    /// </summary>
    public enum GameResult
    {
        InProgress,
        Win,
        Loss,
        Draw
    }

    /// <summary>
    /// Types of counters that can be placed on permanents or players.
    /// </summary>
    public enum CounterType
    {
        // Common counters
        PlusOnePlusOne,
        MinusOneMinusOne,
        Loyalty,
        Charge,

        // Player counters
        Poison,
        Energy,
        Experience,

        // Specific counters
        Age,
        Aim,
        Arrow,
        Arrowhead,
        Blood,
        Bounty,
        Brick,
        Cage,
        Corpse,
        Credit,
        Crystal,
        Cube,
        Currency,
        Death,
        Delay,
        Depletion,
        Despair,
        Devotion,
        Divinity,
        Doom,
        Dream,
        Echo,
        Egg,
        Elixir,
        Ember,
        Fade,
        Fate,
        Feather,
        Filibuster,
        Flame,
        Flood,
        Fungus,
        Fuse,
        Gem,
        Glyph,
        Gold,
        Growth,
        Hatchling,
        Healing,
        Hit,
        Hoofprint,
        Hour,
        Hourglass,
        Hunger,
        Ice,
        Incubation,
        Infection,
        Intervention,
        Javelin,
        Ki,
        Knowledge,
        Level,
        Lore,
        Luck,
        Magnet,
        Manifestation,
        Mannequin,
        Matrix,
        Mine,
        Mining,
        Mire,
        Music,
        Muster,
        Net,
        Omen,
        Ore,
        Page,
        Pain,
        Petal,
        Petrification,
        Phylactery,
        Pin,
        Plague,
        Polyp,
        Pressure,
        Pupa,
        Quest,
        Rust,
        Scream,
        Scroll,
        Shell,
        Shield,
        Shred,
        Sleep,
        Sleight,
        Slime,
        Soot,
        Soul,
        Spark,
        Spore,
        Storage,
        Strife,
        Study,
        Suspect,
        Task,
        Theft,
        Tide,
        Time,
        Tower,
        Training,
        Trap,
        Treasure,
        Velocity,
        Verse,
        Vitality,
        Volatile,
        Vow,
        Voyage,
        Wage,
        Winch,
        Wind,
        Wish
    }

    /// <summary>
    /// Win conditions and loss conditions.
    /// </summary>
    public enum WinLossCondition
    {
        // Loss conditions
        LifeReachedZero,
        DrewFromEmptyLibrary,
        TenOrMorePoison,
        CardEffect,           // "You lose the game" effect
        Conceded,

        // Win conditions
        OpponentLost,
        AlternateWinCondition // "You win the game" effect
    }
}
