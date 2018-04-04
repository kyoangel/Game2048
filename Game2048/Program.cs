using System;
using System.Collections.Generic;

namespace Game2048
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Game game = new Game();
            game.Run();
        }
    }

    internal class Game
    {
        public ulong Score { get; private set; }
        public ulong[,] Board { get; private set; }

        private readonly int nRows;
        private readonly int nCols;

        private static Dictionary<ulong, ConsoleColor> _consoleColors = new Dictionary<ulong, ConsoleColor>()
        {
            { 0, ConsoleColor.DarkGray},
            { 2, ConsoleColor.Cyan},
            { 4, ConsoleColor.Magenta},
            { 8, ConsoleColor.Red},
            { 16, ConsoleColor.Green},
            { 32, ConsoleColor.Yellow},
            { 64, ConsoleColor.Yellow},
            { 128, ConsoleColor.DarkCyan},
            { 256, ConsoleColor.Cyan},
            { 512, ConsoleColor.DarkMagenta},
            { 1024, ConsoleColor.Magenta},
        };

        private Dictionary<ConsoleKey, Direction> _directionLookup = new Dictionary<ConsoleKey, Direction>()
        {
            {ConsoleKey.UpArrow, Direction.Up },
            {ConsoleKey.DownArrow, Direction.Down },
            {ConsoleKey.LeftArrow, Direction.Left },
            {ConsoleKey.RightArrow, Direction.Right },
        };

        private static Random _random = new Random();

        public Game()
        {
            this.Board = new ulong[4, 4];
            this.nRows = this.Board.GetLength(0);
            this.nCols = this.Board.GetLength(1);
            this.Score = 0;
        }

        public void Run()
        {
            bool hasUpdated = true;
            do
            {
                if (hasUpdated)
                {
                    PutNewValue();
                }

                Display();

                if (IsDead())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("YOU ARE DEAD!!!");
                    Console.ResetColor();
                    break;
                }

                Console.WriteLine("Use arrow keys to move the tiles. Press Ctrl-C to exit.");
                ConsoleKeyInfo input = Console.ReadKey(true); // BLOCKING TO WAIT FOR INPUT
                Console.WriteLine(input.Key.ToString());

                hasUpdated = _directionLookup.ContainsKey(input.Key) && Update(_directionLookup[input.Key]);
            }
            while (true); // use CTRL-C to break out of loop

            Console.WriteLine("Press any key to quit...");
            Console.Read();
        }

        private static ConsoleColor GetNumberColor(ulong num)
        {
            return _consoleColors.ContainsKey(num) ? _consoleColors[num] : ConsoleColor.Red;
        }

        private static bool Update(ulong[,] board, Direction direction, out ulong score)
        {
            int nRows = board.GetLength(0);
            int nCols = board.GetLength(1);

            score = 0;
            bool hasUpdated = false;

            // You shouldn't be dead at this point. We always check if you're dead at the end of the Update()

            // Drop along row or column? true: process inner along row; false: process inner along column
            bool isAlongRow = direction == Direction.Left || direction == Direction.Right;

            // Should we process inner dimension in increasing index order?
            bool isIncreasing = direction == Direction.Left || direction == Direction.Up;

            int outterCount = isAlongRow ? nRows : nCols;
            int innerCount = isAlongRow ? nCols : nRows;
            int innerStart = isIncreasing ? 0 : innerCount - 1;
            int innerEnd = isIncreasing ? innerCount - 1 : 0;

            Func<int, int> drop = isIncreasing
                ? new Func<int, int>(innerIndex => innerIndex - 1)
                : new Func<int, int>(innerIndex => innerIndex + 1);

            Func<int, int> reverseDrop = isIncreasing
                ? new Func<int, int>(innerIndex => innerIndex + 1)
                : new Func<int, int>(innerIndex => innerIndex - 1);

            Func<ulong[,], int, int, ulong> getValue = isAlongRow
                ? new Func<ulong[,], int, int, ulong>((x, i, j) => x[i, j])
                : new Func<ulong[,], int, int, ulong>((x, i, j) => x[j, i]);

            Action<ulong[,], int, int, ulong> setValue = isAlongRow
                ? new Action<ulong[,], int, int, ulong>((x, i, j, v) => x[i, j] = v)
                : new Action<ulong[,], int, int, ulong>((x, i, j, v) => x[j, i] = v);

            Func<int, bool> innerCondition = index => Math.Min(innerStart, innerEnd) <= index && index <= Math.Max(innerStart, innerEnd);

            for (int i = 0; i < outterCount; i++)
            {
                for (int j = innerStart; innerCondition(j); j = reverseDrop(j))
                {
                    if (getValue(board, i, j) == 0)
                    {
                        continue;
                    }

                    int newJ = j;
                    do
                    {
                        newJ = drop(newJ);
                    }
                    // Continue probing along as long as we haven't hit the boundary and the new position isn't occupied
                    while (innerCondition(newJ) && getValue(board, i, newJ) == 0);

                    if (innerCondition(newJ) && getValue(board, i, newJ) == getValue(board, i, j))
                    {
                        // We did not hit the canvas boundary (we hit a node) AND no previous merge occurred AND the nodes' values are the same
                        // Let's merge
                        ulong newValue = getValue(board, i, newJ) * 2;
                        setValue(board, i, newJ, newValue);
                        setValue(board, i, j, 0);

                        hasUpdated = true;
                        score += newValue;
                    }
                    else
                    {
                        // Reached the boundary OR...
                        // we hit a node with different value OR...
                        // we hit a node with same value BUT a prevous merge had occurred
                        //
                        // Simply stack along
                        newJ = reverseDrop(newJ); // reverse back to its valid position
                        if (newJ != j)
                        {
                            // there's an update
                            hasUpdated = true;
                        }

                        ulong value = getValue(board, i, j);
                        setValue(board, i, j, 0);
                        setValue(board, i, newJ, value);
                    }
                }
            }

            return hasUpdated;
        }

        private bool Update(Direction dir)
        {
            ulong score;
            bool isUpdated = Game.Update(this.Board, dir, out score);
            this.Score += score;
            return isUpdated;
        }

        private bool IsDead()
        {
            ulong score;
            foreach (Direction dir in new Direction[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right })
            {
                ulong[,] clone = (ulong[,])Board.Clone();
                if (Game.Update(clone, dir, out score))
                {
                    return false;
                }
            }

            return true;
        }

        private void Display()
        {
            Console.Clear();
            Console.WriteLine();
            for (int i = 0; i < nRows; i++)
            {
                for (int j = 0; j < nCols; j++)
                {
                    Console.ForegroundColor = GetNumberColor(Board[i, j]);
                    Console.Write($"{Board[i, j],6}");
                    Console.ResetColor();
                }

                Console.WriteLine();
                Console.WriteLine();
            }

            Console.WriteLine("Score: {0}", this.Score);
            Console.WriteLine();
        }

        private void PutNewValue()
        {
            var emptySlots = FindAllEmptySlots();
            SetNewValueToBoard(emptySlots, GetRandomSlot(emptySlots));
        }

        private void SetNewValueToBoard(List<Tuple<int, int>> emptySlots, int slot)
        {
            Board[emptySlots[slot].Item1, emptySlots[slot].Item2] = GetRandomNewValue();
        }

        private List<Tuple<int, int>> FindAllEmptySlots()
        {
            List<Tuple<int, int>> emptySlots = new List<Tuple<int, int>>();
            for (int iRow = 0; iRow < nRows; iRow++)
            {
                for (int iCol = 0; iCol < nCols; iCol++)
                {
                    if (Board[iRow, iCol] == 0)
                    {
                        emptySlots.Add(new Tuple<int, int>(iRow, iCol));
                    }
                }
            }

            return emptySlots;
        }

        private static int GetRandomSlot(List<Tuple<int, int>> emptySlots)
        {
            return _random.Next(0, emptySlots.Count);
        }

        private static ulong GetRandomNewValue()
        {
            ulong value = _random.Next(0, 100) < 95
                ? (ulong) 2
                : (ulong) 4; // randomly pick 2 (with 95% chance) or 4 (rest of the chance)
            return value;
        }

        #region Utility Classes

        private enum Direction
        {
            Up,
            Down,
            Right,
            Left,
        }

        #endregion Utility Classes
    }
}