using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GrandArchive.GUI.Data;

namespace GrandArchive.GUI
{
    public partial class MainWindow : Window
    {
        private readonly CardDatabase _database = new();
        private readonly List<string> _gameLog = new();
        
        // Game state
        private GameCard? _player1Champion;
        private GameCard? _player2Champion;
        private List<GameCard> _player1Hand = new();
        private List<GameCard> _player2Hand = new();
        private List<GameCard> _player1Field = new();
        private List<GameCard> _player2Field = new();
        private List<GameCard> _player1Deck = new();
        private List<GameCard> _player2Deck = new();
        private List<GameCard> _player1Memory = new();
        private List<GameCard> _player2Memory = new();
        private List<GameCard> _player1Graveyard = new();
        private List<GameCard> _player2Graveyard = new();
        private List<GameCard> _player1MaterialDeck = new();
        private List<GameCard> _player2MaterialDeck = new();
        private List<GameCard> _player1Banishment = new();
        private List<GameCard> _player2Banishment = new();
        
        private int _turnNumber = 1;
        private int _currentPlayer = 1;
        private string _currentPhase = "WakeUp";
        private bool _isLoading = true;
        
        private readonly string[] _phases = new[] { "WakeUp", "Materialize", "Recollection", "Draw", "Main", "Combat", "End" };
        
        // Drag and drop state
        private GameCard? _draggedCard;
        private string? _dragSourceZone;
        private Point _dragStartPoint;
        private bool _isDragging;
        
        // Player deck configurations (from deck builder)
        private List<CardData>? _player1MaterialDeckConfig;
        private List<CardData>? _player1MainDeckConfig;

        public MainWindow()
        {
            InitializeComponent();
            LoadDatabaseAsync();
        }

        private async void LoadDatabaseAsync()
        {
            try
            {
                // Find the Cards and CardImages folders
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var cardsFolder = FindFolder(baseDir, "Cards");
                var imagesFolder = FindFolder(baseDir, "CardImages");

                if (cardsFolder == null || imagesFolder == null)
                {
                    MessageBox.Show(
                        "Cards or CardImages folder not found.\n\n" +
                        "Please run ImportCards.ps1 and ImportCardImages.ps1 first.",
                        "Database Not Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    _isLoading = false;
                    return;
                }

                await _database.LoadCardsAsync(cardsFolder, imagesFolder);
                
                LogMessage($"Loaded {_database.Count} cards from database");
                LogMessage($"Found {_database.GetChampions().Count()} champions");
                
                _isLoading = false;
                StartNewGame();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading card database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _isLoading = false;
            }
        }

        private string? FindFolder(string startDir, string folderName)
        {
            // Search up the directory tree
            var dir = new DirectoryInfo(startDir);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, folderName);
                if (Directory.Exists(candidate))
                    return candidate;
                dir = dir.Parent;
            }
            return null;
        }

