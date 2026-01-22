using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;
using MTGOnline.Abilities.Base;

namespace MTGOnline.Abilities.Types
{
    /// <summary>
    /// Represents an activated ability.
    /// </summary>
    public class ActivatedAbility : Ability, IActivatedAbility
    {
        public override AbilityType AbilityType => AbilityType.Activated;

        public ICost ActivationCost { get; }
        public TimingRestriction TimingRestriction { get; set; } = TimingRestriction.Instant;

        private readonly List<ITarget> _targets = new();
        public IReadOnlyList<ITarget> Targets => _targets.AsReadOnly();

        public Action<IGameState, IReadOnlyList<ITarget>>? Effect { get; set; }

        public ActivatedAbility(ICard source, IPlayer controller, ICost activationCost, string rulesText = "")
            : base(source, controller, rulesText)
        {
            ActivationCost = activationCost;
        }

        public override bool CanActivate(IGameState gameState)
        {
            // Check timing restriction
            switch (TimingRestriction)
            {
                case TimingRestriction.Sorcery:
                    if (gameState.ActivePlayer != Controller ||
                        !gameState.Stack.IsEmpty ||
                        (gameState.CurrentPhase != PhaseType.PreCombatMain &&
                         gameState.CurrentPhase != PhaseType.PostCombatMain))
                        return false;
                    break;

                case TimingRestriction.OnlyDuringCombat:
                    if (gameState.CurrentPhase != PhaseType.Combat)
                        return false;
                    break;

                case TimingRestriction.OnlyDuringYourTurn:
                    if (gameState.ActivePlayer != Controller)
                        return false;
                    break;
            }

            // Check if controller has priority
            if (!Controller.HasPriority) return false;

            // Check if cost can be paid
            return ActivationCost.CanPay(Controller, gameState);
        }

        public void Activate(IGameState gameState, IReadOnlyList<ITarget> targets)
        {
            if (!CanActivate(gameState)) return;

            // Pay the cost
            ActivationCost.Pay(Controller, gameState);

            // Add targets
            _targets.Clear();
            _targets.AddRange(targets);

            // Put ability on stack (unless it's a mana ability)
            var stackObject = new AbilityStackObject(this, Controller, targets);
            gameState.Stack.Push(stackObject);
        }

        public override void Resolve(IGameState gameState)
        {
            Effect?.Invoke(gameState, _targets);
        }
    }

    /// <summary>
    /// Stack object representing an ability on the stack.
    /// </summary>
    public class AbilityStackObject : IStackObject
    {
        public Guid Id { get; }
        public IPlayer Controller { get; }
        public ICard Source { get; }

        private readonly List<ITarget> _targets;
        public IReadOnlyList<ITarget> Targets => _targets.AsReadOnly();

        public bool IsSpell => false;
        public bool IsAbility => true;

        private readonly IAbility _ability;

        public AbilityStackObject(IAbility ability, IPlayer controller, IReadOnlyList<ITarget> targets)
        {
            Id = Guid.NewGuid();
            _ability = ability;
            Controller = controller;
            Source = ability.Source;
            _targets = targets.ToList();
        }

        public bool CanResolve(IGameState gameState)
        {
            return HasLegalTargets(gameState);
        }

        public void Resolve(IGameState gameState)
        {
            if (CanResolve(gameState))
            {
                _ability.Resolve(gameState);
            }
        }

        public void Counter(IGameState gameState)
        {
            // Ability is removed from stack - no other effect
        }

        public bool HasLegalTargets(IGameState gameState)
        {
            return _targets.All(t => t.IsLegalTarget(gameState));
        }
    }
}
