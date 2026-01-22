using GrandArchive.Cards.Base;
using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Cards.Types
{
    /// <summary>
    /// Action card in Grand Archive TCG.
    /// Actions are one-time effect cards that go to the graveyard after resolving.
    /// </summary>
    public class ActionCard : Card
    {
        public override CardType CardType => CardType.Action;

        /// <summary>
        /// The effect this action performs when resolved.
        /// </summary>
        public Action<IGameState>? Effect { get; init; }

        public ActionCard()
        {
            CostType = CostType.Reserve;
        }

        /// <summary>
        /// Resolve this action's effect.
        /// </summary>
        public void Resolve(IGameState gameState)
        {
            Effect?.Invoke(gameState);
        }

        public override bool CanPlay(IGameState gameState)
        {
            if (!base.CanPlay(gameState))
                return false;

            // Fast actions can be played when player has opportunity
            if (Speed == SpeedType.Fast)
                return Controller?.HasOpportunity ?? false;

            // Standard actions can only be played during main phase
            return gameState.CurrentPhase == PhaseType.Main;
        }

        public override string ToString() => $"{Name} (Action, Cost: {Cost})";
    }
}
