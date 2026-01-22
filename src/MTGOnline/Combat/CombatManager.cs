using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;

namespace MTGOnline.Combat
{
    /// <summary>
    /// Manages combat in Magic: The Gathering.
    /// </summary>
    public class CombatManager : ICombatManager
    {
        private readonly IGameState _gameState;

        public bool IsInCombat { get; private set; }
        public IPlayer? AttackingPlayer { get; private set; }
        public IPlayer? DefendingPlayer { get; private set; }
        public CombatStep CurrentCombatStep { get; private set; } = CombatStep.NotInCombat;

        private readonly List<IAttacker> _attackers = new();
        public IReadOnlyList<IAttacker> Attackers => _attackers.AsReadOnly();

        private readonly List<IBlocker> _blockers = new();
        public IReadOnlyList<IBlocker> Blockers => _blockers.AsReadOnly();

        public CombatManager(IGameState gameState)
        {
            _gameState = gameState;
        }

        #region Combat Flow

        public void BeginCombat(IPlayer attackingPlayer, IPlayer defendingPlayer)
        {
            IsInCombat = true;
            AttackingPlayer = attackingPlayer;
            DefendingPlayer = defendingPlayer;
            CurrentCombatStep = CombatStep.BeginningOfCombat;

            _attackers.Clear();
            _blockers.Clear();
        }

        public void EndCombat()
        {
            // Clean up combat state on all creatures
            foreach (var attacker in _attackers)
            {
                if (attacker.Creature is Cards.Types.CreatureCard creature)
                {
                    creature.ResetCombatState();
                }
            }

            foreach (var blocker in _blockers)
            {
                if (blocker.Creature is Cards.Types.CreatureCard creature)
                {
                    creature.ResetCombatState();
                }
            }

            _attackers.Clear();
            _blockers.Clear();

            IsInCombat = false;
            AttackingPlayer = null;
            DefendingPlayer = null;
            CurrentCombatStep = CombatStep.NotInCombat;
        }

        #endregion

        #region Declare Attackers

        public bool CanAttack(ICreature creature)
        {
            if (creature.Controller != AttackingPlayer) return false;
            if (creature.IsTapped) return false;
            if (creature.HasSummoningSickness) return false;
            if (creature.HasKeyword(KeywordAbility.Defender)) return false;

            // Check for effects that prevent attacking
            // (In full implementation, would check continuous effects)

            return true;
        }

        public void DeclareAttacker(ICreature creature, IPlayer defendingPlayer)
        {
            if (!CanAttack(creature)) return;

            var attacker = new Attacker(creature, defendingPlayer);
            _attackers.Add(attacker);

            if (creature is Cards.Types.CreatureCard creatureCard)
            {
                creatureCard.IsAttacking = true;
            }

            // Tap attacking creature (unless it has vigilance)
            if (!creature.HasKeyword(KeywordAbility.Vigilance))
            {
                creature.Tap();
            }
        }

        public void DeclareAttacker(ICreature creature, IPlaneswalker targetPlaneswalker)
        {
            if (!CanAttack(creature)) return;

            var attacker = new Attacker(creature, targetPlaneswalker);
            _attackers.Add(attacker);

            if (creature is Cards.Types.CreatureCard creatureCard)
            {
                creatureCard.IsAttacking = true;
            }

            if (!creature.HasKeyword(KeywordAbility.Vigilance))
            {
                creature.Tap();
            }
        }

        public void DeclareAttacker(ICreature creature, IBattle targetBattle)
        {
            if (!CanAttack(creature)) return;

            var attacker = new Attacker(creature, targetBattle);
            _attackers.Add(attacker);

            if (creature is Cards.Types.CreatureCard creatureCard)
            {
                creatureCard.IsAttacking = true;
            }

            if (!creature.HasKeyword(KeywordAbility.Vigilance))
            {
                creature.Tap();
            }
        }

        public void ConfirmAttackers()
        {
            CurrentCombatStep = CombatStep.DeclareAttackers;
            // Triggers "when attacks" abilities would happen here
        }

        #endregion

        #region Declare Blockers

