namespace GrandArchive.GUI.Data
{
    /// <summary>
    /// Represents a card instance in a game, wrapping the static CardData.
    /// </summary>
    public class GameCard
    {
        public CardData Data { get; }
        public string EditionSlug { get; }
        public string? ImagePath { get; set; }

        // Game state
        public int CurrentPower { get; set; }
        public int CurrentLife { get; set; }
        public int CurrentDurability { get; set; }
        public int DamageCounters { get; set; }
        public bool IsRested { get; set; }
        public bool IsAwakened { get; set; } = true;
        public int Level { get; set; } = 1;
        public int LevelCounters { get; set; }

        // Zone tracking
        public string CurrentZone { get; set; } = "Deck";
        public int? OwnerId { get; set; }
        public int? ControllerId { get; set; }

        public GameCard(CardData data, string? editionSlug = null)
        {
            Data = data;
            EditionSlug = editionSlug ?? data.FirstEditionSlug ?? data.Slug;
            
            // Initialize stats from card data
            CurrentPower = data.Power ?? 0;
            CurrentLife = data.Life ?? 0;
            CurrentDurability = data.Durability ?? 0;
            Level = data.Level ?? 1;
        }

        // Convenience properties
        public string Name => Data.Name;
        public string Slug => Data.Slug;
        public string CardType => Data.CardType;
        public int Cost => Data.Cost;
        public string? Element => Data.Element;
        public string? Effect => Data.Effect;
        public bool IsChampion => Data.IsChampion;
        public bool IsAlly => Data.IsAlly;
        public bool IsAction => Data.IsAction;
        public bool IsAttack => Data.IsAttack;
        public bool IsUnit => IsChampion || IsAlly;

        // Combat
        public int EffectivePower => CurrentPower;
        public int EffectiveLife => CurrentLife - DamageCounters;
        public bool IsDefeated => IsUnit && EffectiveLife <= 0;

        public void TakeDamage(int amount)
        {
            if (amount > 0)
            {
                DamageCounters += amount;
            }
        }

        public void Heal(int amount)
        {
            if (amount > 0)
            {
                DamageCounters = Math.Max(0, DamageCounters - amount);
            }
        }

        public void Rest()
        {
            IsRested = true;
        }

        public void Ready()
        {
            IsRested = false;
        }

        public override string ToString() => $"{Name} ({CardType})";
    }
}
