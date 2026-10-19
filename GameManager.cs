using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public Button[] cells;
    public TextMeshProUGUI statusText;

    private char[] board = new char[9];
    private bool isHumanTurn = true;
    private bool gameOver = false;

    private const char EMPTY = ' ';
    private const char HUMAN = 'X';
    private const char AI = 'O';

    void Start()
    {
        for (int i = 0; i < 9; i++) board[i] = EMPTY;

        // Проверка, что массив кнопок существует
        if (cells == null || cells.Length != 9)
        {
            Debug.LogError("КРИТИЧЕСКАЯ ОШИБКА: В инспекторе в массив Cells не добавлены 9 кнопок!");
            return;
        }

        for (int i = 0; i < cells.Length; i++)
        {
            // Проверка пустых слотов в массиве
            if (cells[i] == null)
            {
                Debug.LogError($"КРИТИЧЕСКАЯ ОШИБКА: В массиве Cells слот {i} пустой (None)! Перетащи туда кнопку.");
                continue;
            }

            int index = i;

            // Проверка текста внутри кнопки
            var btnText = cells[i].GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = "";
            }
            else
            {
                Debug.LogError($"ОШИБКА: В кнопке '{cells[i].name}' (слот {i}) нет компонента TextMeshProUGUI!");
            }

            cells[i].onClick.AddListener(() => OnCellClicked(index));
        }

        if (statusText == null)
            Debug.LogWarning("ВНИМАНИЕ: Поле Status Text пустое.");
        else
            UpdateStatus("Ваш ход (X)");
    }

    void OnCellClicked(int index)
    {
        if (gameOver || !isHumanTurn || board[index] != EMPTY) return;
        MakeMove(index, HUMAN);
        if (CheckGameEnd()) return;

        isHumanTurn = false;
        UpdateStatus("ИИ думает...");
        Invoke(nameof(AIMove), 0.5f);
    }

    void AIMove()
    {
        if (gameOver) return;
        int move = GetBestMove();
        if (move == -1) return;
        MakeMove(move, AI);
        CheckGameEnd();
        if (!gameOver)
        {
            isHumanTurn = true;
            UpdateStatus("Ваш ход (X)");
        }
    }

    void MakeMove(int index, char player)
    {
        board[index] = player;
        var btnText = cells[index].GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null) btnText.text = player.ToString();
        cells[index].interactable = false;
    }

    bool CheckGameEnd()
    {
        char winner = CheckWinner(board);
        if (winner == AI) { gameOver = true; UpdateStatus("ИИ победил! 🤖"); DisableAllButtons(); return true; }
        if (winner == HUMAN) { gameOver = true; UpdateStatus("Вы победили! 🎉"); DisableAllButtons(); return true; }
        if (IsBoardFull(board)) { gameOver = true; UpdateStatus("Ничья. 🤝"); DisableAllButtons(); return true; }
        return false;
    }

    void DisableAllButtons()
    {
        foreach (var b in cells) { if (b != null) b.interactable = false; }
    }

    void UpdateStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    //                    АЛГОРИТМ MINIMAX
    int GetBestMove()
    {
        int bestScore = int.MinValue;
        int bestMove = -1;
        for (int i = 0; i < 9; i++)
        {
            if (board[i] != EMPTY) continue;
            board[i] = AI;
            int score = Minimax(board, 0, false);
            board[i] = EMPTY;
            if (score > bestScore) { bestScore = score; bestMove = i; }
        }
        return bestMove;
    }

    int Minimax(char[] state, int depth, bool isMaximizing)
    {
        char winner = CheckWinner(state);
        if (winner == AI) return 10 - depth;
        if (winner == HUMAN) return depth - 10;
        if (IsBoardFull(state)) return 0;

        if (isMaximizing)
        {
            int best = int.MinValue;
            for (int i = 0; i < 9; i++)
            {
                if (state[i] != EMPTY) continue;
                state[i] = AI;
                best = Mathf.Max(best, Minimax(state, depth + 1, false));
                state[i] = EMPTY;
            }
            return best;
        }
        else
        {
            int best = int.MaxValue;
            for (int i = 0; i < 9; i++)
            {
                if (state[i] != EMPTY) continue;
                state[i] = HUMAN;
                best = Mathf.Min(best, Minimax(state, depth + 1, true));
                state[i] = EMPTY;
            }
            return best;
        }
    }

    char CheckWinner(char[] s)
    {
        int[,] lines = new int[8, 3] { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 }, { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 }, { 0, 4, 8 }, { 2, 4, 6 } };
        for (int i = 0; i < 8; i++)
        {
            int a = lines[i, 0], b = lines[i, 1], c = lines[i, 2];
            if (s[a] != EMPTY && s[a] == s[b] && s[b] == s[c]) return s[a];
        }
        return EMPTY;
    }

    bool IsBoardFull(char[] s)
    {
        foreach (char c in s) if (c == EMPTY) return false;
        return true;
    }
}
