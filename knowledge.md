# GASim - Grand Archive TCG Simulator Knowledge Document

## Overview

GASim is a C# WPF application that simulates the Grand Archive Trading Card Game. It loads card data from the official Grand Archive API and provides a visual interface for playing the game.

## Project Structure

```
GASim/
├── src/
│   ├── GrandArchive/              # Core game engine library (.NET 8.0)
│   │   ├── Cards/
│   │   │   ├── Base/              # Card.cs, UnitCard.cs - base classes
│   │   │   └── Types/             # ActionCard, AllyCard, AttackCard, ChampionCard, etc.
│   │   ├── Core/Enums/            # CardType, CostType, Element, Keywords, PhaseType, ZoneType
│   │   ├── Game/                  # CombatManager, EffectManager, EventManager, GameState, TurnManager
│   │   ├── Interfaces/            # IAbility, ICard, IGameEvent, IGameState, IPlayer, IZone
│   │   ├── Players/               # Deck.cs, Player.cs
│   │   └── Zones/                 # Banishment, EffectsStack, Field, Graveyard, Hand, MainDeck, MaterialDeck, Memory, Zone
│   │
│   ├── GrandArchive.Console/      # Simple console demo app
│   │
│   └── GrandArchive.GUI/          # WPF GUI application (.NET 8.0-windows)
│       ├── Data/
│       │   ├── CardData.cs        # JSON deserialization models for API data
│       │   ├── CardDatabase.cs    # Loads and manages card data from JSON files
│       │   └── GameCard.cs        # Runtime card instance with game state (damage, rested, etc.)
│       ├── App.xaml               # Application resources, styles, color theme
│       ├── App.xaml.cs
│       ├── MainWindow.xaml        # Main game UI layout
│       └── MainWindow.xaml.cs     # Game logic, UI updates, drag-and-drop
│
├── Cards/                         # Imported card JSON data (gitignored, run ImportCards.ps1)
├── CardImages/                    # Imported card images (gitignored, run ImportCardImages.ps1)
├── ImportCards.ps1                # Downloads all card data from api.gatcg.com
├── ImportCardImages.ps1           # Downloads card images from api.gatcg.com
└── Simulator/                     # Legacy/unused implementation
```

## Key Files

### MainWindow.xaml.cs - Game State

The main window manages all game state through these lists:

```csharp
// Player 1 (bottom/you)
private List<GameCard> _player1Hand;
private List<GameCard> _player1Field;
private List<GameCard> _player1Memory;
private List<GameCard> _player1Graveyard;
private List<GameCard> _player1Banishment;
private List<GameCard> _player1Deck;
private List<GameCard> _player1MaterialDeck;
private GameCard? _player1Champion;

// Player 2 (top/opponent) - same structure with _player2 prefix
```

### MainWindow.xaml - UI Layout

The UI is structured as a 3-column layout:
- **Left Panel (140px)**: Phase indicator with all 7 phases, action buttons, stack info
- **Center**: Game board with opponent area (top) and player area (bottom)
- **Right Panel (260px)**: Card preview (shows on hover) and game log

#### Player Area Layout
```
┌─────────────┬────────────────────────────────┬───────────┐
│  Champion   │                                │  BANISH   │ (rotated 90° CCW)
│             │          FIELD                 │           │
│  MATERIAL   │                                │   DECK    │
│             │          MEMORY                │           │
│             │                                │  GRAVE    │
├─────────────┴────────────────────────────────┴───────────┤
│                         HAND                              │
└───────────────────────────────────────────────────────────┘
```

### CardData.cs - API Data Model

```csharp
public class CardData
{
    public string Slug { get; set; }      // Unique identifier (e.g., "lorraine-wandering-warrior")
    public string Name { get; set; }
    public string? Element { get; set; }   // Fire, Water, Wind, Arcane, etc.
    public List<string> Types { get; set; } // ["ALLY"], ["ACTION"], ["CHAMPION"], etc.
    public int? Cost { get; set; }
    public int? Power { get; set; }
    public int? Life { get; set; }
    public string? Effect { get; set; }    // Rules text
    public int? Level { get; set; }        // For champions
    public List<EditionData> Editions { get; set; } // Contains image paths
}
```

### GameCard.cs - Runtime Card State

```csharp
public class GameCard
{
    public CardData Data { get; }
    public string? ImagePath { get; set; }  // Full path to local image file
    public string CurrentZone { get; set; } // "PlayerHand", "PlayerField", etc.
    public int DamageTaken { get; set; }
    public bool IsRested { get; set; }
    public int OwnerId { get; set; }        // 1 or 2
    public int ControllerId { get; set; }   // 1 or 2
    
    // Computed properties
    public int EffectivePower => (Data.Power ?? 0);
    public int EffectiveLife => (Data.Life ?? 0) - DamageTaken;
    public bool IsUnit => CardType is "CHAMPION" or "ALLY";
}
```

