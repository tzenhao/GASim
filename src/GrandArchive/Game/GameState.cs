using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;
using GrandArchive.Players;
using GrandArchive.Zones;

namespace GrandArchive.Game
{
    /// <summary>
    /// Main game state for Grand Archive TCG.
    /// </summary>
    public class GameState : IGameState
    {
        private readonly List<Player> _players = new();
        private int _turnPlayerIndex = 0;

        public IReadOnlyList<IPlayer> Players => _players.AsReadOnly();

        public IPlayer TurnPlayer => _players[_turnPlayerIndex];

        public PhaseType CurrentPhase => TurnManager.CurrentPhase;

        public CombatStep? CurrentCombatStep => TurnManager.CurrentCombatStep;

        public int TurnNumber => TurnManager.TurnNumber;

        /// <summary>
        /// The shared field zone.
        /// </summary>
        public Field Field { get; } = new();
        IZone IGameState.Field => Field;

        /// <summary>
        /// The effects stack.
        /// </summary>
        public EffectsStack EffectsStackZone { get; } = new();
        IEffectsStack IGameState.EffectsStack => EffectsStackZone;

        /// <summary>
        /// Turn manager for phase progression.
        /// </summary>
        public TurnManager TurnManager { get; }

        /// <summary>
        /// Event manager for game events.
        /// </summary>
        public EventManager EventManager { get; }

        /// <summary>
        /// Effect manager for continuous effects.
        /// </summary>
        public EffectManager EffectManager { get; }

        /// <summary>
        /// Combat manager for combat resolution.
        /// </summary>
        public CombatManager CombatManager { get; }

        public bool IsGameOver { get; private set; }

        public IPlayer? Winner { get; private set; }

        public GameState()
        {
            TurnManager = new TurnManager(this);
            EventManager = new EventManager(this);
            EffectManager = new EffectManager(this);
            CombatManager = new CombatManager(this);
        }

        /// <summary>
        /// Add a player to the game.
        /// </summary>
        public void AddPlayer(Player player)
        {
            _players.Add(player);
        }

        /// <summary>
        /// Initialize the game (setup phase).
        /// </summary>
        public void InitializeGame()
        {
            if (_players.Count < 2)
                throw new InvalidOperationException("Need at least 2 players to start");

            // Each player materializes their champion
            foreach (var player in _players)
            {
                player.MaterializeChampion();

                // Add champion to field
                if (player.Champion != null)
                {
                    Field.Add((ICard)player.Champion);
                }

                // Draw starting hand (5 cards)
                player.DrawCards(5);
            }

            // Determine first player (could be random or by some rule)
            _turnPlayerIndex = 0;
            _players[_turnPlayerIndex].IsTurnPlayer = true;

            // Start the first turn
            EventManager.TriggerGameStart();
            EventManager.TriggerTurnStart(TurnPlayer);
        }

        /// <summary>
        /// Get the opponent of a player (2-player game).
        /// </summary>
        public IPlayer GetOpponent(IPlayer player)
        {
            var index = _players.IndexOf((Player)player);
            return _players[(index + 1) % _players.Count];
        }

        /// <summary>
        /// Check if a player can currently act.
        /// </summary>
        public bool CanPlayerAct(IPlayer player)
        {
            if (IsGameOver) return false;

            // During main phase, turn player can act
            if (CurrentPhase == PhaseType.Main && player == TurnPlayer)
                return true;

            // Player with opportunity can act
            return player.HasOpportunity;
        }

        /// <summary>
        /// Pass opportunity to the next player.
        /// </summary>
        public void PassOpportunity()
        {
            // Current player passes
            var currentPlayer = _players.FirstOrDefault(p => p.HasOpportunity);
            if (currentPlayer != null)
            {
                currentPlayer.HasOpportunity = false;
            }

            // If stack is empty and all players passed, advance phase
            if (EffectsStackZone.IsEmpty)
            {
                // Check if we should advance phase or resolve stack
                TurnManager.AdvancePhase();
            }
            else
            {
                // Give opportunity to next player or resolve stack
                var nextPlayer = GetOpponent(currentPlayer ?? TurnPlayer);
                nextPlayer.HasOpportunity = true;
            }
        }

        /// <summary>
        /// Advance to the next phase.
        /// </summary>
        public void AdvancePhase()
        {
            TurnManager.AdvancePhase();
        }

        /// <summary>
        /// Advance the turn player to the next player.
        /// </summary>
        public void AdvanceTurnPlayer()
        {
            _players[_turnPlayerIndex].IsTurnPlayer = false;
            _turnPlayerIndex = (_turnPlayerIndex + 1) % _players.Count;
            _players[_turnPlayerIndex].IsTurnPlayer = true;
        }

        /// <summary>
        /// Start a new turn.
        /// </summary>
        public void StartNewTurn()
        {
            TurnManager.StartNewTurn();
        }

        /// <summary>
        /// End the game with a winner.
        /// </summary>
        public void EndGame(IPlayer winner)
        {
            IsGameOver = true;
            Winner = winner;
            EventManager.TriggerGameEnd(winner);
        }

        /// <summary>
        /// Check for game-ending conditions.
        /// </summary>
        public void CheckGameEndConditions()
        {
            foreach (var player in _players)
            {
                // Check if champion is defeated
                if (player.IsChampionDefeated)
                {
                    player.Lose("Champion defeated");
                }

                // Check if player has lost
                if (player.HasLost)
                {
                    var winner = GetOpponent(player);
                    EndGame(winner);
                    return;
                }
            }
        }

        /// <summary>
        /// Resolve the effects stack until empty.
        /// </summary>
        public void ResolveStack()
        {
            while (!EffectsStackZone.IsEmpty)
            {
                var top = EffectsStackZone.Top;
                if (top != null)
                {
                    EffectsStackZone.ResolveTop();
                    top.Resolve(this);

                    // Check for game-ending conditions after each resolution
                    CheckGameEndConditions();
                    if (IsGameOver) return;
                }
            }
        }

        /// <summary>
        /// Move a card between zones.
        /// </summary>
        public void MoveCard(ICard card, IZone fromZone, IZone toZone)
        {
            var previousZone = card.CurrentZone;
            fromZone.Remove(card);
            toZone.Add(card);

            EventManager.TriggerZoneChange(card, previousZone, toZone.ZoneType);
        }
    }
}
