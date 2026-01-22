using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Game
{
    /// <summary>
    /// Manages turn order and phases in Grand Archive TCG.
    /// Turn order: Wake Up → Materialize → Recollection → Draw → Main → End
    /// </summary>
    public class TurnManager
    {
        private readonly GameState _gameState;

        public PhaseType CurrentPhase { get; private set; } = PhaseType.Main;

        public CombatStep? CurrentCombatStep { get; private set; }

        public int TurnNumber { get; private set; } = 1;

        public bool IsFirstTurn => TurnNumber == 1;

        /// <summary>
        /// Whether the first turn player should skip certain phases.
        /// </summary>
        public bool FirstTurnPlayerSkipsPhases { get; set; } = true;

        public TurnManager(GameState gameState)
        {
            _gameState = gameState;
        }

        /// <summary>
        /// Start a new turn for the next player.
        /// </summary>
        public void StartNewTurn()
        {
            // Switch turn player
            _gameState.AdvanceTurnPlayer();
            TurnNumber++;

            // Start with Wake Up phase
            CurrentPhase = PhaseType.WakeUp;
            CurrentCombatStep = null;

            // Handle first turn skips
            if (ShouldSkipPhase(PhaseType.WakeUp))
            {
                AdvancePhase();
            }
            else
            {
                ExecuteWakeUpPhase();
            }
        }

        /// <summary>
        /// Check if a phase should be skipped (first turn rules).
        /// </summary>
        private bool ShouldSkipPhase(PhaseType phase)
        {
            if (!FirstTurnPlayerSkipsPhases) return false;

            // First turn player skips Wake Up, Materialize, Recollection, and Draw
            if (TurnNumber == 1 && _gameState.TurnPlayer == _gameState.Players[0])
            {
                return phase == PhaseType.WakeUp ||
                       phase == PhaseType.Materialize ||
                       phase == PhaseType.Recollection ||
                       phase == PhaseType.Draw;
            }

            // Second player on their first turn skips Wake Up, Materialize, and Recollection
            if (TurnNumber == 2 && _gameState.TurnPlayer == _gameState.Players[1])
            {
                return phase == PhaseType.WakeUp ||
                       phase == PhaseType.Materialize ||
                       phase == PhaseType.Recollection;
            }

            return false;
        }

        /// <summary>
        /// Advance to the next phase.
        /// </summary>
        public void AdvancePhase()
        {
            // End current phase effects
            _gameState.EventManager.TriggerPhaseEnd(CurrentPhase);

            // Determine next phase
            CurrentPhase = GetNextPhase(CurrentPhase);

            // Skip phases if needed (first turn rules)
            while (ShouldSkipPhase(CurrentPhase) && CurrentPhase != PhaseType.End)
            {
                CurrentPhase = GetNextPhase(CurrentPhase);
            }

            // Execute phase start
            ExecutePhaseStart();
        }

        /// <summary>
        /// Get the next phase in turn order.
        /// </summary>
        private PhaseType GetNextPhase(PhaseType current)
        {
            return current switch
            {
                PhaseType.WakeUp => PhaseType.Materialize,
                PhaseType.Materialize => PhaseType.Recollection,
                PhaseType.Recollection => PhaseType.Draw,
                PhaseType.Draw => PhaseType.Main,
                PhaseType.Main => PhaseType.End,
                PhaseType.Combat => PhaseType.Main, // Return to main after combat
                PhaseType.End => PhaseType.WakeUp, // New turn
                _ => PhaseType.Main
            };
        }

        /// <summary>
        /// Execute the start of the current phase.
        /// </summary>
        private void ExecutePhaseStart()
        {
            _gameState.EventManager.TriggerPhaseStart(CurrentPhase);

            switch (CurrentPhase)
            {
                case PhaseType.WakeUp:
                    ExecuteWakeUpPhase();
                    break;
                case PhaseType.Materialize:
                    ExecuteMaterializePhase();
                    break;
                case PhaseType.Recollection:
                    ExecuteRecollectionPhase();
                    break;
                case PhaseType.Draw:
                    ExecuteDrawPhase();
                    break;
                case PhaseType.Main:
                    ExecuteMainPhase();
                    break;
                case PhaseType.End:
                    ExecuteEndPhase();
                    break;
            }
        }

        /// <summary>
        /// Wake Up Phase - all rested cards controlled by turn player wake up.
        /// </summary>
        private void ExecuteWakeUpPhase()
        {
            // Wake up all units controlled by turn player
            _gameState.Field.WakeUpAllUnits(_gameState.TurnPlayer);

            // No opportunity in this phase, auto-advance
            AdvancePhase();
        }

        /// <summary>
        /// Materialize Phase - turn player may materialize one card from material deck.
        /// </summary>
        private void ExecuteMaterializePhase()
        {
            // Turn player can materialize a card
            // This is handled by player action, not automatic
            // For now, auto-advance (player would need to take action)
            AdvancePhase();
        }

        /// <summary>
        /// Recollection Phase - turn player returns cards from memory to hand.
        /// </summary>
        private void ExecuteRecollectionPhase()
        {
            // Turn player receives Opportunity
            _gameState.TurnPlayer.HasOpportunity = true;

            // Turn player can recollect cards
            // Default: recollect 1 card
            if (_gameState.TurnPlayer is Players.Player player)
            {
                player.Recollect(player.RecollectionAmount);
            }
        }

        /// <summary>
        /// Draw Phase - turn player draws a card.
        /// </summary>
        private void ExecuteDrawPhase()
        {
            // Turn player draws a card
            _gameState.TurnPlayer.DrawCard();

            // No opportunity in this phase, auto-advance
            AdvancePhase();
        }

        /// <summary>
        /// Main Phase - turn player can play cards and declare attacks.
        /// </summary>
        private void ExecuteMainPhase()
        {
            // Turn player receives Opportunity
            _gameState.TurnPlayer.HasOpportunity = true;

            // Player can now take actions
            // This phase continues until player passes or declares attack
        }

        /// <summary>
        /// End Phase - turn ends, effects expire.
        /// </summary>
        private void ExecuteEndPhase()
        {
            // Turn player receives Opportunity
            _gameState.TurnPlayer.HasOpportunity = true;

            // Process "until end of turn" effects
            _gameState.EffectManager.ProcessEndOfTurnEffects();

            // Discard to hand size if needed
            if (_gameState.TurnPlayer is Players.Player player)
            {
                player.DiscardToHandSize();
            }
        }

        /// <summary>
        /// Enter combat phase (when attack is declared).
        /// </summary>
        public void EnterCombat()
        {
            CurrentPhase = PhaseType.Combat;
            CurrentCombatStep = CombatStep.AttackDeclaration;
            _gameState.EventManager.TriggerCombatStart();
        }

        /// <summary>
        /// Advance to the next combat step.
        /// </summary>
        public void AdvanceCombatStep()
        {
            if (CurrentCombatStep == null) return;

            CurrentCombatStep = CurrentCombatStep switch
            {
                CombatStep.AttackDeclaration => CombatStep.Interception,
                CombatStep.Interception => CombatStep.Retaliation,
                CombatStep.Retaliation => CombatStep.Damage,
                CombatStep.Damage => CombatStep.CombatEnd,
                CombatStep.CombatEnd => null,
                _ => null
            };

            if (CurrentCombatStep == null)
            {
                // Combat ended, return to main phase
                ExitCombat();
            }
        }

        /// <summary>
        /// Exit combat and return to main phase.
        /// </summary>
        public void ExitCombat()
        {
            CurrentCombatStep = null;
            CurrentPhase = PhaseType.Main;
            _gameState.EventManager.TriggerCombatEnd();

            // Turn player regains opportunity
            _gameState.TurnPlayer.HasOpportunity = true;
        }

        /// <summary>
        /// End the current turn and start the next player's turn.
        /// </summary>
        public void EndTurn()
        {
            // Process end of turn
            _gameState.EventManager.TriggerTurnEnd(_gameState.TurnPlayer);

            // Start new turn
            StartNewTurn();

            // Trigger turn start
            _gameState.EventManager.TriggerTurnStart(_gameState.TurnPlayer);
        }
    }
}
