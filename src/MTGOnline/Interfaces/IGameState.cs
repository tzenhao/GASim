using MTGOnline.Core.Enums;

namespace MTGOnline.Interfaces
{
    /// <summary>
    /// Interface representing the current state of the game.
    /// </summary>
    public interface IGameState
    {
        Guid GameId { get; }
        GameStatus Status { get; }
        IReadOnlyList<IPlayer> Players { get; }
        IPlayer ActivePlayer { get; }
        IPlayer? PriorityPlayer { get; }
        IBattlefield Battlefield { get; }
        IStack Stack { get; }
        ITurnManager TurnManager { get; }
        ICombatManager CombatManager { get; }
        IEffectManager EffectManager { get; }
        IEventManager EventManager { get; }

        // Turn information
        int TurnNumber { get; }
        PhaseType CurrentPhase { get; }
        StepType CurrentStep { get; }

        // State-based actions
        void CheckStateBasedActions();

        // Priority
        void PassPriority();
        void GivePriority(IPlayer player);

        // Game flow
        void StartGame();
        void EndGame(IPlayer winner, WinLossCondition reason);
        void Draw();

        // Utility
        IPlayer GetOpponent(IPlayer player);
        IReadOnlyList<IPlayer> GetOpponents(IPlayer player);
        ICard? FindCard(Guid cardId);
        IPermanent? FindPermanent(Guid permanentId);
    }

    /// <summary>
    /// Interface for managing turns.
    /// </summary>
    public interface ITurnManager
    {
        int TurnNumber { get; }
        IPlayer ActivePlayer { get; }
        PhaseType CurrentPhase { get; }
        StepType CurrentStep { get; }
        bool IsMainPhase { get; }
        bool IsCombatPhase { get; }

        void StartTurn(IPlayer player);
        void EndTurn();
        void MoveToNextPhase();
        void MoveToNextStep();
        void AddExtraTurn(IPlayer player);
        void SkipNextTurn(IPlayer player);
    }

    /// <summary>
    /// Interface for managing effects.
    /// </summary>
    public interface IEffectManager
    {
        IReadOnlyList<IContinuousEffect> ActiveEffects { get; }

        void AddEffect(IContinuousEffect effect);
        void RemoveEffect(IContinuousEffect effect);
        void ApplyLayeredEffects(IGameState gameState);
        void CleanupExpiredEffects();

        // Replacement effects
        IGameEvent ApplyReplacementEffects(IGameEvent gameEvent, IGameState gameState);

        // Prevention effects
        int ApplyPreventionEffects(IDamageEvent damageEvent, int damage, IGameState gameState);
    }

    /// <summary>
    /// Interface for managing game events.
    /// </summary>
    public interface IEventManager
    {
        void RaiseEvent(IGameEvent gameEvent);
        void Subscribe<T>(Action<T> handler) where T : IGameEvent;
        void Unsubscribe<T>(Action<T> handler) where T : IGameEvent;
    }
}
