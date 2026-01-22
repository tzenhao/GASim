using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;
using MTGOnline.Zones;

namespace MTGOnline.Game.State
{
    /// <summary>
    /// Represents the complete state of a Magic: The Gathering game.
    /// </summary>
    public class GameState : IGameState
    {
        public Guid GameId { get; }
        public GameStatus Status { get; private set; } = GameStatus.NotStarted;

        private readonly List<IPlayer> _players = new();
        public IReadOnlyList<IPlayer> Players => _players.AsReadOnly();

        public IPlayer ActivePlayer { get; private set; } = null!;
        public IPlayer? PriorityPlayer { get; private set; }

        public IBattlefield Battlefield { get; }
        public IStack Stack { get; }

        public ITurnManager TurnManager { get; }
        public ICombatManager CombatManager { get; }
        public IEffectManager EffectManager { get; }
        public IEventManager EventManager { get; }

        // Turn information shortcuts
        public int TurnNumber => TurnManager.TurnNumber;
        public PhaseType CurrentPhase => TurnManager.CurrentPhase;
        public StepType CurrentStep => TurnManager.CurrentStep;

        // Game configuration
        public int StartingLife { get; set; } = 20;
        public int StartingHandSize { get; set; } = 7;

        public GameState()
        {
            GameId = Guid.NewGuid();
            Battlefield = new Battlefield();
            Stack = new GameStack();
            TurnManager = new Turns.TurnManager(this);
            CombatManager = new Combat.CombatManager(this);
            EffectManager = new Effects.EffectManager();
            EventManager = new Events.EventManager();
        }

        public void AddPlayer(IPlayer player)
        {
            if (Status != GameStatus.NotStarted)
            {
                throw new InvalidOperationException("Cannot add players after game has started");
            }
            _players.Add(player);
        }

        public void StartGame()
        {
            if (_players.Count < 2)
            {
                throw new InvalidOperationException("Need at least 2 players to start");
            }

            Status = GameStatus.InProgress;

            // Determine starting player (random for now)
            var random = new Random();
            int startingPlayerIndex = random.Next(_players.Count);
            ActivePlayer = _players[startingPlayerIndex];

            // Each player draws starting hand
            foreach (var player in _players)
            {
                player.DrawCards(StartingHandSize);
            }

            // Start first turn
            ((Turns.TurnManager)TurnManager).StartTurn(ActivePlayer);
            GivePriority(ActivePlayer);
        }

        public void CheckStateBasedActions()
        {
            bool actionsTaken;
            do
            {
                actionsTaken = false;

                // Check for player losses
                foreach (var player in _players.Where(p => p.GameResult == GameResult.InProgress))
                {
                    // Life <= 0
                    if (player.Life <= 0)
                    {
                        player.LoseGame(WinLossCondition.LifeReachedZero);
                        actionsTaken = true;
                    }

                    // 10+ poison counters
                    if (player.PoisonCounters >= 10)
                    {
                        player.LoseGame(WinLossCondition.TenOrMorePoison);
                        actionsTaken = true;
                    }
                }

                // Check for creature deaths
                foreach (var creature in Battlefield.Creatures.ToList())
                {
                    // Toughness <= 0
                    if (creature.Toughness <= 0)
                    {
                        DestroyCreature(creature);
                        actionsTaken = true;
                        continue;
                    }

                    // Lethal damage
                    if (creature.DamageMarked >= creature.Toughness)
                    {
                        if (!creature.HasKeyword(KeywordAbility.Indestructible))
                        {
                            DestroyCreature(creature);
                            actionsTaken = true;
                        }
                    }
                }

                // Check for planeswalker deaths (0 loyalty)
                foreach (var planeswalker in Battlefield.Planeswalkers.ToList())
                {
                    if (planeswalker.CurrentLoyalty <= 0)
                    {
                        DestroyPlaneswalker(planeswalker);
                        actionsTaken = true;
                    }
                }

                // Check for aura validity
                foreach (var enchantment in Battlefield.Enchantments.OfType<IAura>().ToList())
                {
                    if (enchantment.EnchantedPermanent == null && enchantment.EnchantedPlayer == null)
                    {
                        // Unattached aura goes to graveyard
                        ((Battlefield)Battlefield).RemovePermanent(enchantment);
                        enchantment.Owner.Graveyard.Add(enchantment as ICard ?? throw new InvalidOperationException());
                        actionsTaken = true;
                    }
                }

                // Legend rule - if a player controls multiple legendary permanents with same name
                var legendGroups = Battlefield.Permanents
                    .Where(p => p.HasSuperType(SuperType.Legendary))
                    .GroupBy(p => new { p.Controller, p.Name })
                    .Where(g => g.Count() > 1);

                foreach (var group in legendGroups)
                {
                    // Player chooses one to keep, others go to graveyard
                    // For now, keep the most recently entered
                    var toDestroy = group.OrderBy(p => p.EnteredBattlefieldTimestamp).SkipLast(1).ToList();
                    foreach (var permanent in toDestroy)
                    {
                        ((Battlefield)Battlefield).RemovePermanent(permanent);
                        permanent.Owner.Graveyard.Add(permanent as ICard ?? throw new InvalidOperationException());
                        actionsTaken = true;
                    }
                }

            } while (actionsTaken);

            // Check for game over
            CheckGameOver();
        }

