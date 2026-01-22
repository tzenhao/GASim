using MTGOnline.Interfaces;

namespace MTGOnline.Abilities.Effects
{
    /// <summary>
    /// Base class for continuous effects.
    /// </summary>
    public abstract class ContinuousEffect : IContinuousEffect
    {
        public Guid Id { get; }
        public ICard Source { get; }
        public IPlayer Controller { get; }
        public abstract string Description { get; }
        public abstract EffectLayer Layer { get; }
        public DateTime Timestamp { get; }
        public IDuration Duration { get; }

        public bool IsExpired => Duration.HasExpired(null!);

        protected ContinuousEffect(ICard source, IPlayer controller, IDuration duration)
        {
            Id = Guid.NewGuid();
            Source = source;
            Controller = controller;
            Timestamp = DateTime.UtcNow;
            Duration = duration;
        }

        public abstract void Apply(IGameState gameState);
        public abstract void Remove(IGameState gameState);
        public abstract bool AppliesTo(IPermanent permanent, IGameState gameState);
    }

    /// <summary>
    /// Effect that modifies power and toughness.
    /// </summary>
    public class PowerToughnessModificationEffect : ContinuousEffect
    {
        public int PowerModification { get; }
        public int ToughnessModification { get; }
        public Func<ICreature, bool> AffectedCreatures { get; }

        public override string Description => $"+{PowerModification}/+{ToughnessModification}";
        public override EffectLayer Layer => EffectLayer.Layer7c_ModifyPowerToughness;

        public PowerToughnessModificationEffect(
            ICard source,
            IPlayer controller,
            IDuration duration,
            int powerMod,
            int toughnessMod,
            Func<ICreature, bool> filter)
            : base(source, controller, duration)
        {
            PowerModification = powerMod;
            ToughnessModification = toughnessMod;
            AffectedCreatures = filter;
        }

        public override void Apply(IGameState gameState)
        {
            foreach (var creature in gameState.Battlefield.Creatures)
            {
                if (AppliesTo(creature, gameState))
                {
                    creature.Power += PowerModification;
                    creature.Toughness += ToughnessModification;
                }
            }
        }

        public override void Remove(IGameState gameState)
        {
            // Effects are reapplied each time, so removal is handled by not applying
        }

        public override bool AppliesTo(IPermanent permanent, IGameState gameState)
        {
            return permanent is ICreature creature && AffectedCreatures(creature);
        }
    }

    /// <summary>
    /// Effect that grants keyword abilities.
    /// </summary>
    public class GrantKeywordEffect : ContinuousEffect
    {
        public Core.Enums.KeywordAbility Keyword { get; }
        public Func<ICreature, bool> AffectedCreatures { get; }

        public override string Description => $"Grants {Keyword}";
        public override EffectLayer Layer => EffectLayer.Layer6_AbilityAddingRemovingEffects;

        public GrantKeywordEffect(
            ICard source,
            IPlayer controller,
            IDuration duration,
            Core.Enums.KeywordAbility keyword,
            Func<ICreature, bool> filter)
            : base(source, controller, duration)
        {
            Keyword = keyword;
            AffectedCreatures = filter;
        }

        public override void Apply(IGameState gameState)
        {
            foreach (var creature in gameState.Battlefield.Creatures)
            {
                if (AppliesTo(creature, gameState))
                {
                    creature.AddKeyword(Keyword);
                }
            }
        }

        public override void Remove(IGameState gameState)
        {
            // Effects are reapplied each time
        }

        public override bool AppliesTo(IPermanent permanent, IGameState gameState)
        {
            return permanent is ICreature creature && AffectedCreatures(creature);
        }
    }

    #region Duration Implementations

    /// <summary>
    /// Duration that never expires (permanent effect).
    /// </summary>
    public class PermanentDuration : IDuration
    {
        public static PermanentDuration Instance { get; } = new();

        public bool HasExpired(IGameState gameState) => false;
        public string GetDescription() => "permanently";
    }

    /// <summary>
    /// Duration that lasts until end of turn.
    /// </summary>
    public class UntilEndOfTurnDuration : IDuration
    {
        private readonly int _turnNumber;

        public UntilEndOfTurnDuration(int currentTurnNumber)
        {
            _turnNumber = currentTurnNumber;
        }

        public bool HasExpired(IGameState gameState)
        {
            return gameState?.TurnNumber > _turnNumber;
        }

        public string GetDescription() => "until end of turn";
    }

    /// <summary>
    /// Duration that lasts while source is on the battlefield.
    /// </summary>
    public class WhileSourceOnBattlefieldDuration : IDuration
    {
        private readonly Guid _sourceId;

        public WhileSourceOnBattlefieldDuration(Guid sourceId)
        {
            _sourceId = sourceId;
        }

        public bool HasExpired(IGameState gameState)
        {
            return gameState?.FindPermanent(_sourceId) == null;
        }

        public string GetDescription() => "while source is on the battlefield";
    }

    /// <summary>
    /// Duration that lasts for a number of turns.
    /// </summary>
    public class TurnsDuration : IDuration
    {
        private readonly int _endTurnNumber;

        public TurnsDuration(int currentTurnNumber, int numberOfTurns)
        {
            _endTurnNumber = currentTurnNumber + numberOfTurns;
        }

        public bool HasExpired(IGameState gameState)
        {
            return gameState?.TurnNumber > _endTurnNumber;
        }

        public string GetDescription() => "for several turns";
    }

    #endregion
}
