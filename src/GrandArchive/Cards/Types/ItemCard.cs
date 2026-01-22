using GrandArchive.Cards.Base;
using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Cards.Types
{
    /// <summary>
    /// Item card in Grand Archive TCG.
    /// Items are equipment and consumable cards that provide effects.
    /// </summary>
    public class ItemCard : Card
    {
        public override CardType CardType => CardType.Item;

        /// <summary>
        /// Whether this item is an artifact (stays on field).
        /// </summary>
        public bool IsArtifact => HasSubtype(Subtype.Artifact);

        /// <summary>
        /// Whether this item is a potion (consumable).
        /// </summary>
        public bool IsPotion => HasSubtype(Subtype.Potion);

        /// <summary>
        /// Whether this item is an ingredient (used for brewing).
        /// </summary>
        public bool IsIngredient => HasSubtype(Subtype.Ingredient);

        /// <summary>
        /// The effect this item provides.
        /// </summary>
        public Action<IGameState>? Effect { get; init; }

        public ItemCard()
        {
            CostType = CostType.Reserve;
        }

        /// <summary>
        /// Activate this item's effect.
        /// </summary>
        public void Activate(IGameState gameState)
        {
            Effect?.Invoke(gameState);
        }

        public override string ToString() => $"{Name} (Item, Cost: {Cost})";
    }
}
