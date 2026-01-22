using MTGOnline.Core.Enums;
using MTGOnline.Cards.Base;
using MTGOnline.Interfaces;
using MTGOnline.Mana;

namespace MTGOnline.Cards.Types
{
    /// <summary>
    /// Represents an artifact card in Magic: The Gathering.
    /// </summary>
    public class ArtifactCard : Permanent, IArtifact
    {
        public ArtifactCard(string name) : base(name)
        {
            CardTypes = CardType.Artifact;
        }
    }

    /// <summary>
    /// Represents an equipment card (subtype of artifact).
    /// </summary>
    public class EquipmentCard : ArtifactCard, IEquipment
    {
        public ICreature? EquippedCreature { get; private set; }
        public ManaCost EquipCost { get; set; }

        // Bonuses when equipped
        public int PowerBonus { get; set; }
        public int ToughnessBonus { get; set; }
        public KeywordAbility KeywordsGranted { get; set; }

        public EquipmentCard(string name) : base(name)
        {
            AddSubType("Equipment");
            EquipCost = new ManaCost();
        }

        public void AttachTo(ICreature creature)
        {
            // Unattach from current creature if any
            Detach();

            EquippedCreature = creature;

            // Apply bonuses
            if (creature is Cards.Types.CreatureCard creatureCard)
            {
                creatureCard.AddKeyword(KeywordsGranted);
            }
        }

        public void Detach()
        {
            if (EquippedCreature != null)
            {
                // Remove bonuses
                if (EquippedCreature is Cards.Types.CreatureCard creatureCard)
                {
                    creatureCard.RemoveKeyword(KeywordsGranted);
                }
                EquippedCreature = null;
            }
        }

        public override void OnLeaveBattlefield(IGameState gameState)
        {
            base.OnLeaveBattlefield(gameState);
            Detach();
        }
    }

    /// <summary>
    /// Represents an artifact creature.
    /// </summary>
    public class ArtifactCreatureCard : CreatureCard
    {
        public ArtifactCreatureCard(string name, int power, int toughness) : base(name, power, toughness)
        {
            CardTypes = CardType.Artifact | CardType.Creature;
        }
    }
}
