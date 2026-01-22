namespace GrandArchive.Core.Enums
{
    /// <summary>
    /// Card types in Grand Archive TCG.
    /// </summary>
    public enum CardType
    {
        /// <summary>Champion - the player's representative in the game</summary>
        Champion,

        /// <summary>Ally - support units that can attack and defend</summary>
        Ally,

        /// <summary>Action - one-time effect cards</summary>
        Action,

        /// <summary>Attack - combat cards used to deal damage</summary>
        Attack,

        /// <summary>Item - equipment and consumable cards</summary>
        Item,

        /// <summary>Weapon - equippable cards that enhance attacks</summary>
        Weapon,

        /// <summary>Domain - location cards that provide ongoing effects</summary>
        Domain,

        /// <summary>Phantasia - special manifestation cards</summary>
        Phantasia,

        /// <summary>Regalia - special champion equipment from material deck</summary>
        Regalia
    }

    /// <summary>
    /// Supertypes that can modify card types.
    /// </summary>
    public enum Supertype
    {
        None,

        /// <summary>Unique cards - only one copy allowed in deck</summary>
        Unique,

        /// <summary>Token - created by effects, not physical cards</summary>
        Token,

        /// <summary>Lineage - champion lineage cards</summary>
        Lineage
    }

    /// <summary>
    /// Functional subtypes for cards.
    /// </summary>
    public enum Subtype
    {
        None,

        // Unit subtypes (for Allies)
        Beast,
        Chessman,
        Cleric,
        Dragon,
        Elemental,
        Golem,
        Knight,
        Mage,
        Rogue,
        Spirit,
        Undead,
        Warrior,

        // Weapon subtypes
        Sword,
        Axe,
        Bow,
        Staff,
        Tome,
        Dagger,
        Aetherwing,

        // Item subtypes
        Artifact,
        Potion,
        Ingredient,

        // Domain subtypes
        Location,
        Siegeable
    }
}
