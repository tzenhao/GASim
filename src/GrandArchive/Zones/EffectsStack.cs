using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Zones
{
    /// <summary>
    /// Effects Stack zone in Grand Archive TCG.
    /// Where activated cards and abilities resolve.
    /// Shared public zone, LIFO (Last In, First Out) order.
    /// </summary>
    public class EffectsStack : Zone, IEffectsStack
    {
        private readonly List<IStackObject> _stackObjects = new();

        public override ZoneType ZoneType => ZoneType.EffectsStack;

        public override bool IsPublic => true;

        /// <summary>
        /// Stack order is fixed (LIFO).
        /// </summary>
        public override bool IsOrdered => true;

        public IReadOnlyList<IStackObject> Objects => _stackObjects.AsReadOnly();

        public bool IsEmpty => _stackObjects.Count == 0;

        public IStackObject? Top => _stackObjects.Count > 0 ? _stackObjects[^1] : null;

        /// <summary>
        /// Push a stack object onto the stack.
        /// </summary>
        public void Push(IStackObject stackObject)
        {
            _stackObjects.Add(stackObject);
        }

        /// <summary>
        /// Resolve the top object on the stack.
        /// </summary>
        public void ResolveTop()
        {
            if (_stackObjects.Count > 0)
            {
                var top = _stackObjects[^1];
                _stackObjects.RemoveAt(_stackObjects.Count - 1);
                // Resolution is handled by the caller
            }
        }

        /// <summary>
        /// Remove an object from the stack (e.g., countered).
        /// </summary>
        public bool Remove(IStackObject stackObject)
        {
            return _stackObjects.Remove(stackObject);
        }

        /// <summary>
        /// Get all stack objects controlled by a player.
        /// </summary>
        public IEnumerable<IStackObject> GetObjectsControlledBy(IPlayer player)
        {
            return _stackObjects.Where(o => o.Controller == player);
        }

        /// <summary>
        /// Check if a specific card activation is on the stack.
        /// </summary>
        public bool ContainsCardActivation(ICard card)
        {
            return _stackObjects.Any(o => o.Source == card);
        }

        /// <summary>
        /// Clear the entire stack (rare, for game reset).
        /// </summary>
        public void ClearStack()
        {
            _stackObjects.Clear();
        }

        public override string ToString() => $"Effects Stack ({_stackObjects.Count} objects)";
    }

    /// <summary>
    /// Represents a card activation on the effects stack.
    /// </summary>
    public class CardActivation : IStackObject
    {
        public Guid Id { get; } = Guid.NewGuid();

        public object Source { get; }

        public IPlayer Controller { get; }

        public IReadOnlyList<ITarget> Targets { get; }

        public ICard Card => (ICard)Source;

        public CardActivation(ICard card, IPlayer controller, IReadOnlyList<ITarget>? targets = null)
        {
            Source = card;
            Controller = controller;
            Targets = targets ?? Array.Empty<ITarget>().AsReadOnly();
        }

        public void Resolve(IGameState gameState)
        {
            // Resolution logic depends on card type
            // This would be implemented based on the specific card
        }

        public override string ToString() => $"Card Activation: {Card.Name}";
    }

    /// <summary>
    /// Represents an ability activation on the effects stack.
    /// </summary>
    public class AbilityActivation : IStackObject
    {
        public Guid Id { get; } = Guid.NewGuid();

        public object Source { get; }

        public IPlayer Controller { get; }

        public IReadOnlyList<ITarget> Targets { get; }

        public IAbility Ability => (IAbility)Source;

        public AbilityActivation(IAbility ability, IPlayer controller, IReadOnlyList<ITarget>? targets = null)
        {
            Source = ability;
            Controller = controller;
            Targets = targets ?? Array.Empty<ITarget>().AsReadOnly();
        }

        public void Resolve(IGameState gameState)
        {
            Ability.Resolve(gameState);
        }

        public override string ToString() => $"Ability Activation: {Ability.Name}";
    }
}
