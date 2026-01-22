using System.IO;
using System.Text.Json;

namespace GrandArchive.GUI.Data
{
    /// <summary>
    /// Database for loading and querying cards from JSON files.
    /// </summary>
    public class CardDatabase
    {
        private readonly Dictionary<string, CardData> _cardsBySlug = new();
        private readonly Dictionary<string, CardData> _cardsByUuid = new();
        private readonly Dictionary<string, List<CardData>> _cardsByType = new();
        private readonly Dictionary<string, List<CardData>> _cardsByElement = new();
        private readonly List<CardData> _allCards = new();

        public IReadOnlyList<CardData> AllCards => _allCards;
        public int Count => _allCards.Count;
        public bool IsLoaded => _allCards.Count > 0;

        public string CardsFolder { get; private set; } = string.Empty;
        public string ImagesFolder { get; private set; } = string.Empty;

        /// <summary>
        /// Load all cards from the Cards folder.
        /// </summary>
        public async Task LoadCardsAsync(string cardsFolder, string imagesFolder)
        {
            CardsFolder = cardsFolder;
            ImagesFolder = imagesFolder;

            _cardsBySlug.Clear();
            _cardsByUuid.Clear();
            _cardsByType.Clear();
            _cardsByElement.Clear();
            _allCards.Clear();

            if (!Directory.Exists(cardsFolder))
            {
                throw new DirectoryNotFoundException($"Cards folder not found: {cardsFolder}");
            }

            var jsonFiles = Directory.GetFiles(cardsFolder, "*.json");
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            foreach (var file in jsonFiles)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var card = JsonSerializer.Deserialize<CardData>(json, options);

                    if (card != null && !string.IsNullOrEmpty(card.Slug))
                    {
                        _allCards.Add(card);
                        _cardsBySlug[card.Slug] = card;
                        _cardsByUuid[card.Uuid] = card;

                        // Index by type
                        foreach (var type in card.Types)
                        {
                            if (!_cardsByType.ContainsKey(type))
                                _cardsByType[type] = new List<CardData>();
                            _cardsByType[type].Add(card);
                        }

                        // Index by element
                        foreach (var element in card.Elements)
                        {
                            if (!_cardsByElement.ContainsKey(element))
                                _cardsByElement[element] = new List<CardData>();
                            _cardsByElement[element].Add(card);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading {file}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get a card by its slug.
        /// </summary>
        public CardData? GetBySlug(string slug)
        {
            return _cardsBySlug.TryGetValue(slug, out var card) ? card : null;
        }

        /// <summary>
        /// Get a card by its UUID.
        /// </summary>
        public CardData? GetByUuid(string uuid)
        {
            return _cardsByUuid.TryGetValue(uuid, out var card) ? card : null;
        }

        /// <summary>
        /// Get all cards of a specific type.
        /// </summary>
        public IEnumerable<CardData> GetByType(string type)
        {
            return _cardsByType.TryGetValue(type.ToUpper(), out var cards) ? cards : Enumerable.Empty<CardData>();
        }

        /// <summary>
        /// Get all cards of a specific element.
        /// </summary>
        public IEnumerable<CardData> GetByElement(string element)
        {
            return _cardsByElement.TryGetValue(element.ToUpper(), out var cards) ? cards : Enumerable.Empty<CardData>();
        }

        /// <summary>
        /// Get all champion cards.
        /// </summary>
        public IEnumerable<CardData> GetChampions()
        {
            return GetByType("CHAMPION");
        }

        /// <summary>
        /// Get all ally cards.
        /// </summary>
        public IEnumerable<CardData> GetAllies()
        {
            return GetByType("ALLY");
        }

        /// <summary>
        /// Get all action cards.
        /// </summary>
        public IEnumerable<CardData> GetActions()
        {
            return GetByType("ACTION");
        }

        /// <summary>
        /// Get all attack cards.
        /// </summary>
        public IEnumerable<CardData> GetAttacks()
        {
            return GetByType("ATTACK");
        }

        /// <summary>
        /// Search cards by name.
        /// </summary>
        public IEnumerable<CardData> SearchByName(string query)
        {
            var lowerQuery = query.ToLower();
            return _allCards.Where(c => c.Name.ToLower().Contains(lowerQuery));
        }

        /// <summary>
        /// Get the image path for a card edition.
        /// </summary>
        public string? GetImagePath(string editionSlug)
        {
            var imagePath = Path.Combine(ImagesFolder, $"{editionSlug}.jpg");
            return File.Exists(imagePath) ? imagePath : null;
        }

        /// <summary>
        /// Get the image path for a card (uses first edition).
        /// </summary>
        public string? GetImagePath(CardData card)
        {
            var editionSlug = card.FirstEditionSlug;
            return editionSlug != null ? GetImagePath(editionSlug) : null;
        }

        /// <summary>
        /// Get random cards for deck building.
        /// </summary>
        public IEnumerable<CardData> GetRandomCards(int count, Func<CardData, bool>? filter = null)
        {
            var random = new Random();
            var source = filter != null ? _allCards.Where(filter).ToList() : _allCards;
            return source.OrderBy(_ => random.Next()).Take(count);
        }

        /// <summary>
        /// Build a simple deck with a champion and main deck cards.
        /// </summary>
        public (CardData champion, List<CardData> mainDeck) BuildRandomDeck(string? championSlug = null)
        {
            var random = new Random();
            
            // Get champion
            CardData champion;
            if (!string.IsNullOrEmpty(championSlug))
            {
                champion = GetBySlug(championSlug) ?? GetChampions().OrderBy(_ => random.Next()).First();
            }
            else
            {
                champion = GetChampions().OrderBy(_ => random.Next()).First();
            }

            // Build main deck (60 cards, no champions)
            var mainDeckCards = _allCards
                .Where(c => !c.IsChampion)
                .OrderBy(_ => random.Next())
                .Take(60)
                .ToList();

            return (champion, mainDeckCards);
        }
    }
}
