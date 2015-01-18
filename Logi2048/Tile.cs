using System;
using System.Drawing;

namespace Logi2048
{
    /// <summary>
    /// Represents an individual tile on the game board.
    /// </summary>
    public class Tile
    {
        private static readonly Color[] TileColors =
        {
            Color.Red,
            Color.Blue,
            Color.Orange,
            Color.DarkGreen,
            Color.Purple,
            Color.DarkOrange,
            Color.DarkBlue,
            Color.DarkRed,
            Color.IndianRed,
            Color.Indigo,
            Color.LightSteelBlue,
        };

        private static int ValueToColorIndex(int value)
        {
            var index = 0;
            for (int temp = value; temp > 2; temp >>= 1, index++) ;
            return index;
        }

        public int Row { get; private set; }
        public int Column { get; private set; }
        public int Value { get; set; }
        
        public bool Merged { get;  private set; }
        public Tile MergedFrom { get; private set; }
        
        
        public bool Moved { get; private set; }
        public Tile MovedFrom { get; private set; }

        public Tile(int row, int col)
        {
            Row = row;
            Column = col;
            Value = 0;
        }

        public void MoveTo(Tile tile)
        {
            if (!tile.Empty && tile.Value != Value)
            {
                throw new ArgumentException(string.Format(
                    "Illegal move. Cannot merge {0} and {1}",
                    Value,
                    tile.Value));
            }

            if (!tile.Empty)
            {
                tile.Merged = true;
                tile.MergedFrom = this;
            }
            else
            {
                tile.Moved = true;
                tile.MovedFrom = this;
            }

            tile.Value += Value;
            Value = 0;
        }

        public void ClearMoveData()
        {
            Merged = false;
            Moved = false;
            MovedFrom = null;
            MergedFrom = null;
        }

        public bool Empty { get { return Value == 0; } }
    }
}
