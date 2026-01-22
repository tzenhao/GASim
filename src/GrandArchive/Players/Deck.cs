using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Players
{
    /// <summary>
    /// Represents a deck configuration for Grand Archive TCG.
    /// Used for deck building and validation.
    /// </summary>
    public class DeckConfiguration
    {
        /// <summary>
        /// Name of the deck.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Cards in the main deck.
        /// </summary>
        public List<ICard> MainDeckCards { get; } = new();

        /// <summary>
        /// Cards in the material deck.
        /// </summary>
        public List<ICard> MaterialDeckCards { get; } = new();

        /// <summary>
        /// Minimum main deck size (typically 60).
        /// </summary>
        public int MinMainDeckSize { get; set; } = 60;

        /// <summary>
        /// Maximum main deck size.
        /// </summary>
        public int MaxMainDeckSize { get; set; } = 60;

        /// <summary>
        /// Maximum copies of a card (typically 4, 1 for Unique).
        /// </summary>
        public int MaxCopiesPerCard { get; set; } = 4;

        /// <summary>
        /// Maximum material deck size (typically 12).
        /// </summary>
        public int MaxMaterialDeckSize { get; set; } = 12;

        /// <summary>
        /// Validate the deck configuration.
        /// </summary>
        public DeckValidationResult Validate()
        {
            var result = new DeckValidationResult();

            // Check main deck size
            if (MainDeckCards.Count < MinMainDeckSize)
            {
                result.AddError($"Main deck has {MainDeckCards.Count} cards, minimum is {MinMainDeckSize}");
            }
            if (MainDeckCards.Count > MaxMainDeckSize)
            {
                result.AddError($"Main deck has {MainDeckCards.Count} cards, maximum is {MaxMainDeckSize}");
            }

            // Check material deck size
            if (MaterialDeckCards.Count > MaxMaterialDeckSize)
            {
                result.AddError($"Material deck has {MaterialDeckCards.Count} cards, maximum is {MaxMaterialDeckSize}");
            }

            // Check for champion
            var champions = MaterialDeckCards.Where(c => c.CardType == CardType.Champion).ToList();
            if (champions.Count == 0)
            {
                result.AddError("Material deck must contain exactly one champion");
            }
            else if (champions.Count > 1)
            {
                result.AddError("Material deck can only contain one champion");
            }

            // Check card copy limits
            var allCards = MainDeckCards.Concat(MaterialDeckCards);
            var cardCounts = allCards.GroupBy(c => c.Name).ToDictionary(g => g.Key, g => g.Count());

            foreach (var kvp in cardCounts)
            {
                var card = allCards.First(c => c.Name == kvp.Key);
                int maxCopies = card.Supertype == Supertype.Unique ? 1 : MaxCopiesPerCard;

                if (kvp.Value > maxCopies)
                {
                    result.AddError($"Too many copies of {kvp.Key}: {kvp.Value} (max {maxCopies})");
                }
            }

            // Check Divine Relic limit (only one per material deck)
            var divineRelics = MaterialDeckCards.Where(c => c.HasKeyword(Keyword.DivineRelic)).ToList();
            if (divineRelics.Count > 1)
            {
                result.AddError("Material deck can only contain one Divine Relic");
            }

            return result;
        }

        /// <summary>
        /// Load the deck into a player's zones.
        /// </summary>
        public void LoadIntoPlayer(Player player)
        {
            // Load main deck
            foreach (var card in MainDeckCards)
            {
                card.Owner = player;
                card.Controller = player;
                player.MainDeckZone.Add(card);
            }

            // Load material deck
            foreach (var card in MaterialDeckCards)
            {
                card.Owner = player;
                card.Controller = player;
                player.MaterialDeckZone.Add(card);
            }

            // Shuffle main deck
            player.MainDeckZone.Shuffle();
        }
    }

    /// <summary>
    /// Result of deck validation.
    /// </summary>
    public class DeckValidationResult
    {
        public List<string> Errors { get; } = new();
        public List<string> Warnings { get; } = new();

        public bool IsValid => Errors.Count == 0;

        public void AddError(string error) => Errors.Add(error);
        public void AddWarning(string warning) => Warnings.Add(warning);

        public override string ToString()
        {
            if (IsValid)
                return "Deck is valid";

            return $"Deck has {Errors.Count} error(s):\n" + string.Join("\n", Errors);
        }
    }
}
