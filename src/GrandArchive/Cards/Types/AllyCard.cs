using GrandArchive.Cards.Base;
using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Cards.Types
{
    /// <summary>
    /// Ally card in Grand Archive TCG.
    /// Allies are support units that can attack and defend.
    /// They enter the field from the hand by paying their reserve cost.
    /// </summary>
    public class AllyCard : UnitCard
    {
        public override CardType CardType => CardType.Ally;

        /// <summary>
        /// The intent zone for this ally (holds attack cards during combat).
        /// </summary>
        public List<ICard> Intent { get; } = new();

        /// <summary>
        /// Whether this ally is currently attacking.
        /// </summary>
        public bool IsAttacking { get; set; }

        /// <summary>
        /// Whether this ally is currently defending.
        /// </summary>
        public bool IsDefending { get; set; }

        public AllyCard()
        {
            CostType = CostType.Reserve;
        }

        /// <summary>
        /// Add an attack card to this ally's intent.
        /// </summary>
        public void AddToIntent(ICard attackCard)
        {
            if (attackCard.CardType == CardType.Attack)
            {
                Intent.Add(attackCard);
            }
        }

        /// <summary>
        /// Clear the intent zone after combat.
        /// </summary>
        public void ClearIntent()
        {
            Intent.Clear();
        }

        /// <summary>
        /// Calculate total attack power including intent cards.
        /// </summary>
        public int GetTotalAttackPower()
        {
            int total = Power;
            foreach (var card in Intent)
            {
                if (card is AttackCard attack)
                {
                    total += attack.Power;
                }
            }
            return total;
        }

        /// <summary>
        /// Check if this ally can perform a command attack.
        /// </summary>
        public bool CanPerformCommand(AttackCard commandAttack)
        {
            if (State != UnitState.Awake || IsDefeated)
                return false;

            // Check if the command requires a specific subtype
            // This would be checked against the ally's subtypes
            return true;
        }

        public override string ToString() => $"{Name} (Ally) {Power}/{Life} [{State}]";
    }
}
