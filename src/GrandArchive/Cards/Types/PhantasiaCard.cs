using GrandArchive.Cards.Base;
using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Cards.Types
{
    /// <summary>
    /// Phantasia card in Grand Archive TCG.
    /// Phantasias are special manifestation cards with unique effects.
    /// </summary>
    public class PhantasiaCard : Card
    {
        public override CardType CardType => CardType.Phantasia;

        /// <summary>
        /// The effect this phantasia provides.
        /// </summary>
        public Action<IGameState>? Effect { get; init; }

        /// <summary>
        /// Duration of this phantasia's effect (in turns, 0 = permanent until removed).
        /// </summary>
        public int Duration { get; init; }

        /// <summary>
        /// Remaining duration.
        /// </summary>
        public int RemainingDuration { get; set; }

        public PhantasiaCard()
        {
            CostType = CostType.Reserve;
        }

        /// <summary>
        /// Initialize the phantasia when it enters the field.
        /// </summary>
        public void Initialize()
        {
            RemainingDuration = Duration;
        }

        /// <summary>
        /// Tick down the duration at end of turn.
        /// </summary>
        public bool TickDuration()
        {
            if (Duration == 0) // Permanent
                return true;

            RemainingDuration--;
            return RemainingDuration > 0;
        }

        /// <summary>
        /// Check if this phantasia has expired.
        /// </summary>
        public bool HasExpired => Duration > 0 && RemainingDuration <= 0;

        public override string ToString() => $"{Name} (Phantasia, Cost: {Cost})";
    }
}
