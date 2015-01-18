using System.Collections.Generic;
using System.Linq;

namespace Logi2048
{
    /// <summary>
    /// Represents a square board of tiles.
    /// </summary>
    public class Board : IEnumerable<Tile>
    {
        private Tile[] tiles;

        public int Size { get; private set; }

        public Board(int size)
        {
            Size = size;

            tiles = new Tile[Size * Size];
            for (int row = 0 ; row < size ; row++)
            {
                for (int col = 0 ; col < size ; col++)
                {
                    tiles[GetIndex(row, col)] = new Tile(row, col);
                }
            }
        }

        private int GetIndex(int row, int col)
        {
            return row * Size + col;
        }

        public int HighestValue
        {
            get { return tiles.Max(t => t.Value); }
        }

        public Tile this[int row, int col]
        {
            get { return tiles[GetIndex(row, col)]; }
        }

        public bool HasPositionsAvailable
        {
            get { return GetAvailablePositions().Any(); }
        }

        public IEnumerable<Tile> GetAvailablePositions()
        {
            return tiles.Where(t => t.Empty);
        }

        public IEnumerator<Tile> GetEnumerator()
        {
            return tiles.AsEnumerable<Tile>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets a nested IEnumerable that can iterate over each row and then
        /// each Tile within the row.
        /// </summary>
        /// 
        /// <code>
        /// foreach (var row in board.GroupByRows())
        /// {
        ///     foreach (var tile in row)
        ///     {
        ///         // handle tile here
        ///     }
        /// }
        /// </code>
        /// 
        /// <returns>An IEnumerable as described in the summary</returns>
        public IEnumerable<IEnumerable<Tile>> GroupByRows()
        {
            return from tile in tiles
                   group tile by tile.Row into row
                   orderby row.Key
                   select row;
        }

        /// <summary>
        /// Gets a nested IEnumerable that can iterate over each column and then
        /// each Tile within the column.
        /// </summary>
        /// 
        /// <code>
        /// foreach (var col in board.GroupByColumns())
        /// {
        ///     foreach (var tile in col)
        ///     {
        ///         // handle tile here
        ///     }
        /// }
        /// </code>
        /// 
        /// <returns>An IEnumerable as described in the summary</returns>
        public IEnumerable<IEnumerable<Tile>> GroupByColumns()
        {
            return from tile in tiles
                   group tile by tile.Column into col
                   orderby col.Key
                   select col;
        }
    }
}
