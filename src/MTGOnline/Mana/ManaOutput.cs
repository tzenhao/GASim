using MTGOnline.Core.Enums;

namespace MTGOnline.Mana
{
    /// <summary>
    /// Represents the output of a mana ability.
    /// </summary>
    public class ManaOutput
    {
        public int White { get; set; }
        public int Blue { get; set; }
        public int Black { get; set; }
        public int Red { get; set; }
        public int Green { get; set; }
        public int Colorless { get; set; }

        // For abilities that produce mana of any color
        public bool ProducesAnyColor { get; set; }
        public int AnyColorAmount { get; set; }

        public int TotalMana => White + Blue + Black + Red + Green + Colorless + AnyColorAmount;

        public ManaOutput()
        {
        }

        public ManaOutput(ManaColor color, int amount = 1)
        {
            switch (color)
            {
                case ManaColor.White: White = amount; break;
                case ManaColor.Blue: Blue = amount; break;
                case ManaColor.Black: Black = amount; break;
                case ManaColor.Red: Red = amount; break;
                case ManaColor.Green: Green = amount; break;
                case ManaColor.Colorless: Colorless = amount; break;
            }
        }

        public static ManaOutput None => new();

        public static ManaOutput OneWhite => new(ManaColor.White, 1);
        public static ManaOutput OneBlue => new(ManaColor.Blue, 1);
        public static ManaOutput OneBlack => new(ManaColor.Black, 1);
        public static ManaOutput OneRed => new(ManaColor.Red, 1);
        public static ManaOutput OneGreen => new(ManaColor.Green, 1);
        public static ManaOutput OneColorless => new(ManaColor.Colorless, 1);

        public static ManaOutput AnyColor(int amount = 1) => new()
        {
            ProducesAnyColor = true,
            AnyColorAmount = amount
        };

        public ManaOutput Add(ManaOutput other)
        {
            return new ManaOutput
            {
                White = White + other.White,
                Blue = Blue + other.Blue,
                Black = Black + other.Black,
                Red = Red + other.Red,
                Green = Green + other.Green,
                Colorless = Colorless + other.Colorless,
                ProducesAnyColor = ProducesAnyColor || other.ProducesAnyColor,
                AnyColorAmount = AnyColorAmount + other.AnyColorAmount
            };
        }

        public override string ToString()
        {
            var parts = new List<string>();

            if (White > 0) parts.Add($"{White}W");
            if (Blue > 0) parts.Add($"{Blue}U");
            if (Black > 0) parts.Add($"{Black}B");
            if (Red > 0) parts.Add($"{Red}R");
            if (Green > 0) parts.Add($"{Green}G");
            if (Colorless > 0) parts.Add($"{Colorless}C");
            if (ProducesAnyColor && AnyColorAmount > 0) parts.Add($"{AnyColorAmount} of any color");

            return parts.Count > 0 ? string.Join(", ", parts) : "No mana";
        }
    }
}
