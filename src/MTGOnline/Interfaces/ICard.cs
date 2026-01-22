using MTGOnline.Core.Enums;
using MTGOnline.Mana;
using MTGOnline.Abilities.Base;

namespace MTGOnline.Interfaces
{
    /// <summary>
    /// Base interface for all cards.
    /// </summary>
    public interface ICard
    {
        Guid Id { get; }
        string Name { get; }
        ManaCost ManaCost { get; }
        int ConvertedManaCost { get; }
        CardType CardTypes { get; }
        SuperType SuperTypes { get; }
        IReadOnlyList<string> SubTypes { get; }
        string RulesText { get; }
        ManaColor ColorIdentity { get; }
        IReadOnlyList<IAbility> Abilities { get; }

        bool HasCardType(CardType type);
        bool HasSuperType(SuperType type);
        bool HasSubType(string subType);
    }

    /// <summary>
    /// Interface for permanents (cards that stay on the battlefield).
    /// </summary>
    public interface IPermanent : ICard
    {
        bool IsTapped { get; }
        IPlayer Controller { get; }
        IPlayer Owner { get; }
        IReadOnlyDictionary<CounterType, int> Counters { get; }
        IReadOnlyList<IAura> AttachedAuras { get; }
        IReadOnlyList<IEquipment> AttachedEquipment { get; }
        DateTime EnteredBattlefieldTimestamp { get; }
        bool HasSummoningSickness { get; }

        void Tap();
        void Untap();
        void AddCounter(CounterType type, int count = 1);
        void RemoveCounter(CounterType type, int count = 1);
        int GetCounterCount(CounterType type);
    }

    /// <summary>
    /// Interface for creatures.
    /// </summary>
    public interface ICreature : IPermanent
    {
        int BasePower { get; }
        int BaseToughness { get; }
        int Power { get; }
        int Toughness { get; }
        int DamageMarked { get; }
        bool IsAttacking { get; }
        bool IsBlocking { get; }
        ICreature? BlockedCreature { get; }
        IReadOnlyList<ICreature> BlockingCreatures { get; }
        KeywordAbility Keywords { get; }

        void MarkDamage(int amount);
        void ClearDamage();
        bool HasKeyword(KeywordAbility keyword);
        void AddKeyword(KeywordAbility keyword);
        void RemoveKeyword(KeywordAbility keyword);
    }

    /// <summary>
    /// Interface for planeswalkers.
    /// </summary>
    public interface IPlaneswalker : IPermanent
    {
        int StartingLoyalty { get; }
        int CurrentLoyalty { get; }
        IReadOnlyList<ILoyaltyAbility> LoyaltyAbilities { get; }
        bool HasActivatedLoyaltyThisTurn { get; }

        void AddLoyalty(int amount);
        void RemoveLoyalty(int amount);
        void DealDamage(int amount);
    }

    /// <summary>
    /// Interface for lands.
    /// </summary>
    public interface ILand : IPermanent
    {
        IReadOnlyList<IManaAbility> ManaAbilities { get; }
        bool IsBasicLand { get; }
    }

    /// <summary>
    /// Interface for artifacts.
    /// </summary>
    public interface IArtifact : IPermanent
    {
    }

    /// <summary>
    /// Interface for equipment (a subtype of artifact).
    /// </summary>
    public interface IEquipment : IArtifact
    {
        ICreature? EquippedCreature { get; }
        ManaCost EquipCost { get; }

        void AttachTo(ICreature creature);
        void Detach();
    }

    /// <summary>
    /// Interface for enchantments.
    /// </summary>
    public interface IEnchantment : IPermanent
    {
    }

    /// <summary>
    /// Interface for auras (enchantments attached to permanents/players).
    /// </summary>
    public interface IAura : IEnchantment
    {
        IPermanent? EnchantedPermanent { get; }
        IPlayer? EnchantedPlayer { get; }

        void AttachTo(IPermanent permanent);
        void AttachTo(IPlayer player);
        void Detach();
    }

    /// <summary>
    /// Interface for spells (instants and sorceries).
    /// </summary>
    public interface ISpell : ICard
    {
        IPlayer Controller { get; }
        IReadOnlyList<ITarget> Targets { get; }
        bool IsOnStack { get; }

        void Resolve();
        void Counter();
    }
}
