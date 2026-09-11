using UnityEngine;
using StarterAssets;

public class PickUpCoin : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ThirdPersonController player =
                other.GetComponent<ThirdPersonController>();

            if (player != null)
            {
                int playerID = player.playerID;

                CoinEventManager.AddCoins(playerID, coinValue);

                player.IncreaseSpeed();

                Destroy(gameObject);
            }
        }
    }
}