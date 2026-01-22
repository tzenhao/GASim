using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GrandArchive.GUI.Data;
using Microsoft.Win32;

namespace GrandArchive.GUI
{
    /// <summary>
    /// Deck builder window for constructing material and main decks.
    /// </summary>
    public partial class DeckBuilderWindow : Window
    {
        private readonly CardDatabase _database;
        private readonly List<CardData> _filteredCards = new();
        
        // Deck contents - Material deck stores unique cards, Main deck stores (card, count) pairs
        private readonly List<CardData> _materialDeck = new();
        private readonly Dictionary<string, int> _mainDeckCounts = new(); // slug -> count
        private readonly List<CardData> _mainDeckCards = new(); // unique cards in main deck
        
        // Result deck for the game
        public List<CardData>? ResultMaterialDeck { get; private set; }
        public List<CardData>? ResultMainDeck { get; private set; }
        public bool DeckConfirmed { get; private set; }
        
        // Search debounce
        private DateTime _lastSearchTime = DateTime.MinValue;
        private string _lastSearchText = "";
        
        // Initialization flag to prevent events firing during setup
        private bool _isInitialized = false;

        public DeckBuilderWindow(CardDatabase database)
        {
            InitializeComponent();
            _database = database;
            
            // Mark as initialized before refreshing to allow the methods to run
            _isInitialized = true;
            
            RefreshCardBrowser();
            UpdateDeckDisplays();
            UpdateValidation();
        }

        #region Card Browser

        private void RefreshCardBrowser()
        {
            // Don't run if not initialized or UI elements aren't ready
            if (!_isInitialized || CardBrowserPanel == null || ResultsCountText == null)
                return;
                
            _filteredCards.Clear();
            
            var searchText = SearchTextBox?.Text?.ToLower() ?? "";
            var elementFilter = (ElementFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All";
            var typeFilter = (TypeFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All";
            
            IEnumerable<CardData> cards = _database.AllCards;
            
            // Apply category filter (radio buttons)
            if (FilterMainDeck?.IsChecked == true)
            {
                cards = cards.Where(c => c.IsMainDeckCard);
            }
            else if (FilterRegalia?.IsChecked == true)
            {
                cards = cards.Where(c => c.IsRegalia);
            }
            else if (FilterChampions?.IsChecked == true)
            {
                cards = cards.Where(c => c.IsChampion);
            }
            
            // Apply element filter
            if (elementFilter != "All")
            {
                cards = cards.Where(c => c.Element?.ToUpper() == elementFilter.ToUpper());
            }
            
            // Apply type filter
            if (typeFilter != "All")
            {
                cards = cards.Where(c => c.Types.Contains(typeFilter.ToUpper()));
            }
            
            // Apply search text
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                cards = cards.Where(c => 
                    c.Name.ToLower().Contains(searchText) ||
                    c.Effect?.ToLower().Contains(searchText) == true ||
                    c.Subtypes.Any(s => s.ToLower().Contains(searchText)) ||
                    c.Classes.Any(cl => cl.ToLower().Contains(searchText)));
            }
            
            // Sort by name
            _filteredCards.AddRange(cards.OrderBy(c => c.Name).Take(200)); // Limit for performance
            
            UpdateCardBrowserDisplay();
        }

        private void UpdateCardBrowserDisplay()
        {
            CardBrowserPanel.Items.Clear();
            
            foreach (var card in _filteredCards)
            {
                var cardElement = CreateBrowserCardElement(card);
                CardBrowserPanel.Items.Add(cardElement);
            }
            
            ResultsCountText.Text = _filteredCards.Count >= 200 
                ? $"200+ cards found (showing first 200)" 
                : $"{_filteredCards.Count} cards found";
        }

        private Border CreateBrowserCardElement(CardData card)
        {
            var border = new Border
            {
                Width = 100,
                Height = 140,
                Margin = new Thickness(4),
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30363D")),
                Cursor = Cursors.Hand,
                Tag = card
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Card image area
            var artBorder = new Border
            {
                CornerRadius = new CornerRadius(4, 4, 0, 0),
                Margin = new Thickness(2, 2, 2, 0),
                ClipToBounds = true
            };

            var imagePath = _database.GetImagePath(card);
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelWidth = 200;
                    bitmap.EndInit();

                    artBorder.Background = new ImageBrush(bitmap)
                    {
                        Stretch = Stretch.UniformToFill
                    };
                }
                catch
                {
                    artBorder.Background = GetElementGradient(card.Element);
                }
            }
            else
            {
                artBorder.Background = GetElementGradient(card.Element);
            }

            Grid.SetRow(artBorder, 0);
            grid.Children.Add(artBorder);

            // Card info area
            var infoBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(230, 26, 26, 46)),
                CornerRadius = new CornerRadius(0, 0, 4, 4),
                Padding = new Thickness(4, 2, 4, 2)
            };

