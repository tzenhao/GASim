using GrandArchive.Cards.Base;
using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Cards.Types
{
    /// <summary>
    /// Attack card in Grand Archive TCG.
    /// Attacks are combat cards used to deal damage.
    /// They require resting a champion (or ally for Command attacks) to activate.
    /// </summary>
    public class AttackCard : Card, IHasStats
    {
        public override CardType CardType => CardType.Attack;

        /// <summary>
        /// Base power of this attack.
        /// </summary>
        public int BasePower { get; init; }

        /// <summary>
        /// Current power (after modifications).
        /// </summary>
        public int Power => BasePower + PowerModifier;

        /// <summary>
        /// Temporary power modifier from effects.
        /// </summary>
        public int PowerModifier { get; set; }

        /// <summary>
        /// Attacks don't have life, but interface requires it.
        /// </summary>
        public int BaseLife => 0;
        public int Life => 0;

        /// <summary>
        /// Whether this is a Command attack (performed by an ally).
        /// </summary>
        public bool IsCommand => HasKeyword(Keyword.Command);

        /// <summary>
        /// The subtype of ally required for Command attacks (if any).
        /// </summary>
        public Subtype? CommandSubtype { get; init; }

        public AttackCard()
        {
            CostType = CostType.Reserve;
        }

        /// <summary>
        /// Check if this attack can be activated.
        /// </summary>
        public override bool CanPlay(IGameState gameState)
        {
            if (!base.CanPlay(gameState))
                return false;

            // Attacks can only be played during main phase
            if (gameState.CurrentPhase != PhaseType.Main)
                return false;

            // Check if the attacker (champion or ally) is available
            if (Controller?.Champion == null)
                return false;

            // For regular attacks, champion must be awake
            if (!IsCommand && Controller.Champion.State != UnitState.Awake)
                return false;

            return true;
        }

        /// <summary>
        /// Check if this attack has Cleave (attacks all enemies).
        /// </summary>
        public bool HasCleave => HasKeyword(Keyword.Cleave);

        /// <summary>
        /// Check if this attack has Stealth (can't be intercepted).
        /// </summary>
        public bool HasStealth => HasKeyword(Keyword.Stealth);

        public override string ToString() => $"{Name} (Attack, Power: {Power}, Cost: {Cost})";
    }
}
