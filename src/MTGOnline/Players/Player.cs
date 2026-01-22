using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;
using MTGOnline.Mana;
using MTGOnline.Zones;

namespace MTGOnline.Players
{
    /// <summary>
    /// Represents a player in Magic: The Gathering.
    /// </summary>
    public class Player : IPlayer
    {
        public Guid Id { get; }
        public string Name { get; }

        // Life and Resources
        public int Life { get; private set; }
        public int StartingLife { get; }
        public ManaPool ManaPool { get; }
        public int PoisonCounters { get; private set; }
        public int EnergyCounters { get; private set; }
        public int ExperienceCounters { get; private set; }

        // Zones
        public IZone Library { get; }
        public IZone Hand { get; }
        public IZone Graveyard { get; }
        public IZone Exile { get; }
        public IZone CommandZone { get; }
        public IZone Sideboard { get; }

        // Turn-based state
        public bool HasPlayedLandThisTurn { get; private set; }
        public int LandsPlayedThisTurn { get; private set; }
        public int MaxLandsPerTurn { get; set; } = 1;
        public bool HasPriority { get; set; }

        // Game state
        public GameResult GameResult { get; private set; } = GameResult.InProgress;
        public bool HasLost => GameResult == GameResult.Loss;
        public bool HasWon => GameResult == GameResult.Win;
        public bool HasConceded { get; private set; }

        // Damage tracking for commander damage
        private readonly Dictionary<Guid, int> _commanderDamageReceived = new();

        public Player(string name, int startingLife = 20)
        {
            Id = Guid.NewGuid();
            Name = name;
            StartingLife = startingLife;
            Life = startingLife;
            ManaPool = new ManaPool();

            // Initialize zones
            Library = new Library(this);
            Hand = new Zones.Hand(this);
            Graveyard = new Zones.Graveyard(this);
            Exile = new ExileZone();
            CommandZone = new Zones.CommandZone(this);
            Sideboard = new Zone(ZoneType.Sideboard, ZoneVisibility.Hidden, this);
        }

        #region Life Management

        public void GainLife(int amount)
        {
            if (amount > 0)
            {
                Life += amount;
            }
        }

        public void LoseLife(int amount)
        {
            if (amount > 0)
            {
                Life -= amount;
            }
        }

        public void SetLife(int amount)
        {
            Life = amount;
        }

        public void DealDamage(int amount, ICard source, bool isCombatDamage = false)
        {
            if (amount <= 0) return;

            // Check for infect
            if (source is ICreature creature && creature.HasKeyword(KeywordAbility.Infect))
            {
                AddPoisonCounters(amount);
            }
            else
            {
                // Check for lifelink
                if (source is ICreature lifelinkCreature && lifelinkCreature.HasKeyword(KeywordAbility.Lifelink))
                {
                    lifelinkCreature.Controller.GainLife(amount);
                }

                Life -= amount;
            }

            // Track commander damage
            if (source.HasSuperType(SuperType.Legendary) && isCombatDamage)
            {
                if (!_commanderDamageReceived.ContainsKey(source.Id))
                {
                    _commanderDamageReceived[source.Id] = 0;
                }
                _commanderDamageReceived[source.Id] += amount;
            }
        }

        public int GetCommanderDamageFrom(ICard commander)
        {
            return _commanderDamageReceived.TryGetValue(commander.Id, out int damage) ? damage : 0;
        }

        #endregion

        #region Counter Management

        public void AddPoisonCounters(int amount)
        {
            if (amount > 0)
            {
                PoisonCounters += amount;
            }
        }

        public void AddEnergyCounters(int amount)
        {
            if (amount > 0)
            {
                EnergyCounters += amount;
            }
        }

        public void RemoveEnergyCounters(int amount)
        {
            if (amount > 0)
            {
                EnergyCounters = Math.Max(0, EnergyCounters - amount);
            }
        }

        #endregion

        #region Card Actions

        public void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var card = Library.RemoveFromTop();
                if (card != null)
                {
                    Hand.Add(card);
                }
                else
                {
                    // Drew from empty library - lose the game
                    LoseGame(WinLossCondition.DrewFromEmptyLibrary);
                    break;
                }
            }
        }

        public void DiscardCards(int count)
        {
            // In full implementation, would prompt player to choose cards
            for (int i = 0; i < count && !Hand.IsEmpty; i++)
            {
                var card = Hand.RemoveFromTop();
                if (card != null)
                {
                    Graveyard.Add(card);
                }
            }
        }

        public void DiscardCard(ICard card)
        {
            if (Hand.Remove(card))
            {
                Graveyard.Add(card);
            }
        }

        public void Mill(int count)
        {
            for (int i = 0; i < count && !Library.IsEmpty; i++)
            {
                var card = Library.RemoveFromTop();
                if (card != null)
                {
                    Graveyard.Add(card);
                }
            }
        }

        public void Shuffle()
        {
            Library.Shuffle();
        }

        public void SearchLibrary(Func<ICard, bool> predicate, int count = 1)
        {
            // In full implementation, would show matching cards and let player choose
            var matches = Library.GetAll(predicate).Take(count);
            // After searching, library must be shuffled
        }

        #endregion

        #region Land Actions

        public bool CanPlayLand()
        {
            return LandsPlayedThisTurn < MaxLandsPerTurn;
        }

        public void PlayLand(ICard land)
        {
            if (CanPlayLand() && Hand.Remove(land))
            {
                LandsPlayedThisTurn++;
                HasPlayedLandThisTurn = true;
            }
        }

        public void ResetLandPlays()
        {
            LandsPlayedThisTurn = 0;
            HasPlayedLandThisTurn = false;
        }

        #endregion

        #region Game Actions

        public void Concede()
        {
            HasConceded = true;
            GameResult = GameResult.Loss;
        }

        public void LoseGame(WinLossCondition reason)
        {
            GameResult = GameResult.Loss;
        }

        public void WinGame(WinLossCondition reason)
        {
            GameResult = GameResult.Win;
        }

        #endregion

        #region Turn Reset

        public void ResetForNewTurn()
        {
            ResetLandPlays();
            ManaPool.Empty();
        }

        public void EmptyManaPool()
        {
            ManaPool.Empty();
        }

        #endregion

        public override string ToString() => $"{Name} (Life: {Life})";
    }
}