        private void StartNewGame()
        {
            if (!_database.IsLoaded) return;

            // Reset game state
            _turnNumber = 1;
            _currentPlayer = 1;
            _currentPhase = "WakeUp";
            _player1Hand.Clear();
            _player2Hand.Clear();
            _player1Field.Clear();
            _player2Field.Clear();
            _player1Memory.Clear();
            _player2Memory.Clear();
            _player1Graveyard.Clear();
            _player2Graveyard.Clear();
            _player1MaterialDeck.Clear();
            _player2MaterialDeck.Clear();
            _player1Banishment.Clear();
            _player2Banishment.Clear();

            // Check if player 1 has a custom deck from deck builder
            if (_player1MaterialDeckConfig != null && _player1MainDeckConfig != null)
            {
                // Use custom deck for player 1
                // Prefer champion spirit, otherwise any champion from material deck
                var selectedChampion = _player1MaterialDeckConfig.FirstOrDefault(c => c.IsChampionSpirit)
                    ?? _player1MaterialDeckConfig.FirstOrDefault(c => c.IsChampion);
                if (selectedChampion != null)
                {
                    _player1Champion = new GameCard(selectedChampion);
                    _player1Champion.ImagePath = _database.GetImagePath(selectedChampion);
                }
                else
                {
                    // Fallback to any champion in database
                    var fallbackChampion = _database.GetChampions().First();
                    _player1Champion = new GameCard(fallbackChampion);
                    _player1Champion.ImagePath = _database.GetImagePath(fallbackChampion);
                }

                // Build material deck (includes champion spirit)
                _player1MaterialDeck = _player1MaterialDeckConfig
                    .Select(c => {
                        var gc = new GameCard(c);
                        gc.ImagePath = _database.GetImagePath(c);
                        gc.OwnerId = 1;
                        gc.ControllerId = 1;
                        gc.CurrentZone = "MaterialDeck";
                        return gc;
                    }).ToList();

                // Build main deck
                _player1Deck = _player1MainDeckConfig.Select(c => {
                    var gc = new GameCard(c);
                    gc.ImagePath = _database.GetImagePath(c);
                    gc.OwnerId = 1;
                    gc.ControllerId = 1;
                    gc.CurrentZone = "Deck";
                    return gc;
                }).ToList();

                LogMessage("Player 1 using custom deck!");
            }
            else
            {
                // Use random deck for player 1
                var (champion1, deck1) = _database.BuildRandomDeck("lorraine-wandering-warrior");
                _player1Champion = new GameCard(champion1);
                _player1Champion.ImagePath = _database.GetImagePath(champion1);

                _player1Deck = deck1.Select(c => {
                    var gc = new GameCard(c);
                    gc.ImagePath = _database.GetImagePath(c);
                    gc.OwnerId = 1;
                    gc.ControllerId = 1;
                    return gc;
                }).ToList();
            }

            // Player 2 always uses random deck
            var (champion2, deck2) = _database.BuildRandomDeck("rai-storm-seer");
            _player2Champion = new GameCard(champion2);
            _player2Champion.ImagePath = _database.GetImagePath(champion2);

            _player2Deck = deck2.Select(c => {
                var gc = new GameCard(c);
                gc.ImagePath = _database.GetImagePath(c);
                gc.OwnerId = 2;
                gc.ControllerId = 2;
                return gc;
            }).ToList();

            // Shuffle decks
            ShuffleDeck(_player1Deck);
            ShuffleDeck(_player2Deck);

            // Draw starting hands (5 cards each)
            for (int i = 0; i < 5; i++)
            {
                DrawCard(1);
                DrawCard(2);
            }

            LogMessage("Game started!");
            LogMessage($"{_player1Champion?.Name ?? "Unknown"} vs {_player2Champion?.Name ?? "Unknown"}");

            UpdateUI();
        }

        private void ShuffleDeck(List<GameCard> deck)
        {
            var random = new Random();
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
        }

        private bool DrawCard(int player)
        {
            var deck = player == 1 ? _player1Deck : _player2Deck;
            var hand = player == 1 ? _player1Hand : _player2Hand;

            if (deck.Count == 0) return false;

            var card = deck[0];
            deck.RemoveAt(0);
            card.CurrentZone = "Hand";
            hand.Add(card);
            return true;
        }

