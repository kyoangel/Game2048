using System;

namespace Game2048
{
    internal class BoardHandler
    {
        internal static Func<int, int> ReverseDrop;
        internal static Func<ulong[,], int, int, ulong> GetValue;
        internal static Func<int, int> Drop;
        internal static Action<ulong[,], int, int, ulong> SetValue;
        internal static ulong[,] Board;
        internal static ulong Score;
        private static bool _hasUpdated = false;
        public static int SecondAxisStart;
        public static int SecondAxisEnd;

        internal static bool Handle(int outerCount)
        {
            if (DelegatesNotReady())
            {
                return false;
            }

            Score = 0;
            _hasUpdated = false;
            TraverseBoard(outerCount);

            return _hasUpdated;
        }

        private static void TraverseBoard(int outerCount)
        {
            for (int i = 0; i < outerCount; i++)
            {
                for (int j = SecondAxisStart; InnerCondition(j); j = ReverseDrop(j))
                {
                    if (GetValue(Board, i, j) == 0)
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

        private static bool InnerCondition(int index)
        {
            return Math.Min(SecondAxisStart, SecondAxisEnd) <= index
                     && index <= Math.Max(SecondAxisStart, SecondAxisEnd);
        }

        private static void HandlwValues(int i, int j, int newJ)
        {
            if (IsCouldMerge(i, j, newJ))
                MergeValues(i, j, newJ);
            else
                UndoValues(i, j, newJ);
        }

        private static bool DelegatesNotReady()
        {
            return ReverseDrop == null || GetValue == null || Drop == null ||
                   SetValue == null;
        }

        private static bool IsStillProbing(int i, int newJ)
        {
            return InnerCondition(newJ) && GetValue(Board, i, newJ) == 0;
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
        private static void UndoValues(int i, int j, int newJ)
        {
            newJ = ReverseDrop(newJ);
            if (newJ != j)
            {
                _hasUpdated = true;
            }

            ulong value = GetValue(Board, i, j);
            SetValue(Board, i, j, 0);
            SetValue(Board, i, newJ, value);
        }

        private static bool IsCouldMerge(int i, int j, int newJ)
        {
            return InnerCondition(newJ) && GetValue(Board, i, newJ) == GetValue(Board, i, j);
        }

        private static void MergeValues(int i, int j, int newJ)
        {
            ulong newValue = GetValue(Board, i, newJ) * 2;
            SetValue(Board, i, newJ, newValue);
            SetValue(Board, i, j, 0);

            _hasUpdated = true;
            Score += newValue;
        }
    }
}