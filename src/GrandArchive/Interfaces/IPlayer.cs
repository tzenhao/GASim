using GrandArchive.Core.Enums;

namespace GrandArchive.Interfaces
{
    /// <summary>
    /// Interface for players in Grand Archive TCG.
    /// </summary>
    public interface IPlayer
    {
        /// <summary>Unique identifier for this player</summary>
        Guid Id { get; }

        /// <summary>Player's display name</summary>
        string Name { get; }

        /// <summary>The player's champion card</summary>
        IChampion? Champion { get; set; }

        /// <summary>Whether this player currently has Opportunity (can act)</summary>
        bool HasOpportunity { get; set; }

        /// <summary>Whether this player is the turn player</summary>
        bool IsTurnPlayer { get; set; }

        /// <summary>Player's main deck zone</summary>
        IZone MainDeck { get; }

        /// <summary>Player's material deck zone</summary>
        IZone MaterialDeck { get; }

        /// <summary>Player's hand zone</summary>
        IZone Hand { get; }

        /// <summary>Player's memory zone</summary>
        IZone Memory { get; }

        /// <summary>Player's graveyard zone</summary>
        IZone Graveyard { get; }

        /// <summary>Player's banishment zone</summary>
        IZone Banishment { get; }

        /// <summary>Draw a card from main deck to hand</summary>
        bool DrawCard();

        /// <summary>Draw multiple cards from main deck to hand</summary>
        int DrawCards(int count);

        /// <summary>Place a card from hand into memory (to pay costs)</summary>
        bool PlaceInMemory(ICard card);

        /// <summary>Return a card from memory to hand</summary>
        bool RecollectFromMemory(ICard card);

        /// <summary>Check if player has lost the game</summary>
        bool HasLost { get; }
    }

    /// <summary>
    /// Interface for champion cards.
    /// </summary>
    public interface IChampion : IUnit
    {
        /// <summary>Champion's class(es)</summary>
        IReadOnlyList<ChampionClass> Classes { get; }

        /// <summary>Champion's lineage name</summary>
        string LineageName { get; }

        /// <summary>Champion's level</summary>
        int Level { get; set; }

        /// <summary>Number of level counters on this champion</summary>
        int LevelCounters { get; set; }

        /// <summary>Whether this champion has used their mastery this game</summary>
        bool HasUsedMastery { get; set; }
    }
}
