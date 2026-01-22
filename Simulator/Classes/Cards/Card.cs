using static Shared.Enums;

namespace Classes.Cards
{
    public class Card
    {
        public Card()
        {
        }


        #region Properties
        // Name of the card
        public string Name { get; set; }


        #region Casting Information

        // Element of card
        public Elements Element { get; set; }

        // Whether card is exalted or not
        public bool IsExalted { get; set; }

        // Numeric cost of card
        public int Cost { get; set; }

        // What kind of cost the card has
        public CostType CostType { get; set; }

        #endregion

        #endregion

    }

}
