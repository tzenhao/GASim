using System.Text.Json.Serialization;

namespace GrandArchive.GUI.Data
{
    /// <summary>
    /// Represents a card loaded from the JSON database.
    /// </summary>
    public class CardData
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = string.Empty;

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("types")]
        public List<string> Types { get; set; } = new();

        [JsonPropertyName("subtypes")]
        public List<string> Subtypes { get; set; } = new();

        [JsonPropertyName("classes")]
        public List<string> Classes { get; set; } = new();

        [JsonPropertyName("element")]
        public string? Element { get; set; }

        [JsonPropertyName("elements")]
        public List<string> Elements { get; set; } = new();

        [JsonPropertyName("cost_memory")]
        public int? CostMemory { get; set; }

        [JsonPropertyName("cost_reserve")]
        public int? CostReserve { get; set; }

        [JsonPropertyName("power")]
        public int? Power { get; set; }

        [JsonPropertyName("life")]
        public int? Life { get; set; }

        [JsonPropertyName("durability")]
        public int? Durability { get; set; }

        [JsonPropertyName("level")]
        public int? Level { get; set; }

        [JsonPropertyName("speed")]
        public bool? Speed { get; set; }

        [JsonPropertyName("effect")]
        public string? Effect { get; set; }

        [JsonPropertyName("effect_raw")]
        public string? EffectRaw { get; set; }

        [JsonPropertyName("flavor")]
        public string? Flavor { get; set; }

        [JsonPropertyName("editions")]
        public List<EditionData> Editions { get; set; } = new();

        // Computed properties
        public bool IsChampion => Types.Contains("CHAMPION");
        public bool IsAlly => Types.Contains("ALLY");
        public bool IsAction => Types.Contains("ACTION");
        public bool IsAttack => Types.Contains("ATTACK");
        public bool IsItem => Types.Contains("ITEM");
        public bool IsRegalia => Types.Contains("REGALIA");
        public bool IsWeapon => Types.Contains("WEAPON");
        public bool IsDomain => Types.Contains("DOMAIN");
        public bool IsToken => Types.Contains("TOKEN");
        public bool IsPhantasia => Types.Contains("PHANTASIA");

        /// <summary>
        /// Divine Relic cards can only have 1 in a material deck.
        /// Identified by "Divine Relic" keyword in effect text.
        /// </summary>
        public bool IsDivineRelic => Effect?.Contains("Divine Relic") == true || EffectRaw?.Contains("Divine Relic") == true;

        /// <summary>
        /// Champion Spirit - a champion with Spirit class or subtype.
        /// Required to have at least one in a material deck.
        /// </summary>
        public bool IsChampionSpirit => IsChampion && (Classes.Contains("SPIRIT") || Subtypes.Contains("SPIRIT"));

        /// <summary>
        /// Can be added to material deck (regalia and champions).
        /// </summary>
        public bool IsMaterialDeckCard => IsRegalia || IsChampion;

        /// <summary>
        /// Can be added to main deck (non-token, non-regalia, non-champion cards).
        /// </summary>
        public bool IsMainDeckCard => !IsToken && !IsRegalia && !IsChampion;

        public string CardType => Types.FirstOrDefault() ?? "Unknown";
        public int Cost => CostMemory ?? CostReserve ?? 0;
        public string CostTypeDisplay => CostMemory.HasValue ? "Memory" : "Reserve";

        /// <summary>
        /// Get the first edition's image slug for finding the image file.
        /// </summary>
        public string? FirstEditionSlug => Editions.FirstOrDefault()?.Slug;
    }

    /// <summary>
    /// Represents a card edition (printing) from the JSON database.
    /// </summary>
    public class EditionData
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = string.Empty;

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("card_id")]
        public string CardId { get; set; } = string.Empty;

        [JsonPropertyName("collector_number")]
        public string? CollectorNumber { get; set; }

        [JsonPropertyName("rarity")]
        public int Rarity { get; set; }

        [JsonPropertyName("illustrator")]
        public string? Illustrator { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("flavor")]
        public string? Flavor { get; set; }

        [JsonPropertyName("effect")]
        public string? Effect { get; set; }

        [JsonPropertyName("effect_raw")]
        public string? EffectRaw { get; set; }

        [JsonPropertyName("set")]
        public SetData? Set { get; set; }

        public string RarityName => Rarity switch
        {
            1 => "Common",
            2 => "Uncommon",
            3 => "Rare",
            4 => "Super Rare",
            5 => "Ultra Rare",
            6 => "Promo",
            7 => "Collector Rare",
            8 => "Collector Ultra Rare",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Represents a card set from the JSON database.
    /// </summary>
    public class SetData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("prefix")]
        public string Prefix { get; set; } = string.Empty;

        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }
    }
}
