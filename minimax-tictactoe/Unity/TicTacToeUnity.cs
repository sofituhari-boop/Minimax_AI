// Простая графическая версия для Unity (необязательно).
//
// 1. Скопируйте TicTacToe.cs и этот файл в Assets/Scripts вашего Unity-проекта.
// 2. Создайте пустой GameObject на сцене и добавьте к нему компонент TicTacToeUnity.
// 3. Нажмите Play — поле рисуется через OnGUI, ничего настраивать не нужно.

using UnityEngine;
using MinimaxTicTacToe;

public class TicTacToeUnity : MonoBehaviour
{
    GameState state = new GameState(GameState.Human);
    string status = "Ваш ход (вы — X)";

    void OnGUI()
    {
        const int size = 100;
        var cellStyle = new GUIStyle(GUI.skin.button) { fontSize = 48 };
        var textStyle = new GUIStyle(GUI.skin.label) { fontSize = 20 };

        for (int i = 0; i < 9; i++)
        {
            var rect = new Rect(20 + (i % 3) * size, 20 + (i / 3) * size, size, size);
            char cell = state.Board[i];
            bool canClick = cell == GameState.Empty && state.Current == GameState.Human && !TicTacToe.IsTerminal(state);
            if (GUI.Button(rect, cell.ToString(), cellStyle) && canClick)
            {
                state = TicTacToe.ApplyMove(state, i);
                if (!TicTacToe.IsTerminal(state))
                {
                    int move = TicTacToe.BestMove(state, out _);
                    state = TicTacToe.ApplyMove(state, move);
                }
                UpdateStatus();
            }
        }

        GUI.Label(new Rect(20, 30 + 3 * size, 400, 30), status, textStyle);
        if (GUI.Button(new Rect(20, 70 + 3 * size, 3 * size, 40), "Новая игра"))
        {
            state = new GameState(GameState.Human);
            status = "Ваш ход (вы — X)";
        }
    }

    void UpdateStatus()
    {
        char w = TicTacToe.Winner(state);
        if (w == GameState.Ai) status = "Победил ИИ";
        else if (w == GameState.Human) status = "Вы победили!";
        else if (TicTacToe.IsTerminal(state)) status = "Ничья";
        else status = "Ваш ход";
    }
}
