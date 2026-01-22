using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;
using MTGOnline.Abilities.Base;

namespace MTGOnline.Abilities.Types
{
    /// <summary>
    /// Represents a triggered ability.
    /// </summary>
    public class TriggeredAbility : Ability, ITriggeredAbility
    {
        public override AbilityType AbilityType => AbilityType.Triggered;

        public ITriggerCondition TriggerCondition { get; }
        public bool IsOptional { get; set; }

        private readonly List<ITarget> _targets = new();
        public IReadOnlyList<ITarget> Targets => _targets.AsReadOnly();

        public Action<IGameState>? Effect { get; set; }

        public TriggeredAbility(ICard source, IPlayer controller, ITriggerCondition triggerCondition, string rulesText = "")
            : base(source, controller, rulesText)
        {
            TriggerCondition = triggerCondition;
        }

        public override bool CanActivate(IGameState gameState)
        {
            // Triggered abilities don't need to check activation - they trigger automatically
            return true;
        }

        public bool CheckTrigger(IGameEvent gameEvent)
        {
            return TriggerCondition.Matches(gameEvent, Source, null!);
        }

        public void Trigger(IGameState gameState)
        {
            // Put the triggered ability on the stack
            var stackObject = new AbilityStackObject(this, Controller, _targets);
            gameState.Stack.Push(stackObject);
        }

        public override void Resolve(IGameState gameState)
        {
            Effect?.Invoke(gameState);
        }
    }

    /// <summary>
    /// Common trigger conditions.
    /// </summary>
    public static class TriggerConditions
    {
        public static ITriggerCondition EntersBattlefield(Func<IPermanent, bool>? filter = null)
        {
            return new EntersBattlefieldTrigger(filter);
        }

        public static ITriggerCondition LeavesBattlefield(Func<IPermanent, bool>? filter = null)
        {
            return new LeavesBattlefieldTrigger(filter);
        }

        public static ITriggerCondition Dies(Func<ICreature, bool>? filter = null)
        {
            return new DiesTrigger(filter);
        }

        public static ITriggerCondition DealsCombatDamage()
        {
            return new DealsCombatDamageTrigger();
        }

        public static ITriggerCondition BeginningOfUpkeep()
        {
            return new PhaseStepTrigger(PhaseType.Beginning, StepType.Upkeep);
        }

        public static ITriggerCondition EndOfTurn()
        {
            return new PhaseStepTrigger(PhaseType.Ending, StepType.End);
        }

        public static ITriggerCondition SpellCast(Func<ISpell, bool>? filter = null)
        {
            return new SpellCastTrigger(filter);
        }
    }

    #region Trigger Condition Implementations

    public class EntersBattlefieldTrigger : ITriggerCondition
    {
        private readonly Func<IPermanent, bool>? _filter;

        public EntersBattlefieldTrigger(Func<IPermanent, bool>? filter = null)
        {
            _filter = filter;
        }

        public bool Matches(IGameEvent gameEvent, ICard source, IGameState gameState)
        {
            if (gameEvent is IEntersBattlefieldEvent etbEvent)
            {
                return _filter?.Invoke(etbEvent.Permanent) ?? true;
            }
            return false;
        }
    }

    public class LeavesBattlefieldTrigger : ITriggerCondition
    {
        private readonly Func<IPermanent, bool>? _filter;

        public LeavesBattlefieldTrigger(Func<IPermanent, bool>? filter = null)
        {
            _filter = filter;
        }

        public bool Matches(IGameEvent gameEvent, ICard source, IGameState gameState)
        {
            if (gameEvent is ILeavesBattlefieldEvent ltbEvent)
            {
                return _filter?.Invoke(ltbEvent.Permanent) ?? true;
            }
            return false;
        }
    }

    public class DiesTrigger : ITriggerCondition
    {
        private readonly Func<ICreature, bool>? _filter;

        public DiesTrigger(Func<ICreature, bool>? filter = null)
        {
            _filter = filter;
        }

        public bool Matches(IGameEvent gameEvent, ICard source, IGameState gameState)
        {
            if (gameEvent is IDiesEvent diesEvent)
            {
                return _filter?.Invoke(diesEvent.Creature) ?? true;
            }
            return false;
        }
    }

    public class DealsCombatDamageTrigger : ITriggerCondition
    {
        public bool Matches(IGameEvent gameEvent, ICard source, IGameState gameState)
        {
            if (gameEvent is IDamageEvent damageEvent)
            {
                return damageEvent.IsCombatDamage && damageEvent.Source == source;
            }
            return false;
        }
    }

    public class PhaseStepTrigger : ITriggerCondition
    {
        private readonly PhaseType _phase;
        private readonly StepType _step;

        public PhaseStepTrigger(PhaseType phase, StepType step)
        {
            _phase = phase;
            _step = step;
        }

        public bool Matches(IGameEvent gameEvent, ICard source, IGameState gameState)
        {
            if (gameEvent is IPhaseStepEvent phaseEvent)
            {
                return phaseEvent.Phase == _phase && phaseEvent.Step == _step;
            }
            return false;
        }
    }

    public class SpellCastTrigger : ITriggerCondition
    {
        private readonly Func<ISpell, bool>? _filter;

        public SpellCastTrigger(Func<ISpell, bool>? filter = null)
        {
            _filter = filter;
        }

        public bool Matches(IGameEvent gameEvent, ICard source, IGameState gameState)
        {
            if (gameEvent is ICastSpellEvent castEvent)
            {
                return _filter?.Invoke(castEvent.Spell) ?? true;
            }
            return false;
        }
    }

    #endregion
}