        public bool CanBlock(ICreature blocker, ICreature attacker)
        {
            if (blocker.Controller == AttackingPlayer) return false;
            if (blocker.IsTapped) return false;

            // Check evasion abilities
            if (attacker.HasKeyword(KeywordAbility.Flying) || attacker.HasKeyword(KeywordAbility.Shadow))
            {
                if (!blocker.HasKeyword(KeywordAbility.Flying) &&
                    !blocker.HasKeyword(KeywordAbility.Reach) &&
                    !blocker.HasKeyword(KeywordAbility.Shadow))
                {
                    return false;
                }
            }

            if (attacker.HasKeyword(KeywordAbility.Shadow) && !blocker.HasKeyword(KeywordAbility.Shadow))
            {
                return false;
            }

            if (attacker.HasKeyword(KeywordAbility.Menace))
            {
                // Need at least 2 blockers - handled in ValidateBlockers
            }

            // Check for protection, etc.
            // (In full implementation)

            return true;
        }

        public void DeclareBlocker(ICreature blocker, ICreature attacker)
        {
            if (!CanBlock(blocker, attacker)) return;

            var attackerObj = _attackers.FirstOrDefault(a => a.Creature == attacker);
            if (attackerObj == null) return;

            var blockerObj = new Blocker(blocker, attacker);
            _blockers.Add(blockerObj);

            attackerObj.AddBlocker(blocker);

            if (blocker is Cards.Types.CreatureCard creatureCard)
            {
                creatureCard.IsBlocking = true;
                creatureCard.BlockedCreature = attacker;
            }
        }

        public void ConfirmBlockers()
        {
            // Validate blocking restrictions
            ValidateBlockers();

            CurrentCombatStep = CombatStep.DeclareBlockers;

            // Mark which attackers are blocked
            foreach (var attacker in _attackers)
            {
                // If has menace and fewer than 2 blockers, blockers are invalid
                if (attacker.Creature.HasKeyword(KeywordAbility.Menace) && attacker.Blockers.Count == 1)
                {
                    // Remove the blocker (can't block alone)
                    var blocker = attacker.Blockers.First();
                    ((Attacker)attacker).RemoveBlocker(blocker);
                    if (blocker is Cards.Types.CreatureCard creatureCard)
                    {
                        creatureCard.IsBlocking = false;
                        creatureCard.BlockedCreature = null;
                    }
                }
            }
        }

        private void ValidateBlockers()
        {
            // Implementation for various blocking restrictions
        }

        #endregion

        #region Damage Assignment

        public void AssignDamageOrder(ICreature attacker, IReadOnlyList<ICreature> blockerOrder)
        {
            var attackerObj = _attackers.FirstOrDefault(a => a.Creature == attacker);
            attackerObj?.SetDamageAssignmentOrder(blockerOrder);
        }

        public void AssignCombatDamage()
        {
            if (_attackers.Any(a => a.Creature.HasKeyword(KeywordAbility.FirstStrike) ||
                                    a.Creature.HasKeyword(KeywordAbility.DoubleStrike)) ||
                _blockers.Any(b => b.Creature.HasKeyword(KeywordAbility.FirstStrike) ||
                                   b.Creature.HasKeyword(KeywordAbility.DoubleStrike)))
            {
                CurrentCombatStep = CombatStep.FirstStrikeDamage;
            }
            else
            {
                CurrentCombatStep = CombatStep.CombatDamage;
            }
        }

        public void ApplyFirstStrikeDamage()
        {
            CurrentCombatStep = CombatStep.FirstStrikeDamage;

            foreach (var attacker in _attackers)
            {
                if (!attacker.Creature.HasKeyword(KeywordAbility.FirstStrike) &&
                    !attacker.Creature.HasKeyword(KeywordAbility.DoubleStrike))
                    continue;

                ApplyAttackerDamage(attacker);
            }

            foreach (var blocker in _blockers)
            {
                if (!blocker.Creature.HasKeyword(KeywordAbility.FirstStrike) &&
                    !blocker.Creature.HasKeyword(KeywordAbility.DoubleStrike))
                    continue;

                ApplyBlockerDamage(blocker);
            }
        }

