using MTGOnline.Core.Enums;
using MTGOnline.Cards.Base;
using MTGOnline.Interfaces;

namespace MTGOnline.Cards.Types
{
    /// <summary>
    /// Represents a creature card in Magic: The Gathering.
    /// </summary>
    public class CreatureCard : Permanent, ICreature
    {
        public int BasePower { get; set; }
        public int BaseToughness { get; set; }

        // Modified power/toughness (after effects)
        private int? _modifiedPower;
        private int? _modifiedToughness;

        public int Power
        {
            get => _modifiedPower ?? BasePower + GetCounterCount(CounterType.PlusOnePlusOne) - GetCounterCount(CounterType.MinusOneMinusOne);
            set => _modifiedPower = value;
        }

        public int Toughness
        {
            get => _modifiedToughness ?? BaseToughness + GetCounterCount(CounterType.PlusOnePlusOne) - GetCounterCount(CounterType.MinusOneMinusOne);
            set => _modifiedToughness = value;
        }

        public int DamageMarked { get; private set; }

        public bool IsAttacking { get; set; }
        public bool IsBlocking { get; set; }
        public ICreature? BlockedCreature { get; set; }

        private readonly List<ICreature> _blockingCreatures = new();
        public IReadOnlyList<ICreature> BlockingCreatures => _blockingCreatures.AsReadOnly();

        public KeywordAbility Keywords { get; private set; }

        private DateTime _enteredBattlefieldTime;
        private bool _hasHaste;

        public override bool HasSummoningSickness
        {
            get
            {
                if (HasKeyword(KeywordAbility.Haste)) return false;
                // Simplified: creature has summoning sickness if it entered this turn
                // In full implementation, would check against turn system
                return (DateTime.UtcNow - _enteredBattlefieldTime).TotalSeconds < 1;
            }
        }

        public CreatureCard(string name, int power, int toughness) : base(name)
        {
            CardTypes = CardType.Creature;
            BasePower = power;
            BaseToughness = toughness;
        }

        public void MarkDamage(int amount)
        {
            if (amount > 0)
            {
                DamageMarked += amount;
            }
        }

        public void ClearDamage()
        {
            DamageMarked = 0;
        }

        public bool HasKeyword(KeywordAbility keyword)
        {
            return (Keywords & keyword) != 0;
        }

        public void AddKeyword(KeywordAbility keyword)
        {
            Keywords |= keyword;
        }

        public void RemoveKeyword(KeywordAbility keyword)
        {
            Keywords &= ~keyword;
        }

        public bool HasLethalDamage => DamageMarked >= Toughness;
        public bool IsDeadByToughness => Toughness <= 0;

        public void AddBlockingCreature(ICreature blocker)
        {
            if (!_blockingCreatures.Contains(blocker))
            {
                _blockingCreatures.Add(blocker);
            }
        }

        public void RemoveBlockingCreature(ICreature blocker)
        {
            _blockingCreatures.Remove(blocker);
        }

        public void ClearBlockingCreatures()
        {
            _blockingCreatures.Clear();
        }

        public void ResetCombatState()
        {
            IsAttacking = false;
            IsBlocking = false;
            BlockedCreature = null;
            _blockingCreatures.Clear();
        }

        public override void OnEnterBattlefield(IGameState gameState)
        {
            base.OnEnterBattlefield(gameState);
            _enteredBattlefieldTime = DateTime.UtcNow;
        }

        public void ResetModifications()
        {
            _modifiedPower = null;
            _modifiedToughness = null;
        }

        public int CalculateCombatDamage()
        {
            return Power;
        }

        public override string ToString()
        {
            return $"{Name} ({Power}/{Toughness})";
        }
    }
}
