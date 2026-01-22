using MTGOnline.Core.Enums;

namespace MTGOnline.Interfaces
{
    /// <summary>
    /// Interface for targeting.
    /// </summary>
    public interface ITarget
    {
        Guid TargetId { get; }
        TargetType TargetType { get; }
        bool IsLegalTarget(IGameState gameState);

        // Target can be a player, permanent, card in a zone, or spell/ability on the stack
        IPlayer? AsPlayer();
        IPermanent? AsPermanent();
        ICard? AsCard();
        IStackObject? AsStackObject();
    }

    /// <summary>
    /// Interface for target requirements.
    /// </summary>
    public interface ITargetRequirement
    {
        TargetType AllowedTargetTypes { get; }
        TargetController TargetController { get; }
        int MinimumTargets { get; }
        int MaximumTargets { get; }
        string Description { get; }

        bool IsValidTarget(ITarget target, IGameState gameState);
        IReadOnlyList<ITarget> GetValidTargets(IGameState gameState, IPlayer choosingPlayer);
    }

    /// <summary>
    /// Interface for objects that can be on the stack.
    /// </summary>
    public interface IStackObject
    {
        Guid Id { get; }
        IPlayer Controller { get; }
        ICard Source { get; }
        IReadOnlyList<ITarget> Targets { get; }
        bool IsSpell { get; }
        bool IsAbility { get; }

        bool CanResolve(IGameState gameState);
        void Resolve(IGameState gameState);
        void Counter(IGameState gameState);

        // For checking if a target is still legal
        bool HasLegalTargets(IGameState gameState);
    }
}
