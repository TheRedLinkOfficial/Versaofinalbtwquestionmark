using UnityEngine;

public class PickUpCoin : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;
    


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CoinEventManager.AddCoins(coinValue);

            

            Destroy(gameObject);
        }
    }
}
