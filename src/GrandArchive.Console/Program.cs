using GrandArchive.Cards.Types;
using GrandArchive.Core.Enums;
using GrandArchive.Game;
using GrandArchive.Players;

namespace GrandArchive.Console;

/// <summary>
/// Simple console application to demonstrate the Grand Archive TCG simulator.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine("=== Grand Archive TCG Simulator ===\n");

        // Create a simple game demonstration
        var game = new GameState();

        // Create two players
        var player1 = CreatePlayer("Player 1", "Lorraine, Wandering Warrior");
        var player2 = CreatePlayer("Player 2", "Rai, Storm Seer");

        game.AddPlayer(player1);
        game.AddPlayer(player2);

        System.Console.WriteLine($"Players: {player1.Name} vs {player2.Name}");
        System.Console.WriteLine();

        // Display player info
        DisplayPlayerInfo(player1);
        DisplayPlayerInfo(player2);

        // Initialize the game
        System.Console.WriteLine("\n--- Initializing Game ---\n");
        game.InitializeGame();

        // Display game state
        System.Console.WriteLine($"Turn {game.TurnNumber}: {game.TurnPlayer.Name}'s turn");
        System.Console.WriteLine($"Current Phase: {game.CurrentPhase}");
        System.Console.WriteLine();

        // Display champions on field
        System.Console.WriteLine("Champions on Field:");
        foreach (var player in game.Players)
        {
            if (player.Champion != null)
            {
                System.Console.WriteLine($"  {player.Name}: {player.Champion.Name} ({player.Champion.Power}/{player.Champion.Life})");
            }
        }

        System.Console.WriteLine();

        // Display hands
        System.Console.WriteLine("Starting Hands:");
        DisplayHand(player1);
        DisplayHand(player2);

        System.Console.WriteLine("\n=== Simulator Ready ===");
        System.Console.WriteLine("The Grand Archive TCG engine is set up and ready for gameplay implementation!");
    }

    static Player CreatePlayer(string name, string championName)
    {
        var player = new Player { Name = name };

        // Create a champion
        var champion = new ChampionCard
        {
            Name = championName,
            Element = Element.Norm,
            BasePower = 0,
            BaseLife = 20,
            Classes = new List<ChampionClass> { ChampionClass.Warrior }.AsReadOnly(),
            LineageName = championName.Split(',')[0]
        };

        // Add champion to material deck
        player.MaterialDeckZone.Add(champion);

        // Create some sample cards for the main deck
        for (int i = 0; i < 60; i++)
        {
            var card = CreateSampleCard(i);
            player.MainDeckZone.Add(card);
        }

        return player;
    }

    static Cards.Base.Card CreateSampleCard(int index)
    {
        // Create a variety of sample cards
        return (index % 5) switch
        {
            0 => new AllyCard
            {
                Name = $"Training Dummy #{index / 5 + 1}",
                Element = Element.Norm,
                Cost = 1,
                BasePower = 1,
                BaseLife = 1,
                Subtypes = new List<Subtype> { Subtype.Warrior }.AsReadOnly()
            },
            1 => new ActionCard
            {
                Name = $"Quick Strike #{index / 5 + 1}",
                Element = Element.Fire,
                Cost = 1,
                Speed = SpeedType.Fast
            },
            2 => new AttackCard
            {
                Name = $"Sword Slash #{index / 5 + 1}",
                Element = Element.Norm,
                Cost = 0,
                BasePower = 2
            },
            3 => new ItemCard
            {
                Name = $"Health Potion #{index / 5 + 1}",
                Element = Element.Water,
                Cost = 1,
                Subtypes = new List<Subtype> { Subtype.Potion }.AsReadOnly()
            },
            _ => new AllyCard
            {
                Name = $"Apprentice Mage #{index / 5 + 1}",
                Element = Element.Arcane,
                Cost = 2,
                BasePower = 2,
                BaseLife = 2,
                Subtypes = new List<Subtype> { Subtype.Mage }.AsReadOnly()
            }
        };
    }

    static void DisplayPlayerInfo(Player player)
    {
        System.Console.WriteLine($"{player.Name}:");
        System.Console.WriteLine($"  Main Deck: {player.MainDeckZone.Count} cards");
        System.Console.WriteLine($"  Material Deck: {player.MaterialDeckZone.Count} cards");
    }

    static void DisplayHand(Player player)
    {
        System.Console.WriteLine($"  {player.Name}'s Hand ({player.HandZone.Count} cards):");
        foreach (var card in player.HandZone.Cards)
        {
            System.Console.WriteLine($"    - {card.Name} ({card.CardType}, Cost: {card.Cost})");
        }
    }
}
