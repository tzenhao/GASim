using GrandArchive.Core.Enums;

namespace GrandArchive.Interfaces
{
    /// <summary>
    /// Interface for the game state in Grand Archive TCG.
    /// </summary>
    public interface IGameState
    {
        /// <summary>All players in the game</summary>
        IReadOnlyList<IPlayer> Players { get; }

        /// <summary>The current turn player</summary>
        IPlayer TurnPlayer { get; }

        /// <summary>The current phase</summary>
        PhaseType CurrentPhase { get; }

        /// <summary>The current combat step (if in combat)</summary>
        CombatStep? CurrentCombatStep { get; }

        /// <summary>The current turn number</summary>
        int TurnNumber { get; }

        /// <summary>The shared field zone</summary>
        IZone Field { get; }

        /// <summary>The effects stack</summary>
        IEffectsStack EffectsStack { get; }

        /// <summary>Whether the game has ended</summary>
        bool IsGameOver { get; }

        /// <summary>The winning player (if game is over)</summary>
        IPlayer? Winner { get; }

        /// <summary>Get the opponent of a player (2-player game)</summary>
        IPlayer GetOpponent(IPlayer player);

        /// <summary>Check if a player can currently act</summary>
        bool CanPlayerAct(IPlayer player);

        /// <summary>Pass opportunity to the next player</summary>
        void PassOpportunity();

        /// <summary>Move to the next phase</summary>
        void AdvancePhase();

        /// <summary>Start a new turn</summary>
        void StartNewTurn();

        /// <summary>End the game with a winner</summary>
        void EndGame(IPlayer winner);
    }

    /// <summary>
    /// Interface for the effects stack.
    /// </summary>
    public interface IEffectsStack
    {
        /// <summary>All objects currently on the stack</summary>
        IReadOnlyList<IStackObject> Objects { get; }

        /// <summary>Whether the stack is empty</summary>
        bool IsEmpty { get; }

        /// <summary>The top object on the stack</summary>
        IStackObject? Top { get; }

        /// <summary>Push an object onto the stack</summary>
        void Push(IStackObject stackObject);

        /// <summary>Resolve the top object on the stack</summary>
        void ResolveTop();

        /// <summary>Remove an object from the stack (e.g., countered)</summary>
        bool Remove(IStackObject stackObject);
    }

    /// <summary>
    /// Interface for objects on the effects stack.
    /// </summary>
    public interface IStackObject
    {
        /// <summary>Unique identifier</summary>
        Guid Id { get; }

        /// <summary>The source card or ability</summary>
        object Source { get; }

        /// <summary>The controller of this stack object</summary>
        IPlayer Controller { get; }

        /// <summary>Targets for this stack object</summary>
        IReadOnlyList<ITarget> Targets { get; }

        /// <summary>Resolve this stack object</summary>
        void Resolve(IGameState gameState);
    }

    /// <summary>
    /// Interface for targets.
    /// </summary>
    public interface ITarget
    {
        /// <summary>The targeted object</summary>
        object Target { get; }

        /// <summary>Whether this target is still valid</summary>
        bool IsValid { get; }
    }
}
