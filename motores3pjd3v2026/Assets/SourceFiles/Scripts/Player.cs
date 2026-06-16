using UnityEngine;

public class Player : MonoBehaviour
{
    public void NotifyCoinPickup(int amount)
    {
  
        Debug.Log($"<color=green>[Player]</color> Coletou {amount} moeda(s).");

     
    }

}
