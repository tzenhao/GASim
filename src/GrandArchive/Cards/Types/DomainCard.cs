using GrandArchive.Cards.Base;
using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Cards.Types
{
    /// <summary>
    /// Domain card in Grand Archive TCG.
    /// Domains are location cards that provide ongoing effects.
    /// Siegeable domains can be attacked and have life.
    /// </summary>
    public class DomainCard : Card, IHasStats
    {
        public override CardType CardType => CardType.Domain;

        /// <summary>
        /// Domains don't have power (unless they can attack).
        /// </summary>
        public int BasePower { get; init; }
        public int Power => BasePower;

        /// <summary>
        /// Base life for siegeable domains.
        /// </summary>
        public int BaseLife { get; init; }

        /// <summary>
        /// Current life (after modifications).
        /// </summary>
        public int Life => BaseLife + LifeModifier - DamageCounters;

        /// <summary>
        /// Temporary life modifier from effects.
        /// </summary>
        public int LifeModifier { get; set; }

        /// <summary>
        /// Damage counters on this domain.
        /// </summary>
        public int DamageCounters { get; set; }

        /// <summary>
        /// Whether this domain is siegeable (can be attacked).
        /// </summary>
        public bool IsSiegeable => HasSubtype(Subtype.Siegeable);

        /// <summary>
        /// Whether this domain has been destroyed.
        /// </summary>
        public bool IsDestroyed => IsSiegeable && DamageCounters >= BaseLife;

        /// <summary>
        /// The continuous effect this domain provides.
        /// </summary>
        public Action<IGameState>? ContinuousEffect { get; init; }

        public DomainCard()
        {
            CostType = CostType.Reserve;
        }

        /// <summary>
        /// Deal damage to this domain (if siegeable).
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (IsSiegeable && amount > 0)
            {
                DamageCounters += amount;
            }
        }

        public override string ToString()
        {
            if (IsSiegeable)
                return $"{Name} (Domain - Siegeable, Life: {Life}/{BaseLife})";
            return $"{Name} (Domain)";
        }
    }
}