            var infoStack = new StackPanel();

            var nameBlock = new TextBlock
            {
                Text = card.Name.Length > 12 ? card.Name.Substring(0, 10) + ".." : card.Name,
                FontSize = 9,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            infoStack.Children.Add(nameBlock);

            // Type indicator
            var typeBlock = new TextBlock
            {
                Text = GetCardTypeDisplay(card),
                FontSize = 7,
                Foreground = GetCardTypeColor(card)
            };
            infoStack.Children.Add(typeBlock);

            infoBorder.Child = infoStack;
            Grid.SetRow(infoBorder, 1);
            grid.Children.Add(infoBorder);

            // Add count indicator if in deck
            var countInDeck = GetCardCountInDeck(card);
            if (countInDeck > 0)
            {
                var countBadge = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4AF37")),
                    CornerRadius = new CornerRadius(10),
                    Width = 20,
                    Height = 20,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 4, 4, 0)
                };
                countBadge.Child = new TextBlock
                {
                    Text = countInDeck.ToString(),
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(countBadge, 0);
                grid.Children.Add(countBadge);
            }

            border.Child = grid;

            // Events
            border.MouseEnter += (s, e) =>
            {
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4AF37"));
                ShowHoverPreview(card);
            };
            border.MouseLeave += (s, e) =>
            {
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30363D"));
                HideHoverPreview();
            };
            border.MouseLeftButtonDown += (s, e) =>
            {
                AddCardToDeck(card);
            };
            border.MouseRightButtonDown += (s, e) =>
            {
                RemoveCardFromDeck(card);
            };

