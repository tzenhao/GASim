using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;
using MTGOnline.Zones;

namespace MTGOnline.Game.State
{
    /// <summary>
    /// Represents the stack zone where spells and abilities wait to resolve.
    /// </summary>
    public class GameStack : Zone, IStack
    {
        private readonly List<IStackObject> _stackObjects = new();
        public IReadOnlyList<IStackObject> StackObjects => _stackObjects.AsReadOnly();

        public new bool IsEmpty => _stackObjects.Count == 0;

        public GameStack() : base(ZoneType.Stack, ZoneVisibility.Public, null)
        {
        }

        public void Push(IStackObject stackObject)
        {
            _stackObjects.Insert(0, stackObject);
            if (stackObject.Source != null)
            {
                _cards.Insert(0, stackObject.Source);
            }
        }

        public IStackObject? Pop()
        {
            if (_stackObjects.Count == 0) return null;

            var stackObject = _stackObjects[0];
            _stackObjects.RemoveAt(0);

            if (stackObject.Source != null)
            {
                _cards.Remove(stackObject.Source);
            }

            return stackObject;
        }

        public IStackObject? PeekTop()
        {
            return _stackObjects.Count > 0 ? _stackObjects[0] : null;
        }

        /// <summary>
        /// Removes a specific stack object (e.g., when countered).
        /// </summary>
        public bool Remove(IStackObject stackObject)
        {
            if (stackObject.Source != null)
            {
                _cards.Remove(stackObject.Source);
            }
            return _stackObjects.Remove(stackObject);
        }

        /// <summary>
        /// Gets all spells on the stack.
        /// </summary>
        public IReadOnlyList<IStackObject> GetSpells()
        {
            return _stackObjects.Where(so => so.IsSpell).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets all abilities on the stack.
        /// </summary>
        public IReadOnlyList<IStackObject> GetAbilities()
        {
            return _stackObjects.Where(so => so.IsAbility).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets all stack objects controlled by a player.
        /// </summary>
        public IReadOnlyList<IStackObject> GetControlledBy(IPlayer player)
        {
            return _stackObjects.Where(so => so.Controller == player).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the number of items on the stack.
        /// </summary>
        public int StackSize => _stackObjects.Count;
    }
}
