using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;

namespace GrandArchive.Zones
{
    /// <summary>
    /// Field zone in Grand Archive TCG.
    /// Where cards are played and exist as objects.
    /// Shared public zone between all players.
    /// </summary>
    public class Field : Zone
    {
        public override ZoneType ZoneType => ZoneType.Field;

        public override bool IsPublic => true;

        public override bool IsOrdered => false;

        /// <summary>
        /// Field is a shared zone, so Owner is null.
        /// </summary>
        public new IPlayer? Owner => null;

        public override void Add(ICard card)
        {
            base.Add(card);
            // Field cards are always face-up
            card.Visibility = CardVisibility.FaceUp;
        }

        /// <summary>
        /// Get all cards controlled by a specific player.
        /// </summary>
        public IEnumerable<ICard> GetCardsControlledBy(IPlayer player)
        {
            return _cards.Where(c => c.Controller == player);
        }

        /// <summary>
        /// Get all units (champions and allies) on the field.
        /// </summary>
        public IEnumerable<IUnit> GetUnits()
        {
            return _cards.OfType<IUnit>();
        }

        /// <summary>
        /// Get all units controlled by a specific player.
        /// </summary>
        public IEnumerable<IUnit> GetUnitsControlledBy(IPlayer player)
        {
            return _cards.OfType<IUnit>().Where(u => ((ICard)u).Controller == player);
        }

        /// <summary>
        /// Get all allies on the field.
        /// </summary>
        public IEnumerable<ICard> GetAllies()
        {
            return _cards.Where(c => c.CardType == CardType.Ally);
        }

        /// <summary>
        /// Get all allies controlled by a specific player.
        /// </summary>
        public IEnumerable<ICard> GetAlliesControlledBy(IPlayer player)
        {
            return _cards.Where(c => c.CardType == CardType.Ally && c.Controller == player);
        }

        /// <summary>
        /// Get all awake units controlled by a player.
        /// </summary>
        public IEnumerable<IUnit> GetAwakeUnitsControlledBy(IPlayer player)
        {
            return GetUnitsControlledBy(player).Where(u => u.State == UnitState.Awake);
        }

        /// <summary>
        /// Get all weapons on the field.
        /// </summary>
        public IEnumerable<ICard> GetWeapons()
        {
            return _cards.Where(c => c.CardType == CardType.Weapon);
        }

        /// <summary>
        /// Get all domains on the field.
        /// </summary>
        public IEnumerable<ICard> GetDomains()
        {
            return _cards.Where(c => c.CardType == CardType.Domain);
        }

        /// <summary>
        /// Get all attackable objects controlled by a player.
        /// Includes units and siegeable domains.
        /// </summary>
        public IEnumerable<ICard> GetAttackableObjectsControlledBy(IPlayer player)
        {
            return _cards.Where(c =>
                c.Controller == player &&
                (c.CardType == CardType.Champion ||
                 c.CardType == CardType.Ally ||
                 (c.CardType == CardType.Domain && c is Cards.Types.DomainCard domain && domain.IsSiegeable)));
        }

        /// <summary>
        /// Wake up all units controlled by a player.
        /// </summary>
        public void WakeUpAllUnits(IPlayer player)
        {
            foreach (var unit in GetUnitsControlledBy(player))
            {
                unit.WakeUp();
            }
        }

        public override string ToString() => $"Field ({Count} cards)";
    }
}