            return border;
        }

        private int GetCardCountInDeck(CardData card)
        {
            if (card.IsRegalia)
            {
                return _materialDeck.Any(c => c.Slug == card.Slug) ? 1 : 0;
            }
            else
            {
                return _mainDeckCounts.TryGetValue(card.Slug, out var count) ? count : 0;
            }
        }

        private string GetCardTypeDisplay(CardData card)
        {
            var types = new List<string>();
            if (card.IsRegalia) types.Add("Regalia");
            if (card.IsChampion) types.Add("Champion");
            if (card.IsDivineRelic) types.Add("Divine");
            if (card.IsChampionSpirit) types.Add("Spirit");
            if (card.IsToken) types.Add("Token");
            
            if (types.Count > 0) return string.Join(" ", types);
            return card.CardType;
        }

        private SolidColorBrush GetCardTypeColor(CardData card)
        {
            if (card.IsChampionSpirit) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#58A6FF"));
            if (card.IsChampion) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#58A6FF"));
            if (card.IsDivineRelic) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBBF24"));
            if (card.IsRegalia) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A855F7"));
            if (card.IsToken) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F85149"));
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B949E"));
        }

        #endregion

        #region Deck Management

        private void AddCardToDeck(CardData card)
        {
            if (card.IsMaterialDeckCard)
            {
                // Material deck - unique cards only, max 12
                if (_materialDeck.Count >= 12)
                {
                    ShowValidationMessage("Material deck is full (12 cards max)", false);
                    return;
                }
                if (_materialDeck.Any(c => c.Slug == card.Slug))
                {
                    ShowValidationMessage("Material deck must have unique cards only", false);
                    return;
                }
                if (card.IsDivineRelic && _materialDeck.Any(c => c.IsDivineRelic))
                {
                    ShowValidationMessage("Only 1 Divine Relic allowed in material deck", false);
                    return;
                }
                
                _materialDeck.Add(card);
            }
            else if (card.IsMainDeckCard)
            {
                // Main deck - up to 4 copies
                var currentCount = _mainDeckCounts.TryGetValue(card.Slug, out var c) ? c : 0;
                if (currentCount >= 4)
                {
                    ShowValidationMessage("Maximum 4 copies of any card in main deck", false);
                    return;
                }
                
                if (currentCount == 0)
                {
                    _mainDeckCards.Add(card);
                }
                _mainDeckCounts[card.Slug] = currentCount + 1;
            }
            else if (card.IsToken)
            {
                ShowValidationMessage("Tokens cannot be added to decks", false);
                return;
            }
            else
            {
                ShowValidationMessage("This card cannot be added to a deck", false);
                return;
            }
            
            UpdateDeckDisplays();
            UpdateValidation();
            RefreshCardBrowser(); // Update count badges
        }

        private void RemoveCardFromDeck(CardData card)
        {
            if (card.IsRegalia)
            {
                var existing = _materialDeck.FirstOrDefault(c => c.Slug == card.Slug);
                if (existing != null)
                {
                    _materialDeck.Remove(existing);
                }
            }
            else
            {
                if (_mainDeckCounts.TryGetValue(card.Slug, out var count) && count > 0)
                {
                    if (count == 1)
                    {
                        _mainDeckCounts.Remove(card.Slug);
                        _mainDeckCards.RemoveAll(c => c.Slug == card.Slug);
                    }
                    else
                    {
                        _mainDeckCounts[card.Slug] = count - 1;
                    }
                }
            }
            
            UpdateDeckDisplays();
            UpdateValidation();
            RefreshCardBrowser();
        }

        private void UpdateDeckDisplays()
        {
            if (!_isInitialized || MaterialDeckPanel == null || MainDeckPanel == null)
                return;
                
            // Material Deck
            MaterialDeckPanel.Children.Clear();
            foreach (var card in _materialDeck.OrderBy(c => c.Name))
            {
                MaterialDeckPanel.Children.Add(CreateDeckListItem(card, 1, true));
            }
            MaterialDeckInfo.Text = $"{_materialDeck.Count}/12 material cards";
            
            // Main Deck
            MainDeckPanel.Children.Clear();
            var totalMainDeckCards = _mainDeckCounts.Values.Sum();
            foreach (var card in _mainDeckCards.OrderBy(c => c.Name))
            {
                var count = _mainDeckCounts[card.Slug];
                MainDeckPanel.Children.Add(CreateDeckListItem(card, count, false));
            }
            MainDeckInfo.Text = $"{totalMainDeckCards}/60 cards (min 60)";
        }

        private Border CreateDeckListItem(CardData card, int count, bool isMaterialDeck)
        {
            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#21262D")),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 6, 8, 6),
                Margin = new Thickness(0, 0, 0, 4),
                Cursor = Cursors.Hand,
                Tag = card
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Card info
            var infoStack = new StackPanel();
            var nameBlock = new TextBlock
            {
                Text = card.Name,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            infoStack.Children.Add(nameBlock);

            var subtypeBlock = new TextBlock
            {
                Text = $"{card.Element ?? "Norm"} • {GetCardTypeDisplay(card)}",
                FontSize = 9,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B949E"))
            };
            infoStack.Children.Add(subtypeBlock);

            Grid.SetColumn(infoStack, 0);
            grid.Children.Add(infoStack);

            // Count (for main deck only)
            if (!isMaterialDeck)
            {
                var countBlock = new TextBlock
                {
                    Text = $"x{count}",
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4AF37")),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(8, 0, 8, 0)
                };
                Grid.SetColumn(countBlock, 1);
                grid.Children.Add(countBlock);
            }

            // Remove button
            var removeBtn = new Button
            {
                Content = "−",
                Width = 24,
                Height = 24,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F85149")),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            removeBtn.Click += (s, e) => RemoveCardFromDeck(card);
            Grid.SetColumn(removeBtn, 2);
            grid.Children.Add(removeBtn);

            border.Child = grid;

            // Hover preview
            border.MouseEnter += (s, e) =>
            {
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30363D"));
                ShowHoverPreview(card);
            };
            border.MouseLeave += (s, e) =>
            {
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#21262D"));
                HideHoverPreview();
            };

            return border;
        }

        #endregion

        #region Validation

        private void UpdateValidation()
        {
            if (!_isInitialized || ValidationIcon == null || ValidationText == null)
                return;
                
            var errors = new List<string>();
            
            // Material deck validation
            if (_materialDeck.Count != 12)
            {
                errors.Add($"Material deck needs {12 - _materialDeck.Count} more cards");
            }

            // Ensure only regalia or champion spirits are included
            var invalidMaterialCards = _materialDeck.Where(c => !c.IsMaterialDeckCard).ToList();
            if (invalidMaterialCards.Count > 0)
            {
                errors.Add("Material deck can only include regalia and champion spirits");
            }
            
            var divineRelicCount = _materialDeck.Count(c => c.IsDivineRelic);
            var championSpiritCount = _materialDeck.Count(c => c.IsChampionSpirit);
            
            if (divineRelicCount > 1)
            {
                errors.Add("Only 1 Divine Relic allowed");
            }
            
            if (championSpiritCount < 1)
            {
                errors.Add("Need at least 1 Champion Spirit");
            }
            
            // Main deck validation
            var totalMainDeckCards = _mainDeckCounts.Values.Sum();
            if (totalMainDeckCards < 60)
            {
                errors.Add($"Main deck needs {60 - totalMainDeckCards} more cards");
            }
            
            // Check for over 4 copies (shouldn't happen but verify)
            var overLimit = _mainDeckCounts.Where(kvp => kvp.Value > 4).Select(kvp => kvp.Key).ToList();
            foreach (var slug in overLimit)
            {
                var card = _mainDeckCards.First(c => c.Slug == slug);
                errors.Add($"Too many copies of {card.Name}");
            }
            
            // Update UI
            DivineRelicCount.Text = $"{divineRelicCount}/1";
            DivineRelicCount.Foreground = divineRelicCount <= 1 
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3FB950"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F85149"));
            
            ChampionSpiritCount.Text = championSpiritCount >= 1 
                ? $"{championSpiritCount}" 
                : $"{championSpiritCount} (need 1+)";
            ChampionSpiritCount.Foreground = championSpiritCount >= 1 
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3FB950"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F85149"));
            
            if (errors.Count == 0)
            {
                ValidationIcon.Text = "✓";
                ValidationIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3FB950"));
                ValidationText.Text = "Deck is valid and ready to use!";
                ConfirmDeckButton.IsEnabled = true;
            }
            else
            {
                ValidationIcon.Text = "⚠";
                ValidationIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F85149"));
                ValidationText.Text = string.Join(" | ", errors.Take(3));
                ConfirmDeckButton.IsEnabled = false;
            }
        }

        private void ShowValidationMessage(string message, bool isSuccess)
        {
            ValidationIcon.Text = isSuccess ? "✓" : "⚠";
            ValidationIcon.Foreground = isSuccess 
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3FB950"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F85149"));
            ValidationText.Text = message;
        }

        #endregion

        #region Hover Preview

        private void ShowHoverPreview(CardData card)
        {
            HoverPreviewPanel.Visibility = Visibility.Visible;
            HoverPreviewName.Text = card.Name;
            HoverPreviewType.Text = $"{card.Element ?? "Norm"} {card.CardType}";
            HoverPreviewEffect.Text = card.Effect ?? "";

            var imagePath = _database.GetImagePath(card);
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelWidth = 120;
                    bitmap.EndInit();
                    HoverPreviewImage.Source = bitmap;
                }
                catch
                {
                    HoverPreviewImage.Source = null;
                }
            }
            else
            {
                HoverPreviewImage.Source = null;
            }
        }

        private void HideHoverPreview()
        {
            HoverPreviewPanel.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Event Handlers

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isInitialized || SearchTextBox == null)
                return;
                
            // Simple debounce
            var searchText = SearchTextBox.Text ?? "";
            if (searchText == _lastSearchText) return;
            
            _lastSearchText = searchText;
            _lastSearchTime = DateTime.Now;
            
            RefreshCardBrowser();
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            RefreshCardBrowser();
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
                RefreshCardBrowser();
        }

        private void ElementFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitialized)
                RefreshCardBrowser();
        }

        private void TypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitialized)
                RefreshCardBrowser();
        }

        private void ClearMaterialDeckButton_Click(object sender, RoutedEventArgs e)
        {
            _materialDeck.Clear();
            UpdateDeckDisplays();
            UpdateValidation();
            RefreshCardBrowser();
        }

        private void ClearMainDeckButton_Click(object sender, RoutedEventArgs e)
        {
            _mainDeckCards.Clear();
            _mainDeckCounts.Clear();
            UpdateDeckDisplays();
            UpdateValidation();
            RefreshCardBrowser();
        }

        private void LoadDeckButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Deck Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Load Deck"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var json = File.ReadAllText(dialog.FileName);
                    var deckFile = JsonSerializer.Deserialize<DeckFile>(json);
                    
                    if (deckFile != null)
                    {
                        DeckNameTextBox.Text = deckFile.Name ?? "Loaded Deck";
                        
                        // Load material deck
                        _materialDeck.Clear();
                        foreach (var slug in deckFile.MaterialDeck ?? new List<string>())
                        {
                            var card = _database.GetBySlug(slug);
                            if (card != null) _materialDeck.Add(card);
                        }
                        
                        // Load main deck
                        _mainDeckCards.Clear();
                        _mainDeckCounts.Clear();
                        foreach (var entry in deckFile.MainDeck ?? new Dictionary<string, int>())
                        {
                            var card = _database.GetBySlug(entry.Key);
                            if (card != null)
                            {
                                _mainDeckCards.Add(card);
                                _mainDeckCounts[entry.Key] = entry.Value;
                            }
                        }
                        
                        UpdateDeckDisplays();
                        UpdateValidation();
                        RefreshCardBrowser();
                        
                        MessageBox.Show("Deck loaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading deck: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportListButton_Click(object sender, RoutedEventArgs e)
        {
            var text = PromptDeckListText();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            try
            {
                ImportDeckListText(text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing deck list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveDeckButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Deck Files (*.json)|*.json",
                Title = "Save Deck",
                FileName = $"{DeckNameTextBox.Text}.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var deckFile = new DeckFile
                    {
                        Name = DeckNameTextBox.Text,
                        MaterialDeck = _materialDeck.Select(c => c.Slug).ToList(),
                        MainDeck = new Dictionary<string, int>(_mainDeckCounts)
                    };
                    
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var json = JsonSerializer.Serialize(deckFile, options);
                    File.WriteAllText(dialog.FileName, json);
                    
                    MessageBox.Show("Deck saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving deck: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ConfirmDeckButton_Click(object sender, RoutedEventArgs e)
        {
            // Build the result decks
            ResultMaterialDeck = new List<CardData>(_materialDeck);
            
            ResultMainDeck = new List<CardData>();
            foreach (var card in _mainDeckCards)
            {
                var count = _mainDeckCounts[card.Slug];
                for (int i = 0; i < count; i++)
                {
                    ResultMainDeck.Add(card);
                }
            }
            
            DeckConfirmed = true;
            DialogResult = true;
            Close();
        }

        #endregion

        #region Helpers

        private void ImportDeckListText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                MessageBox.Show("Deck list is empty.", "Import Deck List", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var cardsByName = _database.AllCards
                .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            _materialDeck.Clear();
            _mainDeckCards.Clear();
            _mainDeckCounts.Clear();

            var missingCards = new List<string>();
            var invalidMaterialCards = new List<string>();
            var invalidMainDeckCards = new List<string>();
            var currentSection = string.Empty;

            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var linePattern = new Regex(@"^\s*(\d+)\s+(.+?)\s*$");

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                {
                    continue;
                }

                if (line.StartsWith("#"))
                {
                    currentSection = line.TrimStart('#').Trim().ToLowerInvariant();
                    continue;
                }

                var match = linePattern.Match(line);
                if (!match.Success)
                {
                    continue;
                }

                var count = int.Parse(match.Groups[1].Value);
                var name = match.Groups[2].Value;

                if (!cardsByName.TryGetValue(name, out var matchedCards) || matchedCards.Count == 0)
                {
                    missingCards.Add(name);
                    continue;
                }

                var card = matchedCards[0];

                if (currentSection.Contains("material"))
                {
                    if (!card.IsMaterialDeckCard)
                    {
                        invalidMaterialCards.Add(name);
                        continue;
                    }

                    if (_materialDeck.Any(c => c.Slug == card.Slug))
                    {
                        continue;
                    }

                    _materialDeck.Add(card);
                }
                else if (currentSection.Contains("main"))
                {
                    if (!card.IsMainDeckCard)
                    {
                        invalidMainDeckCards.Add(name);
                        continue;
                    }

                    if (!_mainDeckCounts.ContainsKey(card.Slug))
                    {
                        _mainDeckCards.Add(card);
                        _mainDeckCounts[card.Slug] = 0;
                    }

                    _mainDeckCounts[card.Slug] = Math.Min(4, _mainDeckCounts[card.Slug] + count);
                }
                else
                {
                    // Ignore sideboard or unknown sections
                    continue;
                }
            }

            UpdateDeckDisplays();
            UpdateValidation();
            RefreshCardBrowser();

            var messages = new List<string> { "Deck list imported." };
            if (missingCards.Count > 0)
            {
                messages.Add($"Missing cards: {string.Join(", ", missingCards.Take(6))}{(missingCards.Count > 6 ? "..." : "")}");
            }
            if (invalidMaterialCards.Count > 0)
            {
                messages.Add($"Not valid for material deck: {string.Join(", ", invalidMaterialCards.Take(6))}{(invalidMaterialCards.Count > 6 ? "..." : "")}");
            }
            if (invalidMainDeckCards.Count > 0)
            {
                messages.Add($"Not valid for main deck: {string.Join(", ", invalidMainDeckCards.Take(6))}{(invalidMainDeckCards.Count > 6 ? "..." : "")}");
            }

            MessageBox.Show(string.Join("\n", messages), "Import Deck List", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string? PromptDeckListText()
        {
            var dialog = new Window
            {
                Title = "Import Deck List",
                Owner = this,
                Width = 700,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = (Brush)FindResource("PrimaryDarkBrush")
            };

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var header = new TextBlock
            {
                Text = "Paste your deck list below:",
                Margin = new Thickness(16, 16, 16, 8),
                Foreground = (Brush)FindResource("TextPrimaryBrush"),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold
            };
            Grid.SetRow(header, 0);
            root.Children.Add(header);

            var textBox = new TextBox
            {
                Margin = new Thickness(16, 0, 16, 12),
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = (Brush)FindResource("TertiaryDarkBrush"),
                Foreground = (Brush)FindResource("TextPrimaryBrush"),
                BorderBrush = (Brush)FindResource("BorderBrush"),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12
            };

            if (Clipboard.ContainsText())
            {
                textBox.Text = Clipboard.GetText();
            }

            Grid.SetRow(textBox, 1);
            root.Children.Add(textBox);

            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(16, 0, 16, 16)
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Style = (Style)FindResource("GameButton"),
                Margin = new Thickness(0, 0, 8, 0)
            };
            cancelButton.Click += (_, _) => dialog.DialogResult = false;

            var importButton = new Button
            {
                Content = "Import",
                Style = (Style)FindResource("ActionButton")
            };
            importButton.Click += (_, _) => dialog.DialogResult = true;

            buttonsPanel.Children.Add(cancelButton);
            buttonsPanel.Children.Add(importButton);

            Grid.SetRow(buttonsPanel, 2);
            root.Children.Add(buttonsPanel);

            dialog.Content = root;

            var result = dialog.ShowDialog();
            if (result == true)
            {
                return textBox.Text;
            }

            return null;
        }

        private LinearGradientBrush GetElementGradient(string? element)
        {
            var (color1, color2) = (element?.ToUpper()) switch
            {
                "FIRE" => ("#FF6B35", "#CC4422"),
                "WATER" => ("#4ECDC4", "#2E8B8B"),
                "WIND" => ("#95E1D3", "#5CAB9E"),
                "ARCANE" => ("#A855F7", "#7C3AED"),
                "LUXEM" => ("#FBBF24", "#D97706"),
                "UMBRA" => ("#6366F1", "#4338CA"),
                "ASTRA" => ("#EC4899", "#BE185D"),
                "CRUX" => ("#10B981", "#047857"),
                "NORM" => ("#6B7280", "#4B5563"),
                "TERA" => ("#8B4513", "#654321"),
                _ => ("#4B5563", "#374151")
            };

            return new LinearGradientBrush(
                (Color)ColorConverter.ConvertFromString(color1),
                (Color)ColorConverter.ConvertFromString(color2),
                45);
        }

        #endregion
    }

    /// <summary>
    /// Deck file format for saving/loading.
    /// </summary>
    public class DeckFile
    {
        public string? Name { get; set; }
        public List<string>? MaterialDeck { get; set; }
        public Dictionary<string, int>? MainDeck { get; set; }
    }
}
