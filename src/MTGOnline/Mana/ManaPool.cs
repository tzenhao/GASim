using MTGOnline.Core.Enums;

namespace MTGOnline.Mana
{
    /// <summary>
    /// Represents a player's mana pool.
    /// </summary>
    public class ManaPool
    {
        private readonly Dictionary<ManaColor, int> _mana = new()
        {
            { ManaColor.White, 0 },
            { ManaColor.Blue, 0 },
            { ManaColor.Black, 0 },
            { ManaColor.Red, 0 },
            { ManaColor.Green, 0 },
            { ManaColor.Colorless, 0 }
        };

        public int White => _mana[ManaColor.White];
        public int Blue => _mana[ManaColor.Blue];
        public int Black => _mana[ManaColor.Black];
        public int Red => _mana[ManaColor.Red];
        public int Green => _mana[ManaColor.Green];
        public int Colorless => _mana[ManaColor.Colorless];

        public int TotalMana => _mana.Values.Sum();

        public bool IsEmpty => TotalMana == 0;

        public void Add(ManaColor color, int amount = 1)
        {
            if (amount <= 0) return;

            if (_mana.ContainsKey(color))
            {
                _mana[color] += amount;
            }
        }

        public void Add(ManaOutput output)
        {
            Add(ManaColor.White, output.White);
            Add(ManaColor.Blue, output.Blue);
            Add(ManaColor.Black, output.Black);
            Add(ManaColor.Red, output.Red);
            Add(ManaColor.Green, output.Green);
            Add(ManaColor.Colorless, output.Colorless);
        }

        public bool Remove(ManaColor color, int amount = 1)
        {
            if (amount <= 0) return true;

            if (_mana.ContainsKey(color) && _mana[color] >= amount)
            {
                _mana[color] -= amount;
                return true;
            }
            return false;
        }

        public int GetAmount(ManaColor color)
        {
            return _mana.TryGetValue(color, out int amount) ? amount : 0;
        }

        public bool CanPay(ManaCost cost)
        {
            // Clone current mana for simulation
            var available = new Dictionary<ManaColor, int>(_mana);

            // First, pay specific color costs
            if (available[ManaColor.White] < cost.White) return false;
            available[ManaColor.White] -= cost.White;

            if (available[ManaColor.Blue] < cost.Blue) return false;
            available[ManaColor.Blue] -= cost.Blue;

            if (available[ManaColor.Black] < cost.Black) return false;
            available[ManaColor.Black] -= cost.Black;

            if (available[ManaColor.Red] < cost.Red) return false;
            available[ManaColor.Red] -= cost.Red;

            if (available[ManaColor.Green] < cost.Green) return false;
            available[ManaColor.Green] -= cost.Green;

            // Pay colorless-specific costs
            if (available[ManaColor.Colorless] < cost.Colorless) return false;
            available[ManaColor.Colorless] -= cost.Colorless;

            // Calculate remaining mana for generic costs
            int remainingMana = available.Values.Sum();

            // Generic costs can be paid with any mana
            int genericNeeded = cost.Generic + (cost.XCount * cost.XValue);

            return remainingMana >= genericNeeded;
        }

        public bool Pay(ManaCost cost)
        {
            if (!CanPay(cost)) return false;

            // Pay specific colors
            _mana[ManaColor.White] -= cost.White;
            _mana[ManaColor.Blue] -= cost.Blue;
            _mana[ManaColor.Black] -= cost.Black;
            _mana[ManaColor.Red] -= cost.Red;
            _mana[ManaColor.Green] -= cost.Green;
            _mana[ManaColor.Colorless] -= cost.Colorless;

            // Pay generic costs (simplified: pay colorless first, then any color)
            int genericRemaining = cost.Generic + (cost.XCount * cost.XValue);

            // Pay with colorless first
            int fromColorless = Math.Min(_mana[ManaColor.Colorless], genericRemaining);
            _mana[ManaColor.Colorless] -= fromColorless;
            genericRemaining -= fromColorless;

            // Then pay with colored mana (in a defined order)
            var colorOrder = new[] { ManaColor.White, ManaColor.Blue, ManaColor.Black, ManaColor.Red, ManaColor.Green };
            foreach (var color in colorOrder)
            {
                if (genericRemaining <= 0) break;
                int fromColor = Math.Min(_mana[color], genericRemaining);
                _mana[color] -= fromColor;
                genericRemaining -= fromColor;
            }

            return true;
        }

        public void Empty()
        {
            foreach (var key in _mana.Keys.ToList())
            {
                _mana[key] = 0;
            }
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

            return parts.Count > 0 ? string.Join(", ", parts) : "Empty";
        }
    }
}
