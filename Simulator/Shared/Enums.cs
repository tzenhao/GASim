namespace Shared
{
    public class Enums
    {
        // Not setting Exalted here - flag on card itself
        public enum Elements
        {
            Norm    = 0,
            Fire    = 1,
            Water   = 2,
            Wind    = 3,
            Arcane  = 4,
            Astra   = 5,
            Crux    = 6,
            Exia    = 7,
            Luxem   = 8,
            Neos    = 9,
            Tera    = 10,
            Umbra   = 11,
        }

        // Cost types for cards
        public enum CostType
        {
            None    = 0, // Masteries
            Reserve = 1,
            Memory  = 2,
        }
    }
}
