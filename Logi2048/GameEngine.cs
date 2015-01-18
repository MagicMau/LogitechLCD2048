using System;
using System.Collections.Generic;
using System.Linq;

namespace Logi2048
{
    public enum Direction { Up, Down, Left, Right };

    /// <summary>
    /// Implements the rules of the game.
    /// 
    /// Responsible for manipulating the tile positions on the board.
    /// </summary>
    public class GameEngine
    {
        public const int GoalValue = 2048;
        public const int InitialTiles = 2;

        public int Score { get; private set; }

        /// <summary>
        /// New tiles placed on the board will be either a 2 or a 4.
        /// This constant represents the ratio of 2s to 4s as a percentage.
        /// </summary>
        private const float ChanceToGenerateTwo = 0.75f;

        private Board board;
        private Random random;

        public GameEngine(Board b)
        {
            board = b;
            random = new Random();
            Reset();
        }

        public void Reset()
        {
            foreach (var tile in board)
            {
                tile.Value = 0;
                tile.ClearMoveData();
            }

            Score = 0;

            for (int i = 0; i < InitialTiles; i++)
            {
                AddNewTile();
            }
        }

        public bool GameWon
        {
            get { return board.HighestValue == GoalValue; }
        }

        public bool GameLost
        {
            get 
            {
                return (!board.HasPositionsAvailable && !AreMovesAvailable());
            }
        }

        private void AddNewTile()
        {
            var positionsAvailable = board.GetAvailablePositions();
            int index = random.Next(positionsAvailable.Count());
            Tile tile = positionsAvailable.ElementAt(index);
            int value = (random.NextDouble() < ChanceToGenerateTwo) ? 2 : 4;

            tile.Value = value;
        }

        private bool AreMovesAvailable()
        {
            foreach (var tile in board)
            {
                for (int row = tile.Row + 1; row < board.Size ; row++)
                {
                    var tileToCheck = board[row, tile.Column];
                    
                    if (tileToCheck.Value == tile.Value)
                    {
                        return true;
                    }
                    else if (!tileToCheck.Empty)
                    {
                        break;
                    }
                }

                for (int col = tile.Column + 1 ; col < board.Size ; col++)
                {
                    var tileToCheck = board[tile.Row, col];

                    if (tileToCheck.Value == tile.Value)
                    {
                        return true;
                    }
                    else if (!tileToCheck.Empty)
                    {
                        break;
                    }
                }
            }

            return false;
        }

        public void Move(Direction dir)
        {
            foreach (var tile in board)
            {
                tile.ClearMoveData();
            }

            var groups = (dir == Direction.Right || dir == Direction.Left)
                ? board.GroupByRows()
                : board.GroupByColumns();

            foreach (var group in groups)
            {
                // Tiles should be considered starting from the edge they
                // are being moved towards. The default iteration is from
                // lowest index position to highest. When moving Right or
                // down, reverse the iterator to consider tiles in the
                // correct order.
                if (dir == Direction.Right || dir == Direction.Down)
                {
                    MoveOneTileGroup(group.Reverse());
                }
                else
                {
                    MoveOneTileGroup(group);
                }
            }
       
            // Only add a new tile if at least one tile was moved
            if (board.Any(t => (t.Moved || t.Merged)))
            {
                AddNewTile();
            }

            // Update score
            this.Score += (from Tile tile in board
                           where tile.Merged
                           select tile.Value).Sum();
        }

        private void MoveOneTileGroup(IEnumerable<Tile> tiles)
        {
            var previousTiles = new Stack<Tile>();

            foreach (var tile in tiles)
            {
                if (!tile.Empty)
                {
                    var newTile = FindFurthestAvailablePosition(previousTiles, tile.Value);

                    if (newTile != null)
                    {
                        tile.MoveTo(newTile);
                    }
                }

                previousTiles.Push(tile);
            }
        }

        private Tile FindFurthestAvailablePosition(Stack<Tile> tiles, int value)
        {
            Tile rval = null;

            foreach (var tile in tiles)
            {
                if (tile.Empty)
                {
                    rval = tile;
                }
                else if (tile.Value == value && !tile.Merged)
                {
                    return tile;
                }
                else
                {
                    break;
                }
            }

            return rval;
        }
    }
}
