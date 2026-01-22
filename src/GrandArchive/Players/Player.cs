using GrandArchive.Cards.Types;
using GrandArchive.Core.Enums;
using GrandArchive.Interfaces;
using GrandArchive.Zones;

namespace GrandArchive.Players
{
    /// <summary>
    /// Player in Grand Archive TCG.
    /// </summary>
    public class Player : IPlayer
    {
        public Guid Id { get; } = Guid.NewGuid();

        public string Name { get; init; } = string.Empty;

        public IChampion? Champion { get; set; }

        public bool HasOpportunity { get; set; }

        public bool IsTurnPlayer { get; set; }

        /// <summary>
        /// Player's main deck zone.
        /// </summary>
        public MainDeck MainDeckZone { get; }
        IZone IPlayer.MainDeck => MainDeckZone;

        /// <summary>
        /// Player's material deck zone.
        /// </summary>
        public MaterialDeck MaterialDeckZone { get; }
        IZone IPlayer.MaterialDeck => MaterialDeckZone;

        /// <summary>
        /// Player's hand zone.
        /// </summary>
        public Hand HandZone { get; }
        IZone IPlayer.Hand => HandZone;

        /// <summary>
        /// Player's memory zone.
        /// </summary>
        public Memory MemoryZone { get; }
        IZone IPlayer.Memory => MemoryZone;

        /// <summary>
        /// Player's graveyard zone.
        /// </summary>
        public Graveyard GraveyardZone { get; }
        IZone IPlayer.Graveyard => GraveyardZone;

        /// <summary>
        /// Player's banishment zone.
        /// </summary>
        public Banishment BanishmentZone { get; }
        IZone IPlayer.Banishment => BanishmentZone;

        /// <summary>
        /// Number of cards to recollect during Recollection phase.
        /// Default is 1.
        /// </summary>
        public int RecollectionAmount { get; set; } = 1;

        /// <summary>
        /// Whether this player has lost the game.
        /// </summary>
        public bool HasLost { get; private set; }

        /// <summary>
        /// Reason for losing (if applicable).
        /// </summary>
        public string? LossReason { get; private set; }

        public Player()
        {
            MainDeckZone = new MainDeck { Owner = this };
            MaterialDeckZone = new MaterialDeck { Owner = this };
            HandZone = new Hand { Owner = this };
            MemoryZone = new Memory { Owner = this };
            GraveyardZone = new Graveyard { Owner = this };
            BanishmentZone = new Banishment { Owner = this };
        }

        /// <summary>
        /// Draw a card from main deck to hand.
        /// </summary>
        public bool DrawCard()
        {
            var card = MainDeckZone.Draw();
            if (card != null)
            {
                HandZone.Add(card);
                return true;
            }

            // If deck is empty and player needs to draw, they lose
            Lose("Unable to draw from empty deck");
            return false;
        }

        /// <summary>
        /// Draw multiple cards from main deck to hand.
        /// </summary>
        public int DrawCards(int count)
        {
            int drawn = 0;
            for (int i = 0; i < count; i++)
            {
                if (DrawCard())
                {
                    drawn++;
                }
                else
                {
                    break;
                }
            }
            return drawn;
        }

        /// <summary>
        /// Place a card from hand into memory (to pay costs).
        /// </summary>
        public bool PlaceInMemory(ICard card)
        {
            if (!HandZone.Contains(card))
                return false;

            HandZone.Remove(card);
            MemoryZone.PlaceInMemory(card);
            return true;
        }

        /// <summary>
        /// Return a card from memory to hand.
        /// </summary>
        public bool RecollectFromMemory(ICard card)
        {
            var recollected = MemoryZone.Recollect(card);
            if (recollected != null)
            {
                HandZone.Add(recollected);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Recollect cards during Recollection phase.
        /// </summary>
        public int Recollect(int count)
        {
            int recollected = 0;
            var cards = MemoryZone.Recollect(count);
            foreach (var card in cards)
            {
                HandZone.Add(card);
                recollected++;
            }
            return recollected;
        }

        /// <summary>
        /// Pay a reserve cost by placing cards from hand into memory.
        /// </summary>
        public bool PayReserveCost(int cost, IEnumerable<ICard> cardsToPlace)
        {
            var cardList = cardsToPlace.ToList();
            if (cardList.Count < cost)
                return false;

            // Verify all cards are in hand
            if (!cardList.All(c => HandZone.Contains(c)))
                return false;

            // Place cards into memory
            for (int i = 0; i < cost; i++)
            {
                PlaceInMemory(cardList[i]);
            }

            return true;
        }

        /// <summary>
        /// Pay a memory cost by banishing cards from memory.
        /// </summary>
        public bool PayMemoryCost(int cost, IEnumerable<ICard> cardsToBanish)
        {
            var cardList = cardsToBanish.ToList();
            if (cardList.Count < cost)
                return false;

            // Verify all cards are in memory
            if (!cardList.All(c => MemoryZone.Contains(c)))
                return false;

            // Banish cards from memory
            for (int i = 0; i < cost; i++)
            {
                var card = MemoryZone.Banish(cardList[i]);
                if (card != null)
                {
                    BanishmentZone.Add(card);
                }
            }

            return true;
        }

        /// <summary>
        /// Materialize the champion at game start.
        /// </summary>
        public bool MaterializeChampion()
        {
            var championCard = MaterialDeckZone.GetChampion();
            if (championCard is ChampionCard champion)
            {
                MaterialDeckZone.Remove(champion);
                Champion = champion;
                champion.Owner = this;
                champion.Controller = this;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Materialize a card from the material deck.
        /// </summary>
        public ICard? MaterializeFromMaterialDeck(Func<ICard, bool> predicate)
        {
            return MaterialDeckZone.Materialize(predicate);
        }

        /// <summary>
        /// Discard a card from hand to graveyard.
        /// </summary>
        public bool Discard(ICard card)
        {
            var discarded = HandZone.Discard(card);
            if (discarded != null)
            {
                GraveyardZone.Add(discarded);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Discard down to maximum hand size.
        /// </summary>
        public List<ICard> DiscardToHandSize()
        {
            var discarded = new List<ICard>();
            while (HandZone.ExceedsMaxSize)
            {
                var card = HandZone.DiscardRandom();
                if (card != null)
                {
                    GraveyardZone.Add(card);
                    discarded.Add(card);
                }
            }
            return discarded;
        }

        /// <summary>
        /// Mark this player as having lost the game.
        /// </summary>
        public void Lose(string reason)
        {
            HasLost = true;
            LossReason = reason;
        }

        /// <summary>
        /// Check if champion is defeated.
        /// </summary>
        public bool IsChampionDefeated => Champion?.IsDefeated ?? false;

        /// <summary>
        /// Get total reserve available (cards in memory).
        /// </summary>
        public int AvailableReserve => MemoryZone.TotalReserve;

        public override string ToString() => $"{Name} (Champion: {Champion?.Name ?? "None"})";
    }
}
