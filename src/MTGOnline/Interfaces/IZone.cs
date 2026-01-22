using MTGOnline.Core.Enums;

namespace MTGOnline.Interfaces
{
    /// <summary>
    /// Interface for game zones.
    /// </summary>
    public interface IZone
    {
        ZoneType ZoneType { get; }
        ZoneVisibility Visibility { get; }
        IPlayer? Owner { get; }
        int Count { get; }
        bool IsEmpty { get; }
        IReadOnlyList<ICard> Cards { get; }

        void Add(ICard card);
        void AddToTop(ICard card);
        void AddToBottom(ICard card);
        void AddAt(ICard card, int index);
        bool Remove(ICard card);
        ICard? RemoveFromTop();
        ICard? RemoveFromBottom();
        ICard? RemoveAt(int index);
        bool Contains(ICard card);
        void Clear();
        void Shuffle();
        ICard? Peek();
        IReadOnlyList<ICard> PeekTop(int count);
        IReadOnlyList<ICard> GetAll(Func<ICard, bool> predicate);
    }

    /// <summary>
    /// Interface for the battlefield zone (shared between players).
    /// </summary>
    public interface IBattlefield : IZone
    {
        IReadOnlyList<IPermanent> Permanents { get; }
        IReadOnlyList<ICreature> Creatures { get; }
        IReadOnlyList<ILand> Lands { get; }
        IReadOnlyList<IArtifact> Artifacts { get; }
        IReadOnlyList<IEnchantment> Enchantments { get; }
        IReadOnlyList<IPlaneswalker> Planeswalkers { get; }

        IReadOnlyList<IPermanent> GetPermanentsControlledBy(IPlayer player);
        IReadOnlyList<ICreature> GetCreaturesControlledBy(IPlayer player);
        IReadOnlyList<ILand> GetLandsControlledBy(IPlayer player);
        IReadOnlyList<IPermanent> GetPermanentsOwnedBy(IPlayer player);
    }

    /// <summary>
    /// Interface for the stack.
    /// </summary>
    public interface IStack : IZone
    {
        IReadOnlyList<IStackObject> StackObjects { get; }

        void Push(IStackObject stackObject);
        IStackObject? Pop();
        IStackObject? PeekTop();
        bool IsEmpty { get; }
    }
}
