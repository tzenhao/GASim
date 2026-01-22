using MTGOnline.Core.Enums;
using MTGOnline.Zones;
using MTGOnline.Mana;

namespace MTGOnline.Interfaces
{
    /// <summary>
    /// Interface representing a player in the game.
    /// </summary>
    public interface IPlayer
    {
        Guid Id { get; }
        string Name { get; }

        // Life and Resources
        int Life { get; }
        int StartingLife { get; }
        ManaPool ManaPool { get; }
        int PoisonCounters { get; }
        int EnergyCounters { get; }
        int ExperienceCounters { get; }

        // Zones
        IZone Library { get; }
        IZone Hand { get; }
        IZone Graveyard { get; }
        IZone Exile { get; }
        IZone CommandZone { get; }
        IZone Sideboard { get; }

        // Turn-based state
        bool HasPlayedLandThisTurn { get; }
        int LandsPlayedThisTurn { get; }
        int MaxLandsPerTurn { get; }
        bool HasPriority { get; }

        // Game state
        GameResult GameResult { get; }
        bool HasLost { get; }
        bool HasWon { get; }
        bool HasConceded { get; }

        // Actions
        void GainLife(int amount);
        void LoseLife(int amount);
        void SetLife(int amount);
        void DealDamage(int amount, ICard source, bool isCombatDamage = false);
        void AddPoisonCounters(int amount);
        void AddEnergyCounters(int amount);
        void RemoveEnergyCounters(int amount);

        // Card Actions
        void DrawCards(int count);
        void DiscardCards(int count);
        void DiscardCard(ICard card);
        void Mill(int count);
        void Shuffle();
        void SearchLibrary(Func<ICard, bool> predicate, int count = 1);

        // Land Actions
        bool CanPlayLand();
        void PlayLand(ICard land);
        void ResetLandPlays();

        // Game Actions
        void Concede();
        void LoseGame(WinLossCondition reason);
        void WinGame(WinLossCondition reason);
    }
}
