using GrandArchive.Cards.Types;
using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Game
{
    /// <summary>
    /// Manages combat in Grand Archive TCG.
    /// </summary>
    public class CombatManager
    {
        private readonly GameState _gameState;

        /// <summary>
        /// The current attacker (champion or ally).
        /// </summary>
        public IUnit? Attacker { get; private set; }

        /// <summary>
        /// The attack card being used.
        /// </summary>
        public AttackCard? AttackCard { get; private set; }

        /// <summary>
        /// The target of the attack.
        /// </summary>
        public IUnit? Target { get; private set; }

        /// <summary>
        /// Units intercepting the attack.
        /// </summary>
        public List<IUnit> Interceptors { get; } = new();

        /// <summary>
        /// Units retaliating against the attacker.
        /// </summary>
        public List<IUnit> Retaliators { get; } = new();

        /// <summary>
        /// Whether combat is currently active.
        /// </summary>
        public bool IsInCombat => Attacker != null;

        public CombatManager(GameState gameState)
        {
            _gameState = gameState;
        }

        /// <summary>
        /// Declare an attack.
        /// </summary>
        public bool DeclareAttack(IUnit attacker, AttackCard attackCard, IUnit target)
        {
            // Validate attack
            if (!CanAttack(attacker, attackCard, target))
                return false;

            Attacker = attacker;
            AttackCard = attackCard;
            Target = target;

            // Rest the attacker
            attacker.Rest();

            // Add attack card to intent (for allies)
            if (attacker is AllyCard ally)
            {
                ally.AddToIntent(attackCard);
                ally.IsAttacking = true;
            }

            // Enter combat phase
            _gameState.TurnManager.EnterCombat();

            return true;
        }

        /// <summary>
        /// Check if an attack is valid.
        /// </summary>
        public bool CanAttack(IUnit attacker, AttackCard attackCard, IUnit target)
        {
            // Attacker must be awake
            if (attacker.State != UnitState.Awake)
                return false;

            // Attacker must not be defeated
            if (attacker.IsDefeated)
                return false;

            // Target must be valid
            if (target.IsDefeated)
                return false;

            // Check for Floating (can't attack floating units without Ranged)
            if (((ICard)target).HasKeyword(Keyword.Floating) &&
                !((ICard)attacker).HasKeyword(Keyword.Ranged) &&
                !attackCard.HasKeyword(Keyword.Ranged))
                return false;

            // Check for Pride restrictions
            if (attackCard.HasKeyword(Keyword.Pride))
            {
                // Pride condition would be checked here
            }

            return true;
        }

        /// <summary>
        /// Declare an interception.
        /// </summary>
        public bool DeclareInterception(IUnit interceptor)
        {
            if (!CanIntercept(interceptor))
                return false;

            Interceptors.Add(interceptor);
            interceptor.Rest();

            if (interceptor is AllyCard ally)
            {
                ally.IsDefending = true;
            }

            return true;
        }

        /// <summary>
        /// Check if a unit can intercept.
        /// </summary>
        public bool CanIntercept(IUnit interceptor)
        {
            // Must be awake
            if (interceptor.State != UnitState.Awake)
                return false;

            // Must have Intercept keyword
            if (!((ICard)interceptor).HasKeyword(Keyword.Intercept))
                return false;

            // Attack must not have Stealth (unless interceptor has True Sight)
            if (AttackCard?.HasStealth == true &&
                !((ICard)interceptor).HasKeyword(Keyword.TrueSight))
                return false;

            // Attack must not have Cleave
            if (AttackCard?.HasCleave == true)
                return false;

            return true;
        }

        /// <summary>
        /// Declare a retaliation.
        /// </summary>
        public bool DeclareRetaliation(IUnit retaliator)
        {
            if (!CanRetaliate(retaliator))
                return false;

            Retaliators.Add(retaliator);
            return true;
        }

        /// <summary>
        /// Check if a unit can retaliate.
        /// </summary>
        public bool CanRetaliate(IUnit retaliator)
        {
            // Must be awake
            if (retaliator.State != UnitState.Awake)
                return false;

            // Must be defending or have Ambush
            var card = (ICard)retaliator;
            bool isDefending = retaliator == Target || Interceptors.Contains(retaliator);
            bool hasAmbush = card.HasKeyword(Keyword.Ambush);

            return isDefending || hasAmbush;
        }

        /// <summary>
        /// Process the damage step.
        /// </summary>
        public void ProcessDamage()
        {
            if (Attacker == null || AttackCard == null)
                return;

            // Calculate attacker's damage
            int attackerDamage = CalculateAttackerDamage();

            // Determine who receives damage
            var defenders = Interceptors.Count > 0 ? Interceptors : new List<IUnit> { Target! };

            // Deal damage to defenders
            foreach (var defender in defenders.Where(d => d != null))
            {
                // Check for Critical
                int damageToDefender = attackerDamage;
                if (AttackCard.HasKeyword(Keyword.None)) // Would check for Critical N
                {
                    // Critical logic would go here
                }

                DealDamage(defender, damageToDefender, AttackCard, true);
            }

            // Retaliators deal damage to attacker
            foreach (var retaliator in Retaliators)
            {
                int retaliationDamage = retaliator.Power;
                DealDamage(Attacker, retaliationDamage, (ICard)retaliator, true);
            }

            // Check for defeated units
            CheckDefeatedUnits();
        }

        /// <summary>
        /// Calculate the attacker's total damage.
        /// </summary>
        private int CalculateAttackerDamage()
        {
            int damage = Attacker!.Power;

            // Add attack card power
            if (AttackCard != null)
            {
                damage += AttackCard.Power;
            }

            // Add weapon power if equipped
            // This would check for equipped weapons

            return damage;
        }

        /// <summary>
        /// Deal damage to a unit.
        /// </summary>
        private void DealDamage(IUnit target, int amount, ICard source, bool isCombatDamage)
        {
            if (amount <= 0) return;

            // Apply damage counters
            target.TakeDamage(amount);

            // Trigger damage event
            _gameState.EventManager.TriggerDamageDealt(target, amount, source, isCombatDamage);
        }

        /// <summary>
        /// Check for and process defeated units.
        /// </summary>
        private void CheckDefeatedUnits()
        {
            var unitsToCheck = new List<IUnit> { Attacker!, Target! };
            unitsToCheck.AddRange(Interceptors);

            foreach (var unit in unitsToCheck.Where(u => u != null && u.IsDefeated))
            {
                ProcessDefeatedUnit(unit);
            }
        }

        /// <summary>
        /// Process a defeated unit.
        /// </summary>
        private void ProcessDefeatedUnit(IUnit unit)
        {
            _gameState.EventManager.TriggerUnitDefeated(unit);

            var card = (ICard)unit;

            // Move to graveyard (allies) or handle champion defeat
            if (card.CardType == CardType.Ally)
            {
                _gameState.Field.Remove(card);
                card.Owner?.Graveyard.Add(card);
            }
            else if (card.CardType == CardType.Champion)
            {
                // Champion defeat - player loses
                if (card.Controller is Players.Player player)
                {
                    player.Lose("Champion defeated");
                }
            }
        }

        /// <summary>
        /// End combat and clean up.
        /// </summary>
        public void EndCombat()
        {
            // Clear intent for allies
            if (Attacker is AllyCard attackingAlly)
            {
                attackingAlly.ClearIntent();
                attackingAlly.IsAttacking = false;
            }

            foreach (var interceptor in Interceptors)
            {
                if (interceptor is AllyCard ally)
                {
                    ally.IsDefending = false;
                }
            }

            // Move attack card to graveyard
            if (AttackCard != null)
            {
                _gameState.Field.Remove(AttackCard);
                AttackCard.Owner?.Graveyard.Add(AttackCard);
            }

            // Reset combat state
            Attacker = null;
            AttackCard = null;
            Target = null;
            Interceptors.Clear();
            Retaliators.Clear();

            // Exit combat phase
            _gameState.TurnManager.ExitCombat();
        }
    }
}
