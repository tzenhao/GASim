namespace Shared
{
    /// <summary>
    /// Shared enums for Grand Archive TCG Simulator.
    /// Note: The main implementation is in GrandArchive.Core.Enums namespace.
    /// This file is kept for backwards compatibility with the Simulator project.
    /// </summary>
    public static class Enums
    {
        /// <summary>
        /// Elements in Grand Archive TCG.
        /// Each card has an element that determines its magical affinity.
        /// </summary>
        public enum Elements
        {
            /// <summary>Normal element - no elemental affinity</summary>
            Norm = 0,
            /// <summary>Fire element</summary>
            Fire = 1,
            /// <summary>Water element</summary>
            Water = 2,
            /// <summary>Wind element</summary>
            Wind = 3,
            /// <summary>Arcane element - magical/mystical</summary>
            Arcane = 4,
            /// <summary>Astra element - celestial/stellar</summary>
            Astra = 5,
            /// <summary>Crux element - dark/shadow</summary>
            Crux = 6,
            /// <summary>Exia element</summary>
            Exia = 7,
            /// <summary>Luxem element - light</summary>
            Luxem = 8,
            /// <summary>Neos element</summary>
            Neos = 9,
            /// <summary>Tera element - earth</summary>
            Tera = 10,
            /// <summary>Umbra element - shadow/darkness</summary>
            Umbra = 11,
        }

        /// <summary>
        /// Cost types for cards in Grand Archive TCG.
        /// </summary>
        public enum CostType
        {
            /// <summary>No cost - typically for champion/mastery cards</summary>
            None = 0,
            /// <summary>Reserve cost - paid by placing cards into memory</summary>
            Reserve = 1,
            /// <summary>Memory cost - paid by banishing cards from memory</summary>
            Memory = 2,
        }

        /// <summary>
        /// Card types in Grand Archive TCG.
        /// </summary>
        public enum CardType
        {
            Champion,
            Ally,
            Action,
            Attack,
            Item,
            Weapon,
            Domain,
            Phantasia,
            Regalia
        }

        /// <summary>
        /// Game zones in Grand Archive TCG.
        /// </summary>
        public enum ZoneType
        {
            MainDeck,
            MaterialDeck,
            Hand,
            Memory,
            Field,
            Graveyard,
            Banishment,
            Intent,
            EffectsStack
        }

        /// <summary>
        /// Turn phases in Grand Archive TCG.
        /// </summary>
        public enum PhaseType
        {
            WakeUp,
            Materialize,
            Recollection,
            Draw,
            Main,
            Combat,
            End
        }

        /// <summary>
        /// Unit states in Grand Archive TCG.
        /// </summary>
        public enum UnitState
        {
            Awake,
            Rested
        }

        /// <summary>
        /// Champion classes in Grand Archive TCG.
        /// </summary>
        public enum ChampionClass
        {
            None,
            Assassin,
            Cleric,
            Guardian,
            Mage,
            Ranger,
            Tamer,
            Warrior
        }
    }
}
