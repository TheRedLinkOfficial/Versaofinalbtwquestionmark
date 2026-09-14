using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class doisplayersnoteclado : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    private void Awake()
    {
        if (Keyboard.current == null)
        {
            return;
        }
        InputUser.PerformPairingWithDevice(Keyboard.current, playerInput.user);
    }
}
