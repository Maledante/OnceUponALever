using UnityEngine;

public class LantInteract : MonoBehaviour
{
    [Header("Setări lanț")]
    public float minY = -2f;              // cât de jos poate fi tras (față de poziția inițială)
    public float maxY = 0f;               // cât de sus (de obicei 0 = poziția inițială)
    public float returnSpeed = 2f;        // viteza de revenire în sus
    public float triggerDistance = -1.8f; // punctul la care se declanșează acțiunea

    private bool isDragging = false;
    private Vector3 mouseStartPos;
    private Vector3 chainStartPos;
    private bool hasTriggered = false;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        chainStartPos = transform.position; // salvează poziția curentă din scenă ca punct de start
    }

    void OnMouseDown()
    {
        isDragging = true;
        mouseStartPos = cam.ScreenToWorldPoint(Input.mousePosition);
        hasTriggered = false;
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            float deltaY = mouseWorld.y - mouseStartPos.y;

            // Mișcare doar pe axa Y (în jos)
            float newY = Mathf.Clamp(chainStartPos.y + deltaY, chainStartPos.y + minY, chainStartPos.y + maxY);

            transform.position = new Vector3(chainStartPos.x, newY, chainStartPos.z);

            // Declanșează acțiunea când e tras suficient de jos
            if (!hasTriggered && newY <= chainStartPos.y + triggerDistance)
            {
                hasTriggered = true;
                OnChainPulled();
            }
        }
        else
        {
            // Revine automat la poziția inițială
            transform.position = Vector3.Lerp(
                transform.position,
                chainStartPos,
                Time.deltaTime * returnSpeed
            );
        }
    }

    private void OnChainPulled()
    {
        Debug.Log("Lanțul a fost tras complet!");
        // aici poți apela ceva gen:
        // FindObjectOfType<UsaController>().DeschideUsa();
        // sau un eveniment public, dacă vrei să-l legi din Inspector
        FindFirstObjectByType<CortineMovement>()?.InchideCopertine();
    }
}
