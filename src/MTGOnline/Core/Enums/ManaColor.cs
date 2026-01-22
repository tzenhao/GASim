namespace MTGOnline.Core.Enums
{
    /// <summary>
    /// Represents the five colors of mana in Magic: The Gathering, plus colorless.
    /// </summary>
    [Flags]
    public enum ManaColor
    {
        None = 0,
        White = 1 << 0,      // W - Plains
        Blue = 1 << 1,       // U - Island
        Black = 1 << 2,      // B - Swamp
        Red = 1 << 3,        // R - Mountain
        Green = 1 << 4,      // G - Forest
        Colorless = 1 << 5,  // C - Wastes/Eldrazi

        // Common combinations
        Azorius = White | Blue,
        Dimir = Blue | Black,
        Rakdos = Black | Red,
        Gruul = Red | Green,
        Selesnya = Green | White,
        Orzhov = White | Black,
        Izzet = Blue | Red,
        Golgari = Black | Green,
        Boros = Red | White,
        Simic = Green | Blue
    }
}
