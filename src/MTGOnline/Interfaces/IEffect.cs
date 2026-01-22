using MTGOnline.Core.Enums;

namespace MTGOnline.Interfaces
{
    /// <summary>
    /// Base interface for all effects.
    /// </summary>
    public interface IEffect
    {
        Guid Id { get; }
        ICard Source { get; }
        IPlayer Controller { get; }
        string Description { get; }

        void Apply(IGameState gameState);
    }

    /// <summary>
    /// Interface for one-shot effects.
    /// </summary>
    public interface IOneShotEffect : IEffect
    {
        IReadOnlyList<ITarget> Targets { get; }
    }

    /// <summary>
    /// Interface for continuous effects.
    /// </summary>
    public interface IContinuousEffect : IEffect
    {
        EffectLayer Layer { get; }
        DateTime Timestamp { get; }
        IDuration Duration { get; }
        bool IsExpired { get; }

        void Remove(IGameState gameState);
        bool AppliesTo(IPermanent permanent, IGameState gameState);
    }

    /// <summary>
    /// Interface for replacement effects.
    /// </summary>
    public interface IReplacementEffect : IEffect
    {
        bool CanReplace(IGameEvent gameEvent, IGameState gameState);
        IGameEvent Replace(IGameEvent gameEvent, IGameState gameState);
    }

    /// <summary>
    /// Interface for prevention effects.
    /// </summary>
    public interface IPreventionEffect : IEffect
    {
        bool CanPrevent(IDamageEvent damageEvent, IGameState gameState);
        int PreventDamage(IDamageEvent damageEvent, int amount, IGameState gameState);
    }

    /// <summary>
    /// Layers for applying continuous effects (as per MTG rules).
    /// </summary>
    public enum EffectLayer
    {
        Layer1_CopyEffects,
        Layer2_ControlChangingEffects,
        Layer3_TextChangingEffects,
        Layer4_TypeChangingEffects,
        Layer5_ColorChangingEffects,
        Layer6_AbilityAddingRemovingEffects,
        Layer7a_CharacteristicDefiningAbilities,
        Layer7b_SetPowerToughness,
        Layer7c_ModifyPowerToughness,
        Layer7d_SwitchPowerToughness
    }

    /// <summary>
    /// Interface for effect duration.
    /// </summary>
    public interface IDuration
    {
        bool HasExpired(IGameState gameState);
        string GetDescription();
    }
}
