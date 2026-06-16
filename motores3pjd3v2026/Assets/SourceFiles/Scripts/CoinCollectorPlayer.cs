using UnityEngine;
using System;

public class CoinCollectorPlayer: MonoBehaviour
{
    public static event Action<bool> OnInteractable;

    public static void Interactable(bool value)
    {
        
        OnInteractable?.Invoke(value);
    }
    
    public static event Action<Vector3> OnInteractPosition;

    public static void InteractPosition(Vector3 position)
    {
        OnInteractPosition?.Invoke(position);
    }
}
