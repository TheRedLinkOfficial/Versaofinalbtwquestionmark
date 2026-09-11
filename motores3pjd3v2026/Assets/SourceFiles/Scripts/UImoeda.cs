using UnityEngine;
using TMPro;

public class UImoeda : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private int playerID = 1;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private string prefixo = "Moedas: ";

    private void OnEnable()
    {
        CoinEventManager.OnTotalCoinsChanged += AtualizarUI;
    }

    private void OnDisable()
    {
        CoinEventManager.OnTotalCoinsChanged -= AtualizarUI;
    }

    private void Start()
    {
        coinText.color = Color.yellow;

        int total = 0;

        if (playerID == 1)
        {
            total = CoinEventManager.GetPlayer1Coins();
        }
        else if (playerID == 2)
        {
            total = CoinEventManager.GetPlayer2Coins();
        }

        AtualizarUI(playerID, total);
    }

    private void AtualizarUI(int id, int total)
    {
        if (id != playerID)
            return;

        if (coinText != null)
        {
            coinText.text = $"{prefixo}{total}";
        }
    }

    public void AtualizarManual(int valor)
    {
        AtualizarUI(playerID, valor);
    }
}