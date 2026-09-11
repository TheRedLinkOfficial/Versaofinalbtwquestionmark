using UnityEngine;

public class Pickup : MonoBehaviour
{
    [Header("Effects")]
    public GameObject particleEffectPrefab;

    [Header("Motion Settings")]
    public float rotationSpeed = 100f;
    public float bobbingAmount = 0.1f;
    public float bobbingSpeed = 1f;

    private Vector3 startPosition;
    private float timer;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Rotação da estrela
        transform.Rotate(
            Vector3.up,
            rotationSpeed * Time.deltaTime,
            Space.World
        );

        // Movimento para cima e para baixo
        timer += Time.deltaTime * bobbingSpeed;

        float newY =
            startPosition.y +
            Mathf.Sin(timer) * bobbingAmount;

        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
        );
    }
}