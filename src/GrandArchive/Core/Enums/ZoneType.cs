namespace GrandArchive.Core.Enums
{
    /// <summary>
    /// Game zones in Grand Archive TCG.
    /// There are 9 total zones where cards can exist during a game.
    /// </summary>
    public enum ZoneType
    {
        /// <summary>
        /// Main Deck - contains the player's main deck of cards.
        /// Private zone, cards are face-down.
        /// </summary>
        MainDeck,

        /// <summary>
        /// Material Deck - contains champion, regalia, and other material cards.
        /// Public zone, cards are face-up.
        /// </summary>
        MaterialDeck,

        /// <summary>
        /// Hand - cards the player has drawn and can play.
        /// Private zone for the owner, hidden from opponents.
        /// </summary>
        Hand,

        /// <summary>
        /// Memory - cards placed face-down to pay costs.
        /// Private zone, cards are face-down.
        /// </summary>
        Memory,

        /// <summary>
        /// Field - where cards are played and exist as objects.
        /// Shared public zone between all players.
        /// </summary>
        Field,

        /// <summary>
        /// Graveyard - where destroyed/used cards go.
        /// Public zone, cards are face-up.
        /// </summary>
        Graveyard,

        /// <summary>
        /// Banishment - cards removed from the game.
        /// Public zone, cards are typically face-up.
        /// </summary>
        Banishment,

        /// <summary>
        /// Intent - zone for attack cards during combat.
        /// Object-specific zone attached to units.
        /// </summary>
        Intent,

        /// <summary>
        /// Effects Stack - where activated cards and abilities resolve.
        /// Shared public zone, LIFO order.
        /// </summary>
        EffectsStack
    }

    /// <summary>
    /// Visibility state of cards in zones.
    /// </summary>
    public enum CardVisibility
    {
        /// <summary>Card front is visible to all players</summary>
        FaceUp,

        /// <summary>Card back is visible, content hidden</summary>
        FaceDown
    }
}
