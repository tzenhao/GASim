# Grand Archive TCG Simulator (GASim)

A C# implementation of the Grand Archive Trading Card Game engine, based on the [Grand Archive Comprehensive Rules v1.1.1](https://rules.gatcg.com/).

## Project Structure

```
GASim/
├── src/
│   ├── GrandArchive/           # Core game engine library
│   │   ├── Cards/              # Card implementations
│   │   │   ├── Base/           # Base card classes
│   │   │   └── Types/          # Card type implementations
│   │   ├── Core/
│   │   │   └── Enums/          # Game enumerations
│   │   ├── Game/               # Game state and managers
│   │   ├── Interfaces/         # Core interfaces
│   │   ├── Players/            # Player implementation
│   │   └── Zones/              # Game zone implementations
│   ├── GrandArchive.Console/   # Demo console application
│   └── GrandArchive.GUI/       # WPF graphical interface
│       └── Data/               # Card database and game card classes
├── Cards/                      # Imported card JSON data (run ImportCards.ps1)
├── CardImages/                 # Imported card images (run ImportCardImages.ps1)
├── ImportCards.ps1             # Script to download card data from API
├── ImportCardImages.ps1        # Script to download card images from API
└── Simulator/                  # Legacy/simplified implementation
    ├── Classes/
    └── Shared/
```

## Features

### Card Types
- **Champion** - Player representatives with classes and mastery abilities
- **Ally** - Support units that can attack and defend
- **Action** - One-time effect cards
- **Attack** - Combat cards used to deal damage
- **Item** - Equipment and consumable cards
- **Weapon** - Equippable cards with durability
- **Domain** - Location cards (including Siegeable domains)
- **Phantasia** - Special manifestation cards
- **Regalia** - Champion equipment from material deck

### Game Zones
- Main Deck, Material Deck, Hand, Memory
- Field (shared), Graveyard, Banishment
- Effects Stack, Intent (for combat)

### Turn Structure
1. Wake Up Phase - Units wake up
2. Materialize Phase - Materialize from material deck
3. Recollection Phase - Return cards from memory
4. Draw Phase - Draw a card
5. Main Phase - Play cards and declare attacks
6. Combat Phase - Resolve attacks
7. End Phase - End of turn effects

### Combat System
- Attack declaration with targets
- Interception by units with Intercept keyword
- Retaliation by defending units
- Damage calculation with buff counters
- Support for keywords: Stealth, Cleave, Ambush, etc.

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- PowerShell (for importing card data)

### Setup

1. **Import card data from the Grand Archive API:**

```powershell
.\ImportCards.ps1
.\ImportCardImages.ps1
```

This will download all card data and images from the [Grand Archive API](https://api.gatcg.com/). The import scripts may take a few minutes depending on your connection.

2. **Run the GUI:**

```bash
cd src/GrandArchive.GUI
dotnet run
```

## Elements

The game supports all Grand Archive elements:
- Norm (Normal)
- Fire, Water, Wind
- Arcane, Astra, Crux
- Exia, Luxem, Neos
- Tera, Umbra

## Keywords

Implemented keywords include:
- Combat: Ambush, Cleave, Intercept, Stealth, True Sight, Floating, Ranged
- Triggered: On Enter, On Death, On Attack
- Static: Fast, Inherited, Flux, Efficiency, Glimpse, Aethercalling
- Restrictions: Pride, Unique, Divine Relic
- Special: Command, Brew, Mastery, Class Bonus, Champion Bonus

## License

This project is for educational purposes and is not affiliated with Weebs of the Shore or the official Grand Archive TCG.

## References

- [Grand Archive Comprehensive Rules](https://rules.gatcg.com/)
- [Grand Archive Official Website](https://gatcg.com/)
