using MTGOnline.Core.Enums;
using MTGOnline.Mana;

namespace MTGOnline.Interfaces
{
    /// <summary>
    /// Base interface for all abilities.
    /// </summary>
    public interface IAbility
    {
        Guid Id { get; }
        AbilityType AbilityType { get; }
        ICard Source { get; }
        IPlayer Controller { get; }
        string RulesText { get; }

        bool CanActivate(IGameState gameState);
        void Resolve(IGameState gameState);
    }

    /// <summary>
    /// Interface for activated abilities.
    /// </summary>
    public interface IActivatedAbility : IAbility
    {
        ICost ActivationCost { get; }
        TimingRestriction TimingRestriction { get; }
        IReadOnlyList<ITarget> Targets { get; }

        void Activate(IGameState gameState, IReadOnlyList<ITarget> targets);
    }

    /// <summary>
    /// Interface for triggered abilities.
    /// </summary>
    public interface ITriggeredAbility : IAbility
    {
        ITriggerCondition TriggerCondition { get; }
        bool IsOptional { get; }
        IReadOnlyList<ITarget> Targets { get; }

        bool CheckTrigger(IGameEvent gameEvent);
        void Trigger(IGameState gameState);
    }

    /// <summary>
    /// Interface for static abilities.
    /// </summary>
    public interface IStaticAbility : IAbility
    {
        IReadOnlyList<IContinuousEffect> Effects { get; }

        void Apply(IGameState gameState);
        void Remove(IGameState gameState);
    }

    /// <summary>
    /// Interface for mana abilities.
    /// </summary>
    public interface IManaAbility : IActivatedAbility
    {
        ManaOutput ManaProduced { get; }

        // Mana abilities don't use the stack
        new void Activate(IGameState gameState, IReadOnlyList<ITarget> targets);
    }

    /// <summary>
    /// Interface for loyalty abilities (planeswalker abilities).
    /// </summary>
    public interface ILoyaltyAbility : IActivatedAbility
    {
        int LoyaltyCost { get; }  // Positive for +, negative for -
    }

    /// <summary>
    /// Interface for costs.
    /// </summary>
    public interface ICost
    {
        bool CanPay(IPlayer player, IGameState gameState);
        void Pay(IPlayer player, IGameState gameState);
        string GetDescription();
    }

    /// <summary>
    /// Interface for trigger conditions.
    /// </summary>
    public interface ITriggerCondition
    {
        bool Matches(IGameEvent gameEvent, ICard source, IGameState gameState);
    }
}
