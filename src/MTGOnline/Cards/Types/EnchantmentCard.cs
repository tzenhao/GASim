using MTGOnline.Core.Enums;
using MTGOnline.Cards.Base;
using MTGOnline.Interfaces;

namespace MTGOnline.Cards.Types
{
    /// <summary>
    /// Represents an enchantment card in Magic: The Gathering.
    /// </summary>
    public class EnchantmentCard : Permanent, IEnchantment
    {
        public EnchantmentCard(string name) : base(name)
        {
            CardTypes = CardType.Enchantment;
        }
    }

    /// <summary>
    /// Represents an aura card (enchantment that attaches to permanents or players).
    /// </summary>
    public class AuraCard : EnchantmentCard, IAura
    {
        public IPermanent? EnchantedPermanent { get; private set; }
        public IPlayer? EnchantedPlayer { get; private set; }

        // What this aura can enchant
        public TargetType EnchantTarget { get; set; }

        public AuraCard(string name) : base(name)
        {
            AddSubType("Aura");
        }

        public void AttachTo(IPermanent permanent)
        {
            Detach();
            EnchantedPermanent = permanent;
            EnchantedPlayer = null;

            if (permanent is Permanent p)
            {
                p.AttachAura(this);
            }
        }

        public void AttachTo(IPlayer player)
        {
            Detach();
            EnchantedPlayer = player;
            EnchantedPermanent = null;
        }

        public void Detach()
        {
            if (EnchantedPermanent is Permanent p)
            {
                p.DetachAura(this);
            }
            EnchantedPermanent = null;
            EnchantedPlayer = null;
        }

        public override void OnLeaveBattlefield(IGameState gameState)
        {
            base.OnLeaveBattlefield(gameState);
            Detach();
        }

        public bool IsAttached => EnchantedPermanent != null || EnchantedPlayer != null;
    }

    /// <summary>
    /// Represents an enchantment creature.
    /// </summary>
    public class EnchantmentCreatureCard : CreatureCard
    {
        public EnchantmentCreatureCard(string name, int power, int toughness) : base(name, power, toughness)
        {
            CardTypes = CardType.Enchantment | CardType.Creature;
        }
    }
}