## UI Patterns

### Card Elements

Cards are created via `CreateCardElement(GameCard card, bool faceDown, string sourceZone)`:
- Returns a `Border` with the card visual
- Face-down cards show card back (🂠)
- Face-up cards show image with name/cost/stats overlay
- Supports drag-and-drop for player cards
- Hover shows preview in right panel

### Zone Updates

Each zone has an update method called from `UpdateUI()`:
- `UpdateHandDisplay(ItemsControl, List<GameCard>, bool faceDown, string zoneName)`
- `UpdateFieldDisplay(ItemsControl, List<GameCard>, string zoneName)`
- `UpdateMemoryDisplay(ItemsControl, List<GameCard>)` - always face-down
- `UpdateZoneTopCard(Border topCard, Border empty, List<GameCard>)` - for graveyard/banishment

### Drag and Drop

Implemented for player zones only:
- Cards store their source zone in `CurrentZone`
- `MouseLeftButtonDown` starts potential drag
- `MouseMove` initiates `DragDrop.DoDragDrop()` after threshold
- Drop zones have `AllowDrop="True"` and handlers like `PlayerField_Drop`
- `HandleDrop()` moves card between zone lists and calls `UpdateUI()`

### Color Theme (App.xaml)

```
PrimaryDark:    #0D1117  (main background)
SecondaryDark:  #161B22  (panels)
TertiaryDark:   #21262D  (cards, inputs)
Border:         #30363D
TextPrimary:    #F0F6FC
TextSecondary:  #8B949E
AccentGold:     #D4AF37  (highlights, deck)
AccentBlue:     #58A6FF  (memory, cost)
AccentGreen:    #3FB950  (life)
AccentRed:      #F85149  (power, banishment)
AccentPurple:   #A855F7  (material deck, card types)
```

## Grand Archive Game Rules

### Turn Phases (in order)
1. **Wake Up** - Ready (unrest) all rested cards
2. **Materialize** - Play a card from material deck
3. **Recollection** - Return one card from memory to hand
4. **Draw** - Draw a card from deck
5. **Main Phase** - Play cards, activate abilities
6. **Combat** - Declare and resolve attacks
7. **End Phase** - Discard to hand size, end-of-turn effects

### Zones
- **Deck**: Main deck, draw from here
- **Material Deck**: 12-card deck for regalia/materializing
- **Hand**: Cards you can play
- **Field**: Cards in play (allies, items, etc.)
- **Memory**: Face-down cards used to pay costs
- **Graveyard**: Destroyed/discarded cards (face-up)
- **Banishment**: Removed from game cards (face-up, rotated 90°)

### Card Types
- **Champion**: Your hero, starts in play, has levels
- **Ally**: Units that can attack/defend
- **Attack**: Combat cards
- **Action**: One-time effects
- **Item**: Equipment/consumables
- **Weapon**: Equippable with durability
- **Regalia**: Champion equipment from material deck
- **Domain**: Location cards

## API Reference

### Grand Archive API
- Base URL: `https://api.gatcg.com`
- Card search: `GET /cards/search?page={n}` - returns paginated card list
- Card images: `GET /cards/images/{filename}` - returns image file
- Image path in JSON: `editions[].image` (e.g., "/cards/images/lorraine-wandering-warrior-p.jpg")

### ImportCards.ps1
Fetches all cards via pagination and saves each as `Cards/{slug}.json`

### ImportCardImages.ps1
Reads card JSONs, extracts image paths from editions, downloads to `CardImages/`

## Common Tasks

### Adding a new zone
1. Add `List<GameCard>` field in MainWindow.xaml.cs
2. Add XAML for zone display in MainWindow.xaml
3. Add count TextBlock with x:Name
4. Update `UpdateUI()` to set count and call display method
5. If droppable, add `AllowDrop="True"` and drop handler
6. Add to `GetZoneList()` switch for drag-drop support

### Adding a new card display style
1. Create method like `CreateXxxCardElement(GameCard card)`
2. Return a `Border` with appropriate visuals
3. Add hover handlers for preview
4. Add drag handlers if needed

### Modifying game flow
1. Phase logic is in `NextPhaseButton_Click()`
2. `_phases` array defines phase order
3. Auto-draw happens in Draw phase
4. `UpdateUI()` refreshes all displays after state changes
