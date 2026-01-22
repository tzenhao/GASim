using MTGOnline.Core.Enums;
using MTGOnline.Interfaces;

namespace MTGOnline.Zones
{
    /// <summary>
    /// Represents the battlefield zone (shared between all players).
    /// </summary>
    public class Battlefield : Zone, IBattlefield
    {
        private readonly List<IPermanent> _permanents = new();
        public IReadOnlyList<IPermanent> Permanents => _permanents.AsReadOnly();

        public IReadOnlyList<ICreature> Creatures =>
            _permanents.OfType<ICreature>().ToList().AsReadOnly();

        public IReadOnlyList<ILand> Lands =>
            _permanents.OfType<ILand>().ToList().AsReadOnly();

        public IReadOnlyList<IArtifact> Artifacts =>
            _permanents.OfType<IArtifact>().ToList().AsReadOnly();

        public IReadOnlyList<IEnchantment> Enchantments =>
            _permanents.OfType<IEnchantment>().ToList().AsReadOnly();

        public IReadOnlyList<IPlaneswalker> Planeswalkers =>
            _permanents.OfType<IPlaneswalker>().ToList().AsReadOnly();

        public Battlefield() : base(ZoneType.Battlefield, ZoneVisibility.Public, null)
        {
        }

        public void AddPermanent(IPermanent permanent)
        {
            _permanents.Add(permanent);
            _cards.Add(permanent as ICard ?? throw new InvalidOperationException("Permanent must be a card"));
        }

        public bool RemovePermanent(IPermanent permanent)
        {
            _cards.Remove(permanent as ICard ?? throw new InvalidOperationException());
            return _permanents.Remove(permanent);
        }

        public IReadOnlyList<IPermanent> GetPermanentsControlledBy(IPlayer player)
        {
            return _permanents.Where(p => p.Controller == player).ToList().AsReadOnly();
        }

        public IReadOnlyList<ICreature> GetCreaturesControlledBy(IPlayer player)
        {
            return _permanents.OfType<ICreature>()
                .Where(c => c.Controller == player)
                .ToList().AsReadOnly();
        }

        public IReadOnlyList<ILand> GetLandsControlledBy(IPlayer player)
        {
            return _permanents.OfType<ILand>()
                .Where(l => l.Controller == player)
                .ToList().AsReadOnly();
        }

        public IReadOnlyList<IPermanent> GetPermanentsOwnedBy(IPlayer player)
        {
            return _permanents.Where(p => p.Owner == player).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets all permanents of a specific type controlled by a player.
        /// </summary>
        public IReadOnlyList<IPermanent> GetPermanents(IPlayer player, CardType type)
        {
            return _permanents.Where(p =>
                p.Controller == player &&
                p.HasCardType(type)).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets all untapped permanents controlled by a player.
        /// </summary>
        public IReadOnlyList<IPermanent> GetUntappedPermanents(IPlayer player)
        {
            return _permanents.Where(p =>
                p.Controller == player &&
                !p.IsTapped).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets all tapped permanents controlled by a player.
        /// </summary>
        public IReadOnlyList<IPermanent> GetTappedPermanents(IPlayer player)
        {
            return _permanents.Where(p =>
                p.Controller == player &&
                p.IsTapped).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets all creatures that can attack.
        /// </summary>
        public IReadOnlyList<ICreature> GetAttackEligibleCreatures(IPlayer player)
        {
            return Creatures.Where(c =>
                c.Controller == player &&
                !c.IsTapped &&
                !c.HasSummoningSickness &&
                !c.HasKeyword(KeywordAbility.Defender)).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets all creatures that can block.
        /// </summary>
        public IReadOnlyList<ICreature> GetBlockEligibleCreatures(IPlayer player)
        {
            return Creatures.Where(c =>
                c.Controller == player &&
                !c.IsTapped).ToList().AsReadOnly();
        }

        /// <summary>
        /// Untaps all permanents controlled by a player.
        /// </summary>
        public void UntapAll(IPlayer player)
        {
            foreach (var permanent in GetPermanentsControlledBy(player))
            {
                permanent.Untap();
            }
        }

        /// <summary>
        /// Destroys a permanent (moves it to graveyard).
        /// </summary>
        public void Destroy(IPermanent permanent, IZone graveyard)
        {
            if (RemovePermanent(permanent))
            {
                graveyard.Add(permanent as ICard ?? throw new InvalidOperationException());
            }
        }

        /// <summary>
        /// Exiles a permanent.
        /// </summary>
        public void Exile(IPermanent permanent, IZone exileZone)
        {
            if (RemovePermanent(permanent))
            {
                exileZone.Add(permanent as ICard ?? throw new InvalidOperationException());
            }
        }
    }
}
