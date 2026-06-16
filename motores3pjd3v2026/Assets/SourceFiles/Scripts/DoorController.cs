using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Animator anim;

    private bool isOpen;

    private bool _isinteractable;

    private bool isInteractable
    {
        get { return _isinteractable; }
        set
        {
            _isinteractable = value;
            CoinCollectorPlayer.Interactable(_isinteractable);
        }
    }
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isInteractable)
        {
            isInteractable = true;
        
            InteractOM.OnInteract += AbrirFechar;
            CoinCollectorPlayer.InteractPosition(this.transform.position);
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isInteractable)
        {
            isInteractable = false;
            InteractOM.OnInteract -= AbrirFechar;
        }
    }

    private void AbrirFechar()
    {
        throw new System.NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
