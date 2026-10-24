// Логика игры и ИИ на Minimax. Файл не зависит от консоли, поэтому его можно
// без изменений скопировать в Unity-проект (папка Assets/Scripts).

using System.Collections.Generic;

namespace MinimaxTicTacToe
{
    // Состояние игры: доска из 9 клеток и игрок, который сейчас ходит.
    public class GameState
    {
        public const char Human = 'X';
        public const char Ai = 'O';
        public const char Empty = ' ';

        public readonly char[] Board;
        public readonly char Current;

        public GameState(char first)
        {
            Board = new char[9];
            for (int i = 0; i < 9; i++) Board[i] = Empty;
            Current = first;
        }

        public GameState(char[] board, char current)
        {
            Board = board;
            Current = current;
        }

        public static char Other(char player)
        {
            return player == Human ? Ai : Human;
        }
    }

    public static class TicTacToe
    {
        public const int WinScore = 10;

        static readonly int[][] WinLines =
        {
            new[] { 0, 1, 2 }, new[] { 3, 4, 5 }, new[] { 6, 7, 8 },  // строки
            new[] { 0, 3, 6 }, new[] { 1, 4, 7 }, new[] { 2, 5, 8 },  // столбцы
            new[] { 0, 4, 8 }, new[] { 2, 4, 6 },                     // диагонали
        };

        // Счётчик просмотренных узлов дерева (для демонстрации работы алгоритма)
        public static long NodesVisited;

        // Все допустимые ходы — индексы пустых клеток.
        public static List<int> GetMoves(GameState state)
        {
            var moves = new List<int>();
            for (int i = 0; i < 9; i++)
                if (state.Board[i] == GameState.Empty) moves.Add(i);
            return moves;
        }

        // Возвращает новое состояние после хода (исходное не изменяется).
        public static GameState ApplyMove(GameState state, int move)
        {
            var board = (char[])state.Board.Clone();
            board[move] = state.Current;
            return new GameState(board, GameState.Other(state.Current));
        }

        // Возвращает победителя ('X' или 'O') или Empty, если победителя нет.
        public static char Winner(GameState state)
        {
            var b = state.Board;
            foreach (var line in WinLines)
            {
                if (b[line[0]] != GameState.Empty && b[line[0]] == b[line[1]] && b[line[1]] == b[line[2]])
                    return b[line[0]];
            }
            return GameState.Empty;
        }

        // Партия окончена: есть победитель или доска заполнена (ничья).
        public static bool IsTerminal(GameState state)
        {
            if (Winner(state) != GameState.Empty) return true;
            foreach (char cell in state.Board)
                if (cell == GameState.Empty) return false;
            return true;
        }

        // Оценка терминального состояния с точки зрения ИИ.
        public static int Evaluate(GameState state)
        {
            char w = Winner(state);
            if (w == GameState.Ai) return WinScore;      // победа ИИ: +10
            if (w == GameState.Human) return -WinScore;  // победа человека: -10
            return 0;                                    // ничья: 0
        }

        // Рекурсивный Minimax с полным перебором дерева.
        //
        // depth — сколько ходов уже сделано от текущей позиции. Его вычитаем из
        // оценки победы, чтобы ИИ выигрывал как можно быстрее и проигрывал (если бы
        // такое было возможно) как можно позже. Знак оценки при этом не меняется:
        // победа > 0, поражение < 0, ничья = 0.
        public static int Minimax(GameState state, int depth, bool isMaximizing)
        {
            NodesVisited++;

            if (IsTerminal(state))
            {
                int score = Evaluate(state);
                if (score > 0) return score - depth;
                if (score < 0) return score + depth;
                return 0;
            }

            if (isMaximizing)  // ход ИИ — берём максимум
            {
                int best = int.MinValue;
                foreach (int move in GetMoves(state))
                    best = System.Math.Max(best, Minimax(ApplyMove(state, move), depth + 1, false));
                return best;
            }
            else               // ход человека — берём минимум
            {
                int best = int.MaxValue;
                foreach (int move in GetMoves(state))
                    best = System.Math.Min(best, Minimax(ApplyMove(state, move), depth + 1, true));
                return best;
            }
        }

        // Выбирает лучший ход ИИ для текущей позиции.
        // В scores записываются оценки всех ходов — они нужны для вывода анализа.
        public static int BestMove(GameState state, out SortedDictionary<int, int> scores)
        {
            NodesVisited = 0;
            scores = new SortedDictionary<int, int>();

            int best = -1;
            foreach (int move in GetMoves(state))
            {
                // после хода ИИ ходит человек — минимизирующий уровень
                scores[move] = Minimax(ApplyMove(state, move), 1, false);
                if (best == -1 || scores[move] > scores[best]) best = move;
            }
            return best;
        }
    }
}
