using UnityEngine;
using System;

public static class CoinEventManager
{
    public static event Action<int, int> OnCoinCollected;
    public static event Action<int, int> OnTotalCoinsChanged;

    private static int _player1Coins = 0;
    private static int _player2Coins = 0;

    public static void AddCoins(int playerID, int amount)
    {
        if (playerID == 1)
        {
            _player1Coins += amount;

            OnCoinCollected?.Invoke(playerID, amount);
            OnTotalCoinsChanged?.Invoke(playerID, _player1Coins);

            Debug.Log(
                $"[CoinEventManager] Player 1 +{amount} | Total: {_player1Coins}"
            );
        }
        else if (playerID == 2)
        {
            _player2Coins += amount;

            OnCoinCollected?.Invoke(playerID, amount);
            OnTotalCoinsChanged?.Invoke(playerID, _player2Coins);

            Debug.Log(
                $"[CoinEventManager] Player 2 +{amount} | Total: {_player2Coins}"
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

    public static void ResetCoins()
    {
        _player1Coins = 0;
        _player2Coins = 0;

        OnTotalCoinsChanged?.Invoke(1, 0);
        OnTotalCoinsChanged?.Invoke(2, 0);
    }
}