        public void ApplyRegularCombatDamage()
        {
            CurrentCombatStep = CombatStep.CombatDamage;

            foreach (var attacker in _attackers)
            {
                // Skip first strike creatures unless they have double strike
                if (attacker.Creature.HasKeyword(KeywordAbility.FirstStrike) &&
                    !attacker.Creature.HasKeyword(KeywordAbility.DoubleStrike))
                    continue;

                ApplyAttackerDamage(attacker);
            }

            foreach (var blocker in _blockers)
            {
                if (blocker.Creature.HasKeyword(KeywordAbility.FirstStrike) &&
                    !blocker.Creature.HasKeyword(KeywordAbility.DoubleStrike))
                    continue;

                ApplyBlockerDamage(blocker);
            }
        }

        private void ApplyAttackerDamage(IAttacker attacker)
        {
            int damage = attacker.CalculateCombatDamage();

            if (attacker.IsBlocked)
            {
                // Damage goes to blockers in order
                foreach (var blocker in attacker.DamageAssignmentOrder)
                {
                    if (damage <= 0) break;

                    int damageToBlocker = Math.Min(damage, blocker.Toughness - blocker.DamageMarked);

                    // Deathtouch kills with any damage
                    if (attacker.Creature.HasKeyword(KeywordAbility.Deathtouch))
                    {
                        damageToBlocker = 1;
                    }

                    blocker.MarkDamage(damageToBlocker);
                    damage -= damageToBlocker;

                    // Lifelink
                    if (attacker.Creature.HasKeyword(KeywordAbility.Lifelink))
                    {
                        attacker.Creature.Controller.GainLife(damageToBlocker);
                    }
                }

                // Trample - excess damage goes to defending player
                if (damage > 0 && attacker.Creature.HasKeyword(KeywordAbility.Trample))
                {
                    attacker.DefendingPlayer?.DealDamage(damage, attacker.Creature as ICard ?? throw new InvalidOperationException(), true);
                }
            }
            else
            {
                // Unblocked - damage goes to defending player/planeswalker
                if (attacker.DefendingPlayer != null)
                {
                    attacker.DefendingPlayer.DealDamage(damage, attacker.Creature as ICard ?? throw new InvalidOperationException(), true);
                }
                else if (attacker.TargetPlaneswalker != null)
                {
                    attacker.TargetPlaneswalker.DealDamage(damage);
                }
                else if (attacker.TargetBattle != null)
                {
                    attacker.TargetBattle.DealDamage(damage);
                }

                // Lifelink
                if (attacker.Creature.HasKeyword(KeywordAbility.Lifelink))
                {
                    attacker.Creature.Controller.GainLife(damage);
                }
            }
        }

        private void ApplyBlockerDamage(IBlocker blocker)
        {
            int damage = blocker.CalculateCombatDamage();
            var attacker = blocker.BlockedAttacker;

            attacker.MarkDamage(damage);

            // Lifelink
            if (blocker.Creature.HasKeyword(KeywordAbility.Lifelink))
            {
                blocker.Creature.Controller.GainLife(damage);
            }
        }

        #endregion

        #region Queries

        public IReadOnlyList<ICreature> GetBlockersFor(ICreature attacker)
        {
            var attackerObj = _attackers.FirstOrDefault(a => a.Creature == attacker);
            return attackerObj?.Blockers ?? new List<ICreature>().AsReadOnly();
        }

        public ICreature? GetBlockedCreatureFor(ICreature blocker)
        {
            var blockerObj = _blockers.FirstOrDefault(b => b.Creature == blocker);
            return blockerObj?.BlockedAttacker;
        }

        public bool IsAttacking(ICreature creature)
        {
            return _attackers.Any(a => a.Creature == creature);
        }

        public bool IsBlocking(ICreature creature)
        {
            return _blockers.Any(b => b.Creature == creature);
        }

        public bool IsBlocked(ICreature attacker)
        {
            var attackerObj = _attackers.FirstOrDefault(a => a.Creature == attacker);
            return attackerObj?.IsBlocked ?? false;
        }

        public bool IsUnblocked(ICreature attacker)
        {
            return !IsBlocked(attacker);
        }

        #endregion
    }
}