        private void DestroyCreature(ICreature creature)
        {
            ((Battlefield)Battlefield).RemovePermanent(creature);
            creature.Owner.Graveyard.Add(creature as ICard ?? throw new InvalidOperationException());
        }

        private void DestroyPlaneswalker(IPlaneswalker planeswalker)
        {
            ((Battlefield)Battlefield).RemovePermanent(planeswalker);
            planeswalker.Owner.Graveyard.Add(planeswalker as ICard ?? throw new InvalidOperationException());
        }

        private void CheckGameOver()
        {
            var activePlayers = _players.Where(p => p.GameResult == GameResult.InProgress).ToList();

            if (activePlayers.Count == 1)
            {
                activePlayers[0].WinGame(WinLossCondition.OpponentLost);
                Status = GameStatus.Completed;
            }
            else if (activePlayers.Count == 0)
            {
                // Draw - no one wins
                Status = GameStatus.Completed;
            }
        }

        #region Priority

        public void PassPriority()
        {
            if (PriorityPlayer == null) return;

            // Find next player in turn order
            int currentIndex = _players.IndexOf(PriorityPlayer);
            int nextIndex = (currentIndex + 1) % _players.Count;
            var nextPlayer = _players[nextIndex];

            // If we've gone around to the active player and stack is empty, move to next step
            if (nextPlayer == ActivePlayer)
            {
                if (Stack.IsEmpty)
                {
                    TurnManager.MoveToNextStep();
                    GivePriority(ActivePlayer);
                }
                else
                {
                    // Resolve top of stack
                    var topObject = ((GameStack)Stack).Pop();
                    topObject?.Resolve(this);
                    CheckStateBasedActions();
                    GivePriority(ActivePlayer);
                }
            }
            else
            {
                GivePriority(nextPlayer);
            }
        }

        public void GivePriority(IPlayer player)
        {
            PriorityPlayer = player;
            if (player is Players.Player p)
            {
                p.HasPriority = true;
            }

            // Remove priority from other players
            foreach (var otherPlayer in _players.Where(pl => pl != player))
            {
                if (otherPlayer is Players.Player op)
                {
                    op.HasPriority = false;
                }
            }
        }

        #endregion

        #region Game Flow

        public void EndGame(IPlayer winner, WinLossCondition reason)
        {
            winner.WinGame(reason);
            foreach (var player in _players.Where(p => p != winner))
            {
                player.LoseGame(WinLossCondition.OpponentLost);
            }
            Status = GameStatus.Completed;
        }

        public void Draw()
        {
            foreach (var player in _players)
            {
                // In a draw, no one wins
            }
            Status = GameStatus.Completed;
        }

        #endregion

        #region Utility

        public IPlayer GetOpponent(IPlayer player)
        {
            return _players.First(p => p != player);
        }

        public IReadOnlyList<IPlayer> GetOpponents(IPlayer player)
        {
            return _players.Where(p => p != player).ToList().AsReadOnly();
        }

        public ICard? FindCard(Guid cardId)
        {
            // Search all zones
            foreach (var player in _players)
            {
                var card = player.Library.Cards.FirstOrDefault(c => c.Id == cardId);
                if (card != null) return card;

                card = player.Hand.Cards.FirstOrDefault(c => c.Id == cardId);
                if (card != null) return card;

                card = player.Graveyard.Cards.FirstOrDefault(c => c.Id == cardId);
                if (card != null) return card;

                card = player.Exile.Cards.FirstOrDefault(c => c.Id == cardId);
                if (card != null) return card;
            }

            // Check battlefield
            var permanent = Battlefield.Cards.FirstOrDefault(c => c.Id == cardId);
            if (permanent != null) return permanent;

            // Check stack
            return Stack.Cards.FirstOrDefault(c => c.Id == cardId);
        }

        public IPermanent? FindPermanent(Guid permanentId)
        {
            return Battlefield.Permanents.FirstOrDefault(p => p.Id == permanentId);
        }

        #endregion
    }
}
