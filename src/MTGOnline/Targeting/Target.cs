using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;

namespace MTGOnline.Targeting
{
    /// <summary>
    /// Represents a target for a spell or ability.
    /// </summary>
    public class Target : ITarget
    {
        public Guid TargetId { get; }
        public TargetType TargetType { get; }

        private readonly IPlayer? _player;
        private readonly IPermanent? _permanent;
        private readonly ICard? _card;
        private readonly IStackObject? _stackObject;

        private Target(Guid targetId, TargetType targetType)
        {
            TargetId = targetId;
            TargetType = targetType;
        }

        public static Target FromPlayer(IPlayer player)
        {
            return new Target(player.Id, TargetType.Player)
            {
                _player = player
            };
        }

        public static Target FromPermanent(IPermanent permanent)
        {
            var targetType = TargetType.None;

            if (permanent is ICreature) targetType |= TargetType.Creature;
            if (permanent is IArtifact) targetType |= TargetType.Artifact;
            if (permanent is IEnchantment) targetType |= TargetType.Enchantment;
            if (permanent is IPlaneswalker) targetType |= TargetType.Planeswalker;
            if (permanent is ILand) targetType |= TargetType.Land;

            return new Target(permanent.Id, targetType)
            {
                _permanent = permanent
            };
        }

        public static Target FromCard(ICard card, ZoneType zone)
        {
            return new Target(card.Id, TargetType.Card)
            {
                _card = card
            };
        }

        public static Target FromStackObject(IStackObject stackObject)
        {
            var targetType = stackObject.IsSpell ? TargetType.Spell : TargetType.Ability;

            return new Target(stackObject.Id, targetType)
            {
                _stackObject = stackObject
            };
        }

        public bool IsLegalTarget(IGameState gameState)
        {
            // Check if target still exists and is valid
            if (_player != null)
            {
                return !_player.HasLost && gameState.Players.Contains(_player);
            }

            if (_permanent != null)
            {
                // Check if permanent is still on battlefield
                var foundPermanent = gameState.FindPermanent(_permanent.Id);
                if (foundPermanent == null) return false;

                // Check for hexproof/shroud
                if (_permanent is ICreature creature)
                {
                    if (creature.HasKeyword(KeywordAbility.Shroud)) return false;
                    // Hexproof would need to check if controller is different
                }

                return true;
            }

            if (_card != null)
            {
                // Check if card is still in the expected zone
                return gameState.FindCard(_card.Id) != null;
            }

            if (_stackObject != null)
            {
                // Check if stack object is still on the stack
                return gameState.Stack.StackObjects.Any(so => so.Id == _stackObject.Id);
            }

            return false;
        }

        public IPlayer? AsPlayer() => _player;
        public IPermanent? AsPermanent() => _permanent;
        public ICard? AsCard() => _card ?? _permanent as ICard;
        public IStackObject? AsStackObject() => _stackObject;
    }

    /// <summary>
    /// Defines requirements for targeting.
    /// </summary>
    public class TargetRequirement : ITargetRequirement
    {
        public TargetType AllowedTargetTypes { get; set; }
        public TargetController TargetController { get; set; } = TargetController.Any;
        public int MinimumTargets { get; set; } = 1;
        public int MaximumTargets { get; set; } = 1;
        public string Description { get; set; } = "";

        // Additional filters
        public Func<IPermanent, bool>? PermanentFilter { get; set; }
        public Func<IPlayer, bool>? PlayerFilter { get; set; }
        public Func<ICard, bool>? CardFilter { get; set; }

        public TargetRequirement(TargetType allowedTypes, string description = "")
        {
            AllowedTargetTypes = allowedTypes;
            Description = description;
        }

        public bool IsValidTarget(ITarget target, IGameState gameState)
        {
            // Check target type
            if ((target.TargetType & AllowedTargetTypes) == 0)
            {
                return false;
            }

            // Check controller restrictions
            if (target.AsPermanent() is IPermanent permanent)
            {
                switch (TargetController)
                {
                    case TargetController.You:
                        if (permanent.Controller != gameState.PriorityPlayer)
                            return false;
                        break;
                    case TargetController.Opponent:
                        if (permanent.Controller == gameState.PriorityPlayer)
                            return false;
                        break;
                }

                // Apply custom filter
                if (PermanentFilter != null && !PermanentFilter(permanent))
                    return false;
            }

            if (target.AsPlayer() is IPlayer player)
            {
                switch (TargetController)
                {
                    case TargetController.You:
                        if (player != gameState.PriorityPlayer)
                            return false;
                        break;
                    case TargetController.Opponent:
                        if (player == gameState.PriorityPlayer)
                            return false;
                        break;
                    case TargetController.AnotherPlayer:
                        if (player == gameState.PriorityPlayer)
                            return false;
                        break;
                }

                if (PlayerFilter != null && !PlayerFilter(player))
                    return false;
            }

            if (target.AsCard() is ICard card && CardFilter != null)
            {
                if (!CardFilter(card))
                    return false;
            }

            return target.IsLegalTarget(gameState);
        }

        public IReadOnlyList<ITarget> GetValidTargets(IGameState gameState, IPlayer choosingPlayer)
        {
            var validTargets = new List<ITarget>();

            // Check players
            if ((AllowedTargetTypes & TargetType.Player) != 0)
            {
                foreach (var player in gameState.Players)
                {
                    var target = Target.FromPlayer(player);
                    if (IsValidTarget(target, gameState))
                    {
                        validTargets.Add(target);
                    }
                }
            }

            // Check permanents
            if ((AllowedTargetTypes & TargetType.Permanent) != 0)
            {
                foreach (var permanent in gameState.Battlefield.Permanents)
                {
                    var target = Target.FromPermanent(permanent);
                    if (IsValidTarget(target, gameState))
                    {
                        validTargets.Add(target);
                    }
                }
            }

            // Check stack
            if ((AllowedTargetTypes & (TargetType.Spell | TargetType.Ability)) != 0)
            {
                foreach (var stackObject in gameState.Stack.StackObjects)
                {
                    var target = Target.FromStackObject(stackObject);
                    if (IsValidTarget(target, gameState))
                    {
                        validTargets.Add(target);
                    }
                }
            }

            return validTargets.AsReadOnly();
        }
    }

    /// <summary>
    /// Common target requirements.
    /// </summary>
    public static class CommonTargets
    {
        public static TargetRequirement AnyTarget => new(TargetType.AnyTarget, "any target");

        public static TargetRequirement TargetCreature => new(TargetType.Creature, "target creature");

        public static TargetRequirement TargetPlayer => new(TargetType.Player, "target player");

        public static TargetRequirement TargetOpponent => new(TargetType.Player, "target opponent")
        {
            TargetController = TargetController.Opponent
        };

        public static TargetRequirement TargetPermanent => new(TargetType.Permanent, "target permanent");

        public static TargetRequirement TargetNonlandPermanent => new(
            TargetType.Creature | TargetType.Artifact | TargetType.Enchantment | TargetType.Planeswalker,
            "target nonland permanent");

        public static TargetRequirement TargetSpell => new(TargetType.Spell, "target spell");

        public static TargetRequirement TargetCreatureOrPlaneswalker => new(
            TargetType.Creature | TargetType.Planeswalker,
            "target creature or planeswalker");
    }
}
