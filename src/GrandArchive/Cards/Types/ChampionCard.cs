using GrandArchive.Cards.Base;
using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Cards.Types
{
    /// <summary>
    /// Champion card in Grand Archive TCG.
    /// Champions are the player's representative in the game.
    /// They start in the Material Deck and are materialized at game start.
    /// </summary>
    public class ChampionCard : UnitCard, IChampion
    {
        public override CardType CardType => CardType.Champion;

        /// <summary>
        /// Champion's class(es) - determines which Class Bonuses are active.
        /// </summary>
        public IReadOnlyList<ChampionClass> Classes { get; init; } = Array.Empty<ChampionClass>();

        /// <summary>
        /// Champion's lineage name - used for Champion Bonus abilities.
        /// </summary>
        public string LineageName { get; init; } = string.Empty;

        /// <summary>
        /// Champion's current level.
        /// </summary>
        public int Level { get; set; } = 1;

        /// <summary>
        /// Number of level counters on this champion.
        /// </summary>
        public int LevelCounters { get; set; }

        /// <summary>
        /// Whether this champion has used their mastery ability this game.
        /// </summary>
        public bool HasUsedMastery { get; set; }

        /// <summary>
        /// The mastery ability of this champion.
        /// </summary>
        public IAbility? MasteryAbility { get; init; }

        public ChampionCard()
        {
            // Champions are materialized from Material Deck
            CurrentZone = ZoneType.MaterialDeck;
            CostType = CostType.None;
        }

        /// <summary>
        /// Check if this champion has a specific class.
        /// </summary>
        public bool HasClass(ChampionClass championClass) => Classes.Contains(championClass);

        /// <summary>
        /// Add level counters to this champion.
        /// </summary>
        public void AddLevelCounters(int count)
        {
            if (count > 0)
            {
                LevelCounters += count;
            }
        }

        /// <summary>
        /// Level up this champion if they have enough level counters.
        /// </summary>
        public bool TryLevelUp(int requiredCounters)
        {
            if (LevelCounters >= requiredCounters)
            {
                LevelCounters -= requiredCounters;
                Level++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Activate the mastery ability (if available and not used).
        /// </summary>
        public bool ActivateMastery(IGameState gameState)
        {
            if (HasUsedMastery || MasteryAbility == null)
                return false;

            if (!MasteryAbility.CanActivate(gameState))
                return false;

            MasteryAbility.Activate(gameState);
            HasUsedMastery = true;
            return true;
        }

        public override string ToString() => $"{Name} (Champion, {string.Join("/", Classes)}) [{Level}] {Power}/{Life}";
    }
}
