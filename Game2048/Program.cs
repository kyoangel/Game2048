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
            PutNewValue();
            do
            {
                Display();
                if (CheckEndGame()) break;
                var input = GetUserInput();
                UpdateBoardByUserInput(input);
            }
            while (true); // use CTRL-C to break out of loop

            Console.WriteLine("Press any key to quit...");
            Console.Read();
        }

        private void UpdateBoardByUserInput(ConsoleKeyInfo input)
        {
            if (_directionLookup.ContainsKey(input.Key))
                Update(_directionLookup[input.Key]);
        }

        private bool CheckEndGame()
        {
            if (IsDead())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("YOU ARE DEAD!!!");
                Console.ResetColor();
                return true;
            }

            return false;
        }

        private static ConsoleKeyInfo GetUserInput()
        {
            Console.WriteLine("Use arrow keys to move the tiles. Press Ctrl-C to exit.");
            ConsoleKeyInfo input = Console.ReadKey(true); // BLOCKING TO WAIT FOR INPUT
            Console.WriteLine(input.Key.ToString());
            return input;
        }

        private static ConsoleColor GetNumberColor(ulong num)
        {
            return _consoleColors.ContainsKey(num) ? _consoleColors[num] : ConsoleColor.Red;
        }

        /// <summary>
        /// Updates the specified board.
        /// You shouldn't be dead at this point. We always check if you're dead at the end of the Update()
        ///
        /// </summary>
        /// <param name="board">The board.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        private static bool Update(ulong[,] board, Direction direction)
        {
            var boardHandler = CreateBoardHandler(board, IsAlongRow(direction), IsIncreasing(direction));
            return boardHandler.Handle();
        }

        private static BoardHandler CreateBoardHandler(ulong[,] board, bool isAlongRow, bool isIncreasing)
        {
            var boardHandler = new BoardHandler(board, isAlongRow, isIncreasing)
            {
                Drop = isIncreasing
                    ? new Func<int, int>(innerIndex => innerIndex - 1)
                    : new Func<int, int>(innerIndex => innerIndex + 1),
                ReverseDrop = isIncreasing
                    ? new Func<int, int>(innerIndex => innerIndex + 1)
                    : new Func<int, int>(innerIndex => innerIndex - 1),
                GetValue = isAlongRow
                    ? new Func<ulong[,], int, int, ulong>((x, i, j) => x[i, j])
                    : new Func<ulong[,], int, int, ulong>((x, i, j) => x[j, i]),
                SetValue = isAlongRow
                    ? new Action<ulong[,], int, int, ulong>((x, i, j, v) => x[i, j] = v)
                    : new Action<ulong[,], int, int, ulong>((x, i, j, v) => x[j, i] = v)
            };
            return boardHandler;
        }

        /// <summary>
        /// Determines should we process inner dimension in increasing index order?
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <returns>
        ///   <c>true</c> if the specified direction is increasing; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsIncreasing(Direction direction)
        {
            return direction == Direction.Left || direction == Direction.Up;
        }

        /// <summary>
        /// Determines drop along row or column.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <returns>
        ///   <c>true</c> if process inner along row; process inner along column, <c>false</c>.
        /// </returns>
        private static bool IsAlongRow(Direction direction)
        {
            return direction == Direction.Left || direction == Direction.Right;
        }

        private void Update(Direction dir)
        {
            if (Game.Update(this.Board, dir))
            {
                PutNewValue();
            }
            this.Score += BoardHandler.Score;
        }

        private bool IsDead()
        {
            foreach (Direction dir in new Direction[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right })
            {
                ulong[,] clone = (ulong[,])Board.Clone();
                if (Game.Update(clone, dir))
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
            DrawBoard();
            DrawScore();
            Console.WriteLine();
        }

        private void DrawScore()
        {
            Console.WriteLine("Score: {0}", this.Score);
        }

        private void DrawBoard()
        {
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
                ? (ulong)2
                : (ulong)4; // randomly pick 2 (with 95% chance) or 4 (rest of the chance)
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