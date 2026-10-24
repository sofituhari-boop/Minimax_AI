// Консольный режим «человек против ИИ».
//
// Запуск:  dotnet run            — игра
//          dotnet run -- --test  — проверка, что ИИ никогда не проигрывает

using System;
using System.Collections.Generic;
using System.Text;

namespace MinimaxTicTacToe
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            if (args.Length > 0 && args[0] == "--test") return SelfTest() ? 0 : 1;
            Play();
            return 0;
        }

        static void PrintBoard(GameState state)
        {
            for (int r = 0; r < 3; r++)
            {
                var row = new List<string>();
                for (int c = 0; c < 3; c++)
                {
                    int i = r * 3 + c;
                    char cell = state.Board[i];
                    row.Add(cell != GameState.Empty ? cell.ToString() : (i + 1).ToString());
                }
                Console.WriteLine(" " + string.Join(" | ", row));
                if (r < 2) Console.WriteLine("---+---+---");
            }
        }

        static string Describe(int score)
        {
            if (score > 0) return "победа ИИ";
            if (score < 0) return "победа человека";
            return "ничья";
        }

        static int AskHumanMove(GameState state)
        {
            var moves = TicTacToe.GetMoves(state);
            while (true)
            {
                Console.Write("Ваш ход (номер клетки 1-9): ");
                string raw = Console.ReadLine();
                if (raw == null || raw.Trim() == "q")
                {
                    Console.WriteLine("\nВыход.");
                    Environment.Exit(0);
                }
                if (int.TryParse(raw, out int cell) && moves.Contains(cell - 1)) return cell - 1;
                Console.WriteLine("Некорректный ход, выберите свободную клетку.");
            }
        }

        static void Play()
        {
            Console.WriteLine("Крестики-нолики: вы — X, ИИ — O.  (q — выход)");
            Console.Write("Ходить первым? [Y/n]: ");
            string answer = (Console.ReadLine() ?? "").Trim().ToLower();
            var state = new GameState(answer == "n" || answer == "н" ? GameState.Ai : GameState.Human);

            while (!TicTacToe.IsTerminal(state))
            {
                Console.WriteLine();
                PrintBoard(state);
                Console.WriteLine();
                if (state.Current == GameState.Human)
                {
                    state = TicTacToe.ApplyMove(state, AskHumanMove(state));
                }
                else
                {
                    int move = TicTacToe.BestMove(state, out var scores);
                    Console.WriteLine($"ИИ анализирует ходы (просмотрено узлов дерева: {TicTacToe.NodesVisited}):");
                    foreach (var pair in scores)
                    {
                        string mark = pair.Key == move ? "  <- выбран" : "";
                        Console.WriteLine($"  клетка {pair.Key + 1}: оценка {pair.Value:+0;-0;+0} ({Describe(pair.Value)}){mark}");
                    }
                    Console.WriteLine($"ИИ ходит в клетку {move + 1}");
                    state = TicTacToe.ApplyMove(state, move);
                }
            }

            Console.WriteLine();
            PrintBoard(state);
            Console.WriteLine();
            char w = TicTacToe.Winner(state);
            if (w == GameState.Ai) Console.WriteLine("Победил ИИ.");
            else if (w == GameState.Human) Console.WriteLine("Вы победили!");
            else Console.WriteLine("Ничья.");
        }

        // -------------------------------------------------------------------
        // Проверка: перебираем ВСЕ возможные стратегии человека
        // -------------------------------------------------------------------

        static int games, aiWins, draws, humanWins;

        static void Explore(GameState state)
        {
            if (TicTacToe.IsTerminal(state))
            {
                games++;
                char w = TicTacToe.Winner(state);
                if (w == GameState.Ai) aiWins++;
                else if (w == GameState.Human) humanWins++;
                else draws++;
                return;
            }
            if (state.Current == GameState.Human)
            {
                foreach (int move in TicTacToe.GetMoves(state))  // человек пробует все варианты
                    Explore(TicTacToe.ApplyMove(state, move));
            }
            else
            {
                int move = TicTacToe.BestMove(state, out _);     // ИИ отвечает лучшим ходом
                Explore(TicTacToe.ApplyMove(state, move));
            }
        }

        static bool SelfTest()
        {
            Explore(new GameState(GameState.Human));
            Explore(new GameState(GameState.Ai));

            Console.WriteLine($"Сыграно партий (все ходы человека, оба варианта очерёдности): {games}");
            Console.WriteLine($"  побед ИИ:       {aiWins}");
            Console.WriteLine($"  ничьих:         {draws}");
            Console.WriteLine($"  побед человека: {humanWins}");
            Console.WriteLine(humanWins == 0 ? "OK: ИИ ни разу не проиграл." : "ОШИБКА: ИИ проиграл!");
            return humanWins == 0;
        }
    }
}
