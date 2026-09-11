using UnityEngine;
using System;

public class CoinCollectorPlayer : MonoBehaviour 
{
    // Evento disparado quando um jogador coleta uma moeda
    // Envia: ID do jogador + valor da moeda
    public static event Action<int, int> OnCoinCollected;

    // Evento para atualizar a UI
    // Envia: ID do jogador + total de moedas dele
    public static event Action<int, int> OnTotalCoinsChanged;

    // Total de moedas de cada jogador
    private static int _player1Coins = 0;
    private static int _player2Coins = 0;

    /// <summary>
    /// Adiciona moedas ao jogador correspondente.
    /// </summary>
    public static void AddCoins(int playerID, int amount)
    {
        if (playerID == 1)
        {
            _player1Coins += amount;

            OnCoinCollected?.Invoke(playerID, amount);
            OnTotalCoinsChanged?.Invoke(playerID, _player1Coins);

            Debug.Log(
                $"<color=yellow>[CoinEventManager]</color> " +
                $"Player 1 +{amount} moeda(s) | Total: {_player1Coins}"
            );
        }
        else if (playerID == 2)
        {
            _player2Coins += amount;

            OnCoinCollected?.Invoke(playerID, amount);
            OnTotalCoinsChanged?.Invoke(playerID, _player2Coins);

            Debug.Log(
                $"<color=yellow>[CoinEventManager]</color> " +
                $"Player 2 +{amount} moeda(s) | Total: {_player2Coins}"
            );
        }
    }

    public static int GetPlayer1Coins()
    {
        return _player1Coins;
    }

    public static int GetPlayer2Coins()
    {
        return _player2Coins;
    }

    /// <summary>
    /// Reseta as moedas dos dois jogadores.
    /// </summary>
    public static void ResetCoins()
    {
        _player1Coins = 0;
        _player2Coins = 0;

        OnTotalCoinsChanged?.Invoke(1, 0);
        OnTotalCoinsChanged?.Invoke(2, 0);
    }
}