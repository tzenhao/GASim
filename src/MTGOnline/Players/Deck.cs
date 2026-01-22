using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;

namespace MTGOnline.Players
{
    /// <summary>
    /// Represents a deck of cards.
    /// </summary>
    public class Deck
    {
        public string Name { get; set; }
        public DeckFormat Format { get; set; }

        private readonly List<ICard> _mainDeck = new();
        public IReadOnlyList<ICard> MainDeck => _mainDeck.AsReadOnly();

        private readonly List<ICard> _sideboard = new();
        public IReadOnlyList<ICard> Sideboard => _sideboard.AsReadOnly();

        // For Commander format
        public ICard? Commander { get; set; }
        public ICard? PartnerCommander { get; set; }

        public int MainDeckCount => _mainDeck.Count;
        public int SideboardCount => _sideboard.Count;

        public Deck(string name, DeckFormat format = DeckFormat.Standard)
        {
            Name = name;
            Format = format;
        }

        public void AddToMainDeck(ICard card)
        {
            _mainDeck.Add(card);
        }

        public void AddToMainDeck(ICard card, int count)
        {
            for (int i = 0; i < count; i++)
            {
                // In real implementation, would create copies of the card
                _mainDeck.Add(card);
            }
        }

        public void RemoveFromMainDeck(ICard card)
        {
            _mainDeck.Remove(card);
        }

        public void AddToSideboard(ICard card)
        {
            _sideboard.Add(card);
        }

        public void RemoveFromSideboard(ICard card)
        {
            _sideboard.Remove(card);
        }

        /// <summary>
        /// Validates the deck according to format rules.
        /// </summary>
        public DeckValidationResult Validate()
        {
            var result = new DeckValidationResult();

            switch (Format)
            {
                case DeckFormat.Standard:
                case DeckFormat.Modern:
                case DeckFormat.Pioneer:
                case DeckFormat.Legacy:
                case DeckFormat.Vintage:
                    ValidateConstructed(result);
                    break;
                case DeckFormat.Commander:
                    ValidateCommander(result);
                    break;
                case DeckFormat.Limited:
                    ValidateLimited(result);
                    break;
            }

            return result;
        }

        private void ValidateConstructed(DeckValidationResult result)
        {
            // Minimum 60 cards
            if (MainDeckCount < 60)
            {
                result.AddError($"Main deck must have at least 60 cards (has {MainDeckCount})");
            }

            // Maximum 15 sideboard
            if (SideboardCount > 15)
            {
                result.AddError($"Sideboard can have at most 15 cards (has {SideboardCount})");
            }

            // Max 4 copies of non-basic lands
            ValidateCardCopies(result, 4);
        }

        private void ValidateCommander(DeckValidationResult result)
        {
            // Exactly 100 cards including commander
            int totalCards = MainDeckCount + (Commander != null ? 1 : 0) + (PartnerCommander != null ? 1 : 0);
            if (totalCards != 100)
            {
                result.AddError($"Commander deck must have exactly 100 cards (has {totalCards})");
            }

            // Commander must be legendary creature or specified planeswalker
            if (Commander != null && !Commander.HasSuperType(SuperType.Legendary))
            {
                result.AddError("Commander must be a legendary creature");
            }

            // Singleton format (except basic lands)
            ValidateCardCopies(result, 1);

            // All cards must be within commander's color identity
            // (In full implementation)
        }

        private void ValidateLimited(DeckValidationResult result)
        {
            // Minimum 40 cards
            if (MainDeckCount < 40)
            {
                result.AddError($"Limited deck must have at least 40 cards (has {MainDeckCount})");
            }
        }

        private void ValidateCardCopies(DeckValidationResult result, int maxCopies)
        {
            var cardCounts = new Dictionary<string, int>();

            foreach (var card in _mainDeck.Concat(_sideboard))
            {
                // Basic lands are exempt
                if (card.HasSuperType(SuperType.Basic) && card.HasCardType(CardType.Land))
                    continue;

                if (!cardCounts.ContainsKey(card.Name))
                {
                    cardCounts[card.Name] = 0;
                }
                cardCounts[card.Name]++;
            }

            foreach (var kvp in cardCounts)
            {
                if (kvp.Value > maxCopies)
                {
                    result.AddError($"Too many copies of {kvp.Key} (has {kvp.Value}, max {maxCopies})");
                }
            }
        }

        /// <summary>
        /// Gets a shuffled copy of the main deck for starting a game.
        /// </summary>
        public List<ICard> GetShuffledDeck()
        {
            var deck = new List<ICard>(_mainDeck);
            var random = new Random();
            int n = deck.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                (deck[k], deck[n]) = (deck[n], deck[k]);
            }
            return deck;
        }
    }

    /// <summary>
    /// Deck formats in Magic: The Gathering.
    /// </summary>
    public enum DeckFormat
    {
        Standard,
        Modern,
        Pioneer,
        Legacy,
        Vintage,
        Commander,
        Pauper,
        Limited,
        Draft,
        Sealed,
        Brawl,
        Historic,
        Alchemy
    }

    /// <summary>
    /// Result of deck validation.
    /// </summary>
    public class DeckValidationResult
    {
        private readonly List<string> _errors = new();
        private readonly List<string> _warnings = new();

        public bool IsValid => _errors.Count == 0;
        public IReadOnlyList<string> Errors => _errors.AsReadOnly();
        public IReadOnlyList<string> Warnings => _warnings.AsReadOnly();

        public void AddError(string error)
        {
            _errors.Add(error);
        }

        public void AddWarning(string warning)
        {
            _warnings.Add(warning);
        }
    }
}
