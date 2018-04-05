using System;

namespace Game2048
{
    internal class BoardHandler
    {
        internal Func<int, int> ReverseDrop;
        internal Func<ulong[,], int, int, ulong> GetValue;
        internal Func<int, int> Drop;
        internal Action<ulong[,], int, int, ulong> SetValue;
        private ulong[,] _board;
        internal static ulong Score;
        private bool _hasUpdated = false;
        private int _secondAxisStart;
        private int _secondAxisEnd;
        private int _firstAxisLength;

        public BoardHandler(ulong[,] board, bool isAlongRow, bool isIncreasing)
        {
            _board = board;
            var nRows = board.GetLength(0);
            var nCols = board.GetLength(1);
            _firstAxisLength = isAlongRow ? nRows : nCols;
            int secondAxisLength = isAlongRow ? nCols : nRows;
            _secondAxisStart = isIncreasing ? 0 : secondAxisLength - 1;
            _secondAxisEnd = isIncreasing ? secondAxisLength - 1 : 0;
        }

        internal bool Handle()
        {
            if (DelegatesNotReady())
            {
                return false;
            }

            Score = 0;
            _hasUpdated = false;
            TraverseBoard();

            return _hasUpdated;
        }

        private void TraverseBoard()
        {
            for (int i = 0; i < _firstAxisLength; i++)
            {
                for (int j = _secondAxisStart; InnerCondition(j); j = ReverseDrop(j))
                {
                    if (GetValue(_board, i, j) == 0)
                    {
                        continue;
                    }

                    int newJ = j;
                    do
                    {
                        newJ = Drop(newJ);
                    } while (IsStillProbing(i, newJ));

                    HandlwValues(i, j, newJ);
                }
            }
        }

        private bool InnerCondition(int index)
        {
            return Math.Min(_secondAxisStart, _secondAxisEnd) <= index
                     && index <= Math.Max(_secondAxisStart, _secondAxisEnd);
        }

        private void HandlwValues(int i, int j, int newJ)
        {
            if (IsCouldMerge(i, j, newJ))
                MergeValues(i, j, newJ);
            else
                UndoValues(i, j, newJ);
        }

        private bool DelegatesNotReady()
        {
            return ReverseDrop == null || GetValue == null || Drop == null ||
                   SetValue == null;
        }

        private bool IsStillProbing(int i, int newJ)
        {
            return InnerCondition(newJ) && GetValue(_board, i, newJ) == 0;
        }

        /// <summary>
        /// Reached the boundary OR...
        /// we hit a node with different value OR...
        /// we hit a node with same value BUT a previous merge had occurred
        /// Simply stack along
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="j">The j.</param>
        /// <param name="newJ">The new j.</param>
        private void UndoValues(int i, int j, int newJ)
        {
            newJ = ReverseDrop(newJ);
            if (newJ != j)
            {
                _hasUpdated = true;
            }

            ulong value = GetValue(_board, i, j);
            SetValue(_board, i, j, 0);
            SetValue(_board, i, newJ, value);
        }

        private bool IsCouldMerge(int i, int j, int newJ)
        {
            return InnerCondition(newJ) && GetValue(_board, i, newJ) == GetValue(_board, i, j);
        }

        private void MergeValues(int i, int j, int newJ)
        {
            ulong newValue = GetValue(_board, i, newJ) * 2;
            SetValue(_board, i, newJ, newValue);
            SetValue(_board, i, j, 0);

            _hasUpdated = true;
            Score += newValue;
        }
    }
}