using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;

namespace MTGOnline.Game.Turns
{
    /// <summary>
    /// Manages turns, phases, and steps in a Magic: The Gathering game.
    /// </summary>
    public class TurnManager : ITurnManager
    {
        private readonly IGameState _gameState;
        private readonly Queue<IPlayer> _extraTurns = new();
        private readonly HashSet<IPlayer> _skipNextTurn = new();

        public int TurnNumber { get; private set; }
        public IPlayer ActivePlayer { get; private set; } = null!;
        public PhaseType CurrentPhase { get; private set; }
        public StepType CurrentStep { get; private set; }

        public bool IsMainPhase =>
            CurrentPhase == PhaseType.PreCombatMain ||
            CurrentPhase == PhaseType.PostCombatMain;

        public bool IsCombatPhase => CurrentPhase == PhaseType.Combat;

        // Turn structure definition
        private static readonly (PhaseType Phase, StepType Step)[] TurnStructure = new[]
        {
            // Beginning Phase
            (PhaseType.Beginning, StepType.Untap),
            (PhaseType.Beginning, StepType.Upkeep),
            (PhaseType.Beginning, StepType.Draw),

            // Pre-combat Main Phase
            (PhaseType.PreCombatMain, StepType.Main),

            // Combat Phase
            (PhaseType.Combat, StepType.BeginningOfCombat),
            (PhaseType.Combat, StepType.DeclareAttackers),
            (PhaseType.Combat, StepType.DeclareBlockers),
            (PhaseType.Combat, StepType.FirstStrikeDamage),
            (PhaseType.Combat, StepType.CombatDamage),
            (PhaseType.Combat, StepType.EndOfCombat),

            // Post-combat Main Phase
            (PhaseType.PostCombatMain, StepType.Main),

            // Ending Phase
            (PhaseType.Ending, StepType.End),
            (PhaseType.Ending, StepType.Cleanup)
        };

        private int _currentStepIndex;

        public TurnManager(IGameState gameState)
        {
            _gameState = gameState;
        }

        public void StartTurn(IPlayer player)
        {
            ActivePlayer = player;
            TurnNumber++;
            _currentStepIndex = 0;

            // Reset player state for new turn
            if (player is Players.Player p)
            {
                p.ResetForNewTurn();
            }

            CurrentPhase = TurnStructure[0].Phase;
            CurrentStep = TurnStructure[0].Step;

            // Execute untap step
            ExecuteCurrentStep();
        }

        public void EndTurn()
        {
            // Determine next player
            IPlayer nextPlayer;

            // Check for extra turns
            if (_extraTurns.Count > 0)
            {
                nextPlayer = _extraTurns.Dequeue();
            }
            else
            {
                // Normal turn order
                var players = _gameState.Players.ToList();
                int currentIndex = players.IndexOf(ActivePlayer);
                int nextIndex = (currentIndex + 1) % players.Count;
                nextPlayer = players[nextIndex];

                // Check if next player's turn should be skipped
                while (_skipNextTurn.Contains(nextPlayer))
                {
                    _skipNextTurn.Remove(nextPlayer);
                    nextIndex = (nextIndex + 1) % players.Count;
                    nextPlayer = players[nextIndex];

                    // Avoid infinite loop if all players skip
                    if (nextPlayer == ActivePlayer) break;
                }
            }

            // Start next turn
            StartTurn(nextPlayer);
        }

        public void MoveToNextPhase()
        {
            // Skip to next phase
            var currentPhase = CurrentPhase;
            while (_currentStepIndex < TurnStructure.Length - 1 &&
                   TurnStructure[_currentStepIndex].Phase == currentPhase)
            {
                _currentStepIndex++;
            }

            if (_currentStepIndex < TurnStructure.Length)
            {
                CurrentPhase = TurnStructure[_currentStepIndex].Phase;
                CurrentStep = TurnStructure[_currentStepIndex].Step;
                ExecuteCurrentStep();
            }
            else
            {
                EndTurn();
            }
        }

        public void MoveToNextStep()
        {
            _currentStepIndex++;

            // Skip first strike damage step if no first strike creatures
            if (_currentStepIndex < TurnStructure.Length &&
                TurnStructure[_currentStepIndex].Step == StepType.FirstStrikeDamage)
            {
                if (!HasFirstStrikeCreatures())
                {
                    _currentStepIndex++;
                }
            }

            if (_currentStepIndex < TurnStructure.Length)
            {
                CurrentPhase = TurnStructure[_currentStepIndex].Phase;
                CurrentStep = TurnStructure[_currentStepIndex].Step;
                ExecuteCurrentStep();
            }
            else
            {
                EndTurn();
            }
        }

        private void ExecuteCurrentStep()
        {
            // Raise phase/step event
            _gameState.EventManager.RaiseEvent(new PhaseStepEvent(CurrentPhase, CurrentStep, ActivePlayer));

            switch (CurrentStep)
            {
                case StepType.Untap:
                    ExecuteUntapStep();
                    // No priority during untap step - move directly to upkeep
                    MoveToNextStep();
                    break;

                case StepType.Upkeep:
                    // Players get priority during upkeep
                    break;

                case StepType.Draw:
                    ExecuteDrawStep();
                    // Players get priority after draw
                    break;

                case StepType.Main:
                    // Players can cast spells and play lands
                    break;

                case StepType.BeginningOfCombat:
                    _gameState.CombatManager.BeginCombat(ActivePlayer, _gameState.GetOpponent(ActivePlayer));
                    break;

                case StepType.DeclareAttackers:
                    // Active player declares attackers
                    break;

                case StepType.DeclareBlockers:
                    // Defending player declares blockers
                    break;

                case StepType.FirstStrikeDamage:
                    _gameState.CombatManager.ApplyFirstStrikeDamage();
                    _gameState.CheckStateBasedActions();
                    break;

                case StepType.CombatDamage:
                    _gameState.CombatManager.ApplyRegularCombatDamage();
                    _gameState.CheckStateBasedActions();
                    break;

                case StepType.EndOfCombat:
                    _gameState.CombatManager.EndCombat();
                    break;

                case StepType.End:
                    // "At end of turn" triggers happen here
                    break;

                case StepType.Cleanup:
                    ExecuteCleanupStep();
                    break;
            }
        }

        private void ExecuteUntapStep()
        {
            // Untap all permanents controlled by active player
            foreach (var permanent in _gameState.Battlefield.GetPermanentsControlledBy(ActivePlayer))
            {
                // Check for "doesn't untap" effects
                // For now, just untap everything
                permanent.Untap();
            }

            // Reset "once per turn" abilities
            foreach (var planeswalker in _gameState.Battlefield.Planeswalkers
                .Where(pw => pw.Controller == ActivePlayer))
            {
                if (planeswalker is Cards.Types.PlaneswalkerCard pw)
                {
                    pw.ResetLoyaltyActivation();
                }
            }
        }

        private void ExecuteDrawStep()
        {
            // Active player draws a card (skip on first turn of the game for starting player)
            if (!(TurnNumber == 1 && _gameState.Players.IndexOf(ActivePlayer) == 0))
            {
                ActivePlayer.DrawCards(1);
            }
        }

        private void ExecuteCleanupStep()
        {
            // Discard down to max hand size
            var hand = ActivePlayer.Hand;
            if (hand is Zones.Hand h && h.ExceedsMaxHandSize)
            {
                ActivePlayer.DiscardCards(h.CardsToDiscard);
            }

            // Remove damage from creatures
            foreach (var creature in _gameState.Battlefield.Creatures)
            {
                creature.ClearDamage();
            }

            // End "until end of turn" effects
            _gameState.EffectManager.CleanupExpiredEffects();

            // Empty mana pools
            foreach (var player in _gameState.Players)
            {
                if (player is Players.Player p)
                {
                    p.EmptyManaPool();
                }
            }

            // If state-based actions or triggers happen, repeat cleanup
            // For simplicity, just end the turn
        }

        private bool HasFirstStrikeCreatures()
        {
            return _gameState.CombatManager.Attackers
                       .Any(a => a.Creature.HasKeyword(KeywordAbility.FirstStrike) ||
                                 a.Creature.HasKeyword(KeywordAbility.DoubleStrike)) ||
                   _gameState.CombatManager.Blockers
                       .Any(b => b.Creature.HasKeyword(KeywordAbility.FirstStrike) ||
                                 b.Creature.HasKeyword(KeywordAbility.DoubleStrike));
        }

        public void AddExtraTurn(IPlayer player)
        {
            _extraTurns.Enqueue(player);
        }

        public void SkipNextTurn(IPlayer player)
        {
            _skipNextTurn.Add(player);
        }
    }

    /// <summary>
    /// Event for phase/step changes.
    /// </summary>
    public class PhaseStepEvent : IPhaseStepEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public bool WasReplaced { get; set; }
        public bool WasPrevented { get; set; }

        public PhaseType Phase { get; }
        public StepType Step { get; }
        public IPlayer ActivePlayer { get; }

        public PhaseStepEvent(PhaseType phase, StepType step, IPlayer activePlayer)
        {
            Phase = phase;
            Step = step;
            ActivePlayer = activePlayer;
        }
    }
}