        private void UpdateUI()
        {
            if (_isLoading) return;

            // Update turn info
            TurnNumberText.Text = _turnNumber.ToString();
            TurnPlayerText.Text = $"Player {_currentPlayer}'s Turn";
            
            // Update phase indicators
            UpdatePhaseIndicators();

            // Update player 1 (bottom)
            PlayerNameText.Text = "Player 1";
            UpdateChampionDisplay(_player1Champion, PlayerChampionName, PlayerChampionPower, PlayerChampionLife, PlayerLevelText, PlayerChampionBorder);
            PlayerDeckCount.Text = $"({_player1Deck.Count})";
            PlayerMaterialDeckCount.Text = $"({_player1MaterialDeck.Count})";
            PlayerMemoryCount.Text = $"({_player1Memory.Count})";
            PlayerGraveyardCount.Text = $"({_player1Graveyard.Count})";
            PlayerBanishmentCount.Text = $"({_player1Banishment.Count})";
            UpdateZoneTopCard(PlayerGraveyardTopCard, PlayerGraveyardEmpty, _player1Graveyard);
            UpdateZoneTopCard(PlayerBanishmentTopCard, PlayerBanishmentEmpty, _player1Banishment);
            UpdateMemoryDisplay(PlayerMemoryPanel, _player1Memory);
            PlayerHandCount.Text = $"({_player1Hand.Count})";
            UpdateHandDisplay(PlayerHandPanel, _player1Hand, false, "PlayerHand");
            UpdateFieldDisplay(PlayerFieldPanel, _player1Field, "PlayerField");

            // Update player 2 (top/opponent)
            OpponentNameText.Text = "Player 2";
            UpdateChampionDisplay(_player2Champion, OpponentChampionName, OpponentChampionPower, OpponentChampionLife, OpponentLevelText, OpponentChampionBorder);
            OpponentDeckCount.Text = $"({_player2Deck.Count})";
            OpponentMaterialDeckCount.Text = $"({_player2MaterialDeck.Count})";
            OpponentMemoryCount.Text = $"({_player2Memory.Count})";
            OpponentGraveyardCount.Text = $"({_player2Graveyard.Count})";
            OpponentBanishmentCount.Text = $"({_player2Banishment.Count})";
            UpdateZoneTopCard(OpponentGraveyardTopCard, OpponentGraveyardEmpty, _player2Graveyard);
            UpdateZoneTopCard(OpponentBanishmentTopCard, OpponentBanishmentEmpty, _player2Banishment);
            UpdateMemoryDisplay(OpponentMemoryPanel, _player2Memory);
            OpponentHandCount.Text = $"({_player2Hand.Count})";
            UpdateHandDisplay(OpponentHandPanel, _player2Hand, true, "OpponentHand");
            UpdateFieldDisplay(OpponentFieldPanel, _player2Field, "OpponentField");

            // Update stack
            StackCountText.Text = "Empty";

            // Update button states
            NextPhaseButton.IsEnabled = true;
            PassButton.IsEnabled = true;
            DrawCardButton.IsEnabled = _currentPlayer == 1 && _player1Deck.Count > 0;
        }

        private void UpdateChampionDisplay(GameCard? champion, TextBlock nameText, TextBlock powerText, 
            TextBlock lifeText, TextBlock levelText, Border championBorder)
        {
            if (champion == null) return;

            nameText.Text = champion.Name.Length > 15 
                ? champion.Name.Substring(0, 12) + "..." 
                : champion.Name;
            powerText.Text = champion.EffectivePower.ToString();
            lifeText.Text = champion.EffectiveLife.ToString();
            levelText.Text = champion.Level.ToString();

            // Update champion image
            UpdateCardBorderImage(championBorder, champion);
        }

