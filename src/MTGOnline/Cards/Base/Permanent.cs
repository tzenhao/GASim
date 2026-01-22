using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;

namespace MTGOnline.Cards.Base
{
    /// <summary>
    /// Base class for all permanents (cards that stay on the battlefield).
    /// </summary>
    public abstract class Permanent : Card, IPermanent
    {
        public bool IsTapped { get; protected set; }
        public IPlayer Controller { get; set; } = null!;
        public IPlayer Owner { get; set; } = null!;

        private readonly Dictionary<CounterType, int> _counters = new();
        public IReadOnlyDictionary<CounterType, int> Counters => _counters;

        private readonly List<IAura> _attachedAuras = new();
        public IReadOnlyList<IAura> AttachedAuras => _attachedAuras.AsReadOnly();

        private readonly List<IEquipment> _attachedEquipment = new();
        public IReadOnlyList<IEquipment> AttachedEquipment => _attachedEquipment.AsReadOnly();

        public DateTime EnteredBattlefieldTimestamp { get; set; }
        public virtual bool HasSummoningSickness => false;

        protected Permanent(string name) : base(name)
        {
        }

        public virtual void Tap()
        {
            IsTapped = true;
        }

        public virtual void Untap()
        {
            IsTapped = false;
        }

        public void AddCounter(CounterType type, int count = 1)
        {
            if (count <= 0) return;

            if (_counters.ContainsKey(type))
            {
                _counters[type] += count;
            }
            else
            {
                _counters[type] = count;
            }
        }

        public void RemoveCounter(CounterType type, int count = 1)
        {
            if (count <= 0) return;

            if (_counters.ContainsKey(type))
            {
                _counters[type] -= count;
                if (_counters[type] <= 0)
                {
                    _counters.Remove(type);
                }
            }
        }

        public int GetCounterCount(CounterType type)
        {
            return _counters.TryGetValue(type, out int count) ? count : 0;
        }

        public void AttachAura(IAura aura)
        {
            if (!_attachedAuras.Contains(aura))
            {
                _attachedAuras.Add(aura);
            }
        }

        public void DetachAura(IAura aura)
        {
            _attachedAuras.Remove(aura);
        }

        public void AttachEquipment(IEquipment equipment)
        {
            if (!_attachedEquipment.Contains(equipment))
            {
                _attachedEquipment.Add(equipment);
            }
        }

        public void DetachEquipment(IEquipment equipment)
        {
            _attachedEquipment.Remove(equipment);
        }

        public virtual void OnEnterBattlefield(IGameState gameState)
        {
            EnteredBattlefieldTimestamp = DateTime.UtcNow;
        }

        public virtual void OnLeaveBattlefield(IGameState gameState)
        {
        }
    }
}
