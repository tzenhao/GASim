using MTGOnline.Core.Enums;

namespace MTGOnline.Mana
{
    /// <summary>
    /// Represents a mana cost for casting spells or activating abilities.
    /// </summary>
    public class ManaCost
    {
        public int White { get; set; }
        public int Blue { get; set; }
        public int Black { get; set; }
        public int Red { get; set; }
        public int Green { get; set; }
        public int Colorless { get; set; }  // Specifically requires colorless (C)
        public int Generic { get; set; }     // Can be paid with any mana

        // Special mana symbols
        public int Snow { get; set; }        // Requires snow mana
        public List<HybridManaCost> HybridCosts { get; } = new();
        public List<PhyrexianManaCost> PhyrexianCosts { get; } = new();

        // X costs
        public int XCount { get; set; }      // Number of X in the cost
        public int XValue { get; set; }      // The chosen value for X

        public int TotalMana => White + Blue + Black + Red + Green + Colorless + Generic +
                                HybridCosts.Count + PhyrexianCosts.Count + (XCount * XValue);

        public int ConvertedManaCost => White + Blue + Black + Red + Green + Colorless + Generic +
                                        HybridCosts.Sum(h => h.ConvertedValue) +
                                        PhyrexianCosts.Sum(p => p.ConvertedValue) +
                                        (XCount * XValue);

        public ManaColor Colors
        {
            get
            {
                var colors = ManaColor.None;
                if (White > 0) colors |= ManaColor.White;
                if (Blue > 0) colors |= ManaColor.Blue;
                if (Black > 0) colors |= ManaColor.Black;
                if (Red > 0) colors |= ManaColor.Red;
                if (Green > 0) colors |= ManaColor.Green;
                return colors;
            }
        }

        public ManaCost()
        {
        }

        public ManaCost(int generic, int white = 0, int blue = 0, int black = 0, int red = 0, int green = 0)
        {
            Generic = generic;
            White = white;
            Blue = blue;
            Black = black;
            Red = red;
            Green = green;
        }

        public static ManaCost Parse(string manaCostString)
        {
            var cost = new ManaCost();

            // Simple parser for mana cost strings like "{2}{W}{W}" or "2WW"
            // Full implementation would handle all mana symbols

            int i = 0;
            while (i < manaCostString.Length)
            {
                char c = manaCostString[i];

                if (c == '{')
                {
                    // Find matching }
                    int end = manaCostString.IndexOf('}', i);
                    if (end > i)
                    {
                        string symbol = manaCostString.Substring(i + 1, end - i - 1);
                        ParseSymbol(cost, symbol);
                        i = end + 1;
                        continue;
                    }
                }
                else if (char.IsDigit(c))
                {
                    // Generic mana
                    string numberStr = "";
                    while (i < manaCostString.Length && char.IsDigit(manaCostString[i]))
                    {
                        numberStr += manaCostString[i];
                        i++;
                    }
                    cost.Generic += int.Parse(numberStr);
                    continue;
                }
                else
                {
                    ParseSymbol(cost, c.ToString());
                }
                i++;
            }

            return cost;
        }

        private static void ParseSymbol(ManaCost cost, string symbol)
        {
            switch (symbol.ToUpper())
            {
                case "W": cost.White++; break;
                case "U": cost.Blue++; break;
                case "B": cost.Black++; break;
                case "R": cost.Red++; break;
                case "G": cost.Green++; break;
                case "C": cost.Colorless++; break;
                case "S": cost.Snow++; break;
                case "X": cost.XCount++; break;
                default:
                    if (int.TryParse(symbol, out int generic))
                    {
                        cost.Generic += generic;
                    }
                    // Handle hybrid (W/U, etc.) and Phyrexian (W/P, etc.) in full implementation
                    break;
            }
        }

        public override string ToString()
        {
            var parts = new List<string>();

            if (XCount > 0)
            {
                for (int i = 0; i < XCount; i++)
                    parts.Add("{X}");
            }
            if (Generic > 0) parts.Add($"{{{Generic}}}");
            if (White > 0) parts.Add(string.Concat(Enumerable.Repeat("{W}", White)));
            if (Blue > 0) parts.Add(string.Concat(Enumerable.Repeat("{U}", Blue)));
            if (Black > 0) parts.Add(string.Concat(Enumerable.Repeat("{B}", Black)));
            if (Red > 0) parts.Add(string.Concat(Enumerable.Repeat("{R}", Red)));
            if (Green > 0) parts.Add(string.Concat(Enumerable.Repeat("{G}", Green)));
            if (Colorless > 0) parts.Add(string.Concat(Enumerable.Repeat("{C}", Colorless)));
            if (Snow > 0) parts.Add(string.Concat(Enumerable.Repeat("{S}", Snow)));

            return parts.Count > 0 ? string.Join("", parts) : "{0}";
        }
    }

    /// <summary>
    /// Represents a hybrid mana cost that can be paid with either of two colors.
    /// </summary>
    public class HybridManaCost
    {
        public ManaColor Option1 { get; set; }
        public ManaColor Option2 { get; set; }
        public int ConvertedValue => 1;  // Hybrid mana counts as 1 CMC

        public HybridManaCost(ManaColor option1, ManaColor option2)
        {
            Option1 = option1;
            Option2 = option2;
        }

        public override string ToString() => $"{{{GetSymbol(Option1)}/{GetSymbol(Option2)}}}";

        private static string GetSymbol(ManaColor color) => color switch
        {
            ManaColor.White => "W",
            ManaColor.Blue => "U",
            ManaColor.Black => "B",
            ManaColor.Red => "R",
            ManaColor.Green => "G",
            _ => "?"
        };
    }

    /// <summary>
    /// Represents a Phyrexian mana cost that can be paid with mana or 2 life.
    /// </summary>
    public class PhyrexianManaCost
    {
        public ManaColor Color { get; set; }
        public int ConvertedValue => 1;

        public PhyrexianManaCost(ManaColor color)
        {
            Color = color;
        }

        public override string ToString()
        {
            string symbol = Color switch
            {
                ManaColor.White => "W",
                ManaColor.Blue => "U",
                ManaColor.Black => "B",
                ManaColor.Red => "R",
                ManaColor.Green => "G",
                _ => "?"
            };
            return $"{{{symbol}/P}}";
        }
    }
}