        private void UpdateCardBorderImage(Border border, GameCard card)
        {
            if (border.Child is Grid grid && grid.Children.Count > 0)
            {
                // Find the image area (first child)
                if (grid.Children[0] is Border artBorder)
                {
                    if (!string.IsNullOrEmpty(card.ImagePath) && File.Exists(card.ImagePath))
                    {
                        try
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(card.ImagePath);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();

                            artBorder.Background = new ImageBrush(bitmap)
                            {
                                Stretch = Stretch.UniformToFill
                            };
                        }
                        catch
                        {
                            // Keep default background on error
                        }
                    }
                }
            }
        }

        private void UpdateHandDisplay(ItemsControl handPanel, List<GameCard> hand, bool faceDown, string zoneName)
        {
            handPanel.Items.Clear();
            foreach (var card in hand)
            {
                var cardElement = CreateCardElement(card, faceDown, zoneName);
                handPanel.Items.Add(cardElement);
            }
        }

        private void UpdateMemoryDisplay(ItemsControl memoryPanel, List<GameCard> memory)
        {
            memoryPanel.Items.Clear();
            foreach (var card in memory)
            {
                // Memory cards are always face-down (used to pay costs)
                var cardElement = CreateMemoryCardElement(card);
                memoryPanel.Items.Add(cardElement);
            }
        }

        private Border CreateMemoryCardElement(GameCard card)
        {
            var border = new Border
            {
                Width = 60,
                Height = 84,
                Margin = new Thickness(2),
                CornerRadius = new CornerRadius(4),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#58A6FF")),
                Cursor = Cursors.Hand,
                Tag = card
            };

            // Face-down card with blue tint (memory color)
            border.Background = new LinearGradientBrush(
                (Color)ColorConverter.ConvertFromString("#1a2a4e"),
                (Color)ColorConverter.ConvertFromString("#162040"),
                45);

            var backText = new TextBlock
            {
                Text = "🂠",
                FontSize = 22,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#58A6FF"))
            };
            border.Child = backText;

            // Hover to preview the actual card (player can see their own memory)
            border.MouseEnter += (s, e) =>
            {
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4AF37"));
                if (border.Tag is GameCard hoverCard)
                {
                    ShowCardHoverPreview(hoverCard);
                }
            };
            border.MouseLeave += (s, e) =>
            {
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#58A6FF"));
                HideCardHoverPreview();
            };

            return border;
        }

        private void UpdateFieldDisplay(ItemsControl fieldPanel, List<GameCard> field, string zoneName)
        {
            fieldPanel.Items.Clear();
            foreach (var card in field)
            {
                var cardElement = CreateCardElement(card, false, zoneName);
                fieldPanel.Items.Add(cardElement);
            }
        }

        private void UpdateZoneTopCard(Border topCardBorder, Border emptyBorder, List<GameCard> zone)
        {
            if (zone.Count > 0)
            {
                var topCard = zone[^1]; // Last card (most recent)
                emptyBorder.Visibility = Visibility.Collapsed;
                topCardBorder.Visibility = Visibility.Visible;

                // Load the card image
                if (!string.IsNullOrEmpty(topCard.ImagePath) && File.Exists(topCard.ImagePath))
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(topCard.ImagePath);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.DecodePixelWidth = 140;
                        bitmap.EndInit();

                        topCardBorder.Background = new ImageBrush(bitmap)
                        {
                            Stretch = Stretch.UniformToFill
                        };
                    }
                    catch
                    {
                        topCardBorder.Background = GetElementGradient(topCard.Element);
                    }
                }
                else
                {
                    topCardBorder.Background = GetElementGradient(topCard.Element);
                }

                // Add hover preview for the top card
                topCardBorder.Tag = topCard;
                topCardBorder.Cursor = Cursors.Hand;
                topCardBorder.MouseEnter -= ZoneTopCard_MouseEnter;
                topCardBorder.MouseLeave -= ZoneTopCard_MouseLeave;
                topCardBorder.MouseEnter += ZoneTopCard_MouseEnter;
                topCardBorder.MouseLeave += ZoneTopCard_MouseLeave;
            }
            else
            {
                emptyBorder.Visibility = Visibility.Visible;
                topCardBorder.Visibility = Visibility.Collapsed;
                topCardBorder.Background = null;
            }
        }

        private void ZoneTopCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border && border.Tag is GameCard card)
            {
                ShowCardHoverPreview(card);
            }
        }

        private void ZoneTopCard_MouseLeave(object sender, MouseEventArgs e)
        {
            HideCardHoverPreview();
        }

        private Border CreateCardElement(GameCard card, bool faceDown, string sourceZone = "")
        {
            var border = new Border
            {
                Width = 80,
                Height = 112,
                Margin = new Thickness(4),
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30363D")),
                Cursor = Cursors.Hand,
                Tag = card,
                AllowDrop = false
            };
            
            // Store source zone in card for drag operations
            card.CurrentZone = sourceZone;

            if (faceDown)
            {
                // Face-down card
                border.Background = new LinearGradientBrush(
                    (Color)ColorConverter.ConvertFromString("#1a1a2e"),
                    (Color)ColorConverter.ConvertFromString("#16213e"),
                    45);

                var backText = new TextBlock
                {
                    Text = "🂠",
                    FontSize = 28,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4AF37"))
                };
                border.Child = backText;
            }
            else
            {
                // Face-up card with image
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

                // Try to load the card image
                if (!string.IsNullOrEmpty(card.ImagePath) && File.Exists(card.ImagePath))
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(card.ImagePath);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.DecodePixelWidth = 160; // Optimize memory
                        bitmap.EndInit();

                        artBorder.Background = new ImageBrush(bitmap)
                        {
                            Stretch = Stretch.UniformToFill
                        };
                    }
                    catch
                    {
                        artBorder.Background = GetElementGradient(card.Element);
                        artBorder.Child = new TextBlock
                        {
                            Text = GetCardIcon(card),
                            FontSize = 24,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Foreground = Brushes.White
                        };
                    }
                }
                else
                {
                    // Fallback to colored background with icon
                    artBorder.Background = GetElementGradient(card.Element);
                    artBorder.Child = new TextBlock
                    {
                        Text = GetCardIcon(card),
                        FontSize = 24,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = Brushes.White
                    };
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
                    Text = card.Name.Length > 10 ? card.Name.Substring(0, 8) + ".." : card.Name,
                    FontSize = 8,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.White,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                infoStack.Children.Add(nameBlock);

                // Stats row
                var statsPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 1, 0, 0) };

                // Cost
                var costBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#58A6FF")),
                    CornerRadius = new CornerRadius(2),
                    Padding = new Thickness(3, 0, 3, 0),
                    Margin = new Thickness(0, 0, 2, 0)
                };
                costBorder.Child = new TextBlock
                {
                    Text = card.Cost.ToString(),
                    FontSize = 8,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White
                };
                statsPanel.Children.Add(costBorder);

                // Power/Life for units
                if (card.IsUnit)
                {
                    var powerBorder = new Border
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F85149")),
                        CornerRadius = new CornerRadius(2),
                        Padding = new Thickness(3, 0, 3, 0),
                        Margin = new Thickness(0, 0, 2, 0)
                    };
                    powerBorder.Child = new TextBlock
                    {
                        Text = card.EffectivePower.ToString(),
                        FontSize = 8,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White
                    };
                    statsPanel.Children.Add(powerBorder);

                    var lifeBorder = new Border
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3FB950")),
                        CornerRadius = new CornerRadius(2),
                        Padding = new Thickness(3, 0, 3, 0)
                    };
                    lifeBorder.Child = new TextBlock
                    {
                        Text = card.EffectiveLife.ToString(),
                        FontSize = 8,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White
                    };
                    statsPanel.Children.Add(lifeBorder);
                }

                infoStack.Children.Add(statsPanel);
                infoBorder.Child = infoStack;
                Grid.SetRow(infoBorder, 1);
                grid.Children.Add(infoBorder);

                border.Child = grid;
            }

            border.MouseEnter += (s, e) =>
            {
                if (!_isDragging)
                {
                    border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4AF37"));
                    if (!faceDown && border.Tag is GameCard hoverCard)
                    {
                        ShowCardHoverPreview(hoverCard);
                    }
                }
            };
            border.MouseLeave += (s, e) =>
            {
                if (!_isDragging)
                {
                    border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30363D"));
                    HideCardHoverPreview();
                }
            };
            
            // Drag support - only for player's cards and non-face-down opponent cards
            if (!string.IsNullOrEmpty(sourceZone) && sourceZone.StartsWith("Player"))
            {
                border.MouseLeftButtonDown += (s, e) =>
                {
                    _dragStartPoint = e.GetPosition(this);
                    _draggedCard = card;
                    _dragSourceZone = sourceZone;
                };
                
                border.MouseMove += (s, e) =>
                {
                    if (e.LeftButton == MouseButtonState.Pressed && _draggedCard == card && !_isDragging)
                    {
                        var currentPos = e.GetPosition(this);
                        var diff = _dragStartPoint - currentPos;
                        
                        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                        {
                            _isDragging = true;
                            var data = new DataObject("GameCard", card);
                            data.SetData("SourceZone", _dragSourceZone);
                            DragDrop.DoDragDrop(border, data, DragDropEffects.Move);
                            _isDragging = false;
                            _draggedCard = null;
                            _dragSourceZone = null;
                        }
                    }
                };
                
                border.MouseLeftButtonUp += (s, e) =>
                {
                    _draggedCard = null;
                    _dragSourceZone = null;
                };
            }

            return border;
        }

        private void UpdatePhaseIndicators()
        {
            var phaseIndicators = new Dictionary<string, (Border border, TextBlock indicator)>
            {
                { "WakeUp", (PhaseWakeUp, PhaseWakeUpIndicator) },
                { "Materialize", (PhaseMaterialize, PhaseMaterializeIndicator) },
                { "Recollection", (PhaseRecollection, PhaseRecollectionIndicator) },
                { "Draw", (PhaseDraw, PhaseDrawIndicator) },
                { "Main", (PhaseMain, PhaseMainIndicator) },
                { "Combat", (PhaseCombat, PhaseCombatIndicator) },
                { "End", (PhaseEnd, PhaseEndIndicator) }
            };

            var accentGold = (SolidColorBrush)FindResource("AccentGoldBrush");
            var textSecondary = (SolidColorBrush)FindResource("TextSecondaryBrush");
            var tertiaryDark = (SolidColorBrush)FindResource("TertiaryDarkBrush");

            foreach (var kvp in phaseIndicators)
            {
                var isActive = kvp.Key == _currentPhase;
                kvp.Value.indicator.Text = isActive ? "●" : "○";
                kvp.Value.indicator.Foreground = isActive ? accentGold : textSecondary;
                
                // Highlight background for active phase
                if (isActive)
                {
                    kvp.Value.border.Background = tertiaryDark;
                }
            }
        }

        private void Phase_Click(object sender, MouseButtonEventArgs e)
        {
            // Clicking a phase doesn't change to it (phases advance in order)
            // But we could show info about the phase
            if (sender is Border border && border.Tag is string phaseName)
            {
                var phaseDescriptions = new Dictionary<string, string>
                {
                    { "WakeUp", "Ready all rested cards you control." },
                    { "Materialize", "You may play a card from your material deck." },
                    { "Recollection", "You may return one card from your memory to your hand." },
                    { "Draw", "Draw a card from your deck." },
                    { "Main", "Play cards, activate abilities, and prepare for combat." },
                    { "Combat", "Declare attackers and resolve combat." },
                    { "End", "Discard down to hand size, end of turn effects trigger." }
                };

                if (phaseDescriptions.TryGetValue(phaseName, out var description))
                {
                    LogMessage($"{phaseName} Phase: {description}");
                }
            }
        }

        private void ShowCardHoverPreview(GameCard card)
        {
            // Update card info in the right panel
            HoverCardName.Text = card.Name;
            HoverCardType.Text = card.CardType;
            HoverCardElement.Text = card.Element ?? "Norm";
            HoverCardCost.Text = card.Cost.ToString();
            HoverCardEffect.Text = card.Effect ?? "";

            if (card.IsUnit)
            {
                HoverStatsPanel.Visibility = Visibility.Visible;
                HoverCardPower.Text = card.EffectivePower.ToString();
                HoverCardLife.Text = card.EffectiveLife.ToString();
            }
            else
            {
                HoverStatsPanel.Visibility = Visibility.Collapsed;
            }

            // Load card image
            if (!string.IsNullOrEmpty(card.ImagePath) && File.Exists(card.ImagePath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(card.ImagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    HoverCardImage.Source = bitmap;
                }
                catch
                {
                    HoverCardImage.Source = null;
                }
            }
            else
            {
                HoverCardImage.Source = null;
            }

            // Show the preview panel elements
            CardPreviewPlaceholder.Visibility = Visibility.Collapsed;
            HoverCardImage.Visibility = Visibility.Visible;
            CardPreviewInfo.Visibility = Visibility.Visible;
        }

        private void HideCardHoverPreview()
        {
            CardPreviewPlaceholder.Visibility = Visibility.Visible;
            HoverCardImage.Visibility = Visibility.Collapsed;
            HoverCardImage.Source = null;
            CardPreviewInfo.Visibility = Visibility.Collapsed;
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
                _ => ("#4B5563", "#374151")
            };

            return new LinearGradientBrush(
                (Color)ColorConverter.ConvertFromString(color1),
                (Color)ColorConverter.ConvertFromString(color2),
                45);
        }

        private string GetCardIcon(GameCard card)
        {
            return card.CardType.ToUpper() switch
            {
                "CHAMPION" => "👑",
                "ALLY" => "🛡",
                "ACTION" => "⚡",
                "ATTACK" => "⚔",
                "ITEM" => "🧪",
                "REGALIA" => "💎",
                "WEAPON" => "🗡",
                "DOMAIN" => "🏰",
                _ => "📜"
            };
        }

        private void Card_Click(object sender, MouseButtonEventArgs e)
        {
            // Handle champion card clicks - show in hover preview
            if (sender is Border border)
            {
                var tag = border.Tag?.ToString();
                if (tag == "PlayerChampion" && _player1Champion != null)
                {
                    ShowCardHoverPreview(_player1Champion);
                }
                else if (tag == "OpponentChampion" && _player2Champion != null)
                {
                    ShowCardHoverPreview(_player2Champion);
                }
            }
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            _gameLog.Clear();
            GameLogText.Text = "";
            StartNewGame();
        }

        private void DeckBuilderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_database.IsLoaded)
            {
                MessageBox.Show("Please wait for the card database to load.", "Database Loading", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var deckBuilder = new DeckBuilderWindow(_database);
            deckBuilder.Owner = this;
            
            if (deckBuilder.ShowDialog() == true && deckBuilder.DeckConfirmed)
            {
                _player1MaterialDeckConfig = deckBuilder.ResultMaterialDeck;
                _player1MainDeckConfig = deckBuilder.ResultMainDeck;
                
                LogMessage($"Deck loaded: {_player1MaterialDeckConfig?.Count ?? 0} material cards, {_player1MainDeckConfig?.Count ?? 0} main deck cards");
                
                // Start a new game with the custom deck
                _gameLog.Clear();
                GameLogText.Text = "";
                StartNewGame();
            }
        }

        private void NextPhaseButton_Click(object sender, RoutedEventArgs e)
        {
            var currentIndex = Array.IndexOf(_phases, _currentPhase);
            
            if (currentIndex == _phases.Length - 1)
            {
                // End of turn, switch players
                _currentPlayer = _currentPlayer == 1 ? 2 : 1;
                if (_currentPlayer == 1) _turnNumber++;
                _currentPhase = _phases[0];
                LogMessage($"Turn {_turnNumber} - Player {_currentPlayer}'s turn");
            }
            else
            {
                _currentPhase = _phases[currentIndex + 1];
                
                // Auto-draw during Draw phase
                if (_currentPhase == "Draw")
                {
                    var deck = _currentPlayer == 1 ? _player1Deck : _player2Deck;
                    if (deck.Count > 0)
                    {
                        DrawCard(_currentPlayer);
                        var hand = _currentPlayer == 1 ? _player1Hand : _player2Hand;
                        var drawnCard = hand.LastOrDefault();
                        LogMessage($"Player {_currentPlayer} drew: {drawnCard?.Name ?? "a card"}");
                    }
                    else
                    {
                        LogMessage($"Player {_currentPlayer}'s deck is empty!");
                    }
                }
            }

            LogMessage($"Phase: {GetPhaseDisplayName(_currentPhase)}");
            UpdateUI();
        }
        
        private string GetPhaseDisplayName(string phase)
        {
            return phase switch
            {
                "WakeUp" => "Wake Up",
                "Materialize" => "Materialize",
                "Recollection" => "Recollection",
                "Draw" => "Draw",
                "Main" => "Main Phase",
                "Combat" => "Combat",
                "End" => "End Phase",
                _ => phase
            };
        }

        private void PassButton_Click(object sender, RoutedEventArgs e)
        {
            LogMessage($"Player {_currentPlayer} passed.");
            UpdateUI();
        }

        private void DrawCardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPlayer == 1 && DrawCard(1))
            {
                var drawnCard = _player1Hand.LastOrDefault();
                LogMessage($"Drew: {drawnCard?.Name ?? "a card"}");
                UpdateUI();
            }
            else
            {
                LogMessage("Cannot draw - deck is empty!");
            }
        }

        private void Deck_Click(object sender, MouseButtonEventArgs e)
        {
            DrawCardButton_Click(sender, new RoutedEventArgs());
        }

        #region Drag and Drop
        
        private void Zone_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("GameCard"))
            {
                e.Effects = DragDropEffects.Move;
                if (sender is Border border)
                {
                    border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4AF37"));
                    border.BorderThickness = new Thickness(2);
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }
        
        private void Zone_DragLeave(object sender, DragEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = (SolidColorBrush)FindResource("BorderBrush");
                border.BorderThickness = new Thickness(1);
            }
        }
        
        private void PlayerHand_Drop(object sender, DragEventArgs e)
        {
            HandleDrop(e, _player1Hand, "PlayerHand");
            ResetZoneBorder(sender);
        }
        
        private void PlayerField_Drop(object sender, DragEventArgs e)
        {
            HandleDrop(e, _player1Field, "PlayerField");
            ResetZoneBorder(sender);
        }
        
        private void PlayerMemory_Drop(object sender, DragEventArgs e)
        {
            HandleDrop(e, _player1Memory, "PlayerMemory");
            ResetZoneBorder(sender);
        }
        
        private void PlayerGraveyard_Drop(object sender, DragEventArgs e)
        {
            HandleDrop(e, _player1Graveyard, "PlayerGraveyard");
            ResetZoneBorder(sender);
        }
        
        private void PlayerBanishment_Drop(object sender, DragEventArgs e)
        {
            HandleDrop(e, _player1Banishment, "PlayerBanishment");
            ResetZoneBorder(sender);
        }
        
        private void ResetZoneBorder(object sender)
        {
            if (sender is Border border)
            {
                border.BorderBrush = (SolidColorBrush)FindResource("BorderBrush");
                border.BorderThickness = new Thickness(1);
            }
        }
        
        private void HandleDrop(DragEventArgs e, List<GameCard> targetZone, string targetZoneName)
        {
            if (!e.Data.GetDataPresent("GameCard")) return;
            
            var card = e.Data.GetData("GameCard") as GameCard;
            var sourceZone = e.Data.GetData("SourceZone") as string;
            
            if (card == null || sourceZone == null) return;
            if (sourceZone == targetZoneName) return; // Can't drop on same zone
            
            // Remove from source zone
            var sourceList = GetZoneList(sourceZone);
            if (sourceList == null) return;
            
            if (!sourceList.Remove(card)) return;
            
            // Add to target zone
            targetZone.Add(card);
            card.CurrentZone = targetZoneName;
            
            // Log the move
            LogMessage($"Moved {card.Name} from {GetZoneDisplayName(sourceZone)} to {GetZoneDisplayName(targetZoneName)}");
            
            UpdateUI();
            e.Handled = true;
        }
        
        private List<GameCard>? GetZoneList(string zoneName)
        {
            return zoneName switch
            {
                "PlayerHand" => _player1Hand,
                "PlayerField" => _player1Field,
                "PlayerMemory" => _player1Memory,
                "PlayerGraveyard" => _player1Graveyard,
                "PlayerBanishment" => _player1Banishment,
                "OpponentHand" => _player2Hand,
                "OpponentField" => _player2Field,
                "OpponentMemory" => _player2Memory,
                "OpponentGraveyard" => _player2Graveyard,
                "OpponentBanishment" => _player2Banishment,
                _ => null
            };
        }
        
        private string GetZoneDisplayName(string zoneName)
        {
            return zoneName switch
            {
                "PlayerHand" => "Hand",
                "PlayerField" => "Field",
                "PlayerMemory" => "Memory",
                "PlayerGraveyard" => "Graveyard",
                "PlayerBanishment" => "Banishment",
                "OpponentHand" => "Opponent's Hand",
                "OpponentField" => "Opponent's Field",
                "OpponentMemory" => "Opponent's Memory",
                "OpponentGraveyard" => "Opponent's Graveyard",
                "OpponentBanishment" => "Opponent's Banishment",
                _ => zoneName
            };
        }
        
        #endregion

        private void LogMessage(string message)
        {
            _gameLog.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            GameLogText.Text = string.Join("\n", _gameLog.TakeLast(50));
            GameLogScroller.ScrollToEnd();
        }
    }
}
