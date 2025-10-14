using UnityEngine;

// Acest script gestionează interacțiunea cu sfoara (rope), permițând tragerea în jos și revenirea.
public class RopeInteract : MonoBehaviour {
    [Header("Setări lanț")]
    public float minY = -2f; // Limita inferioară Y relativă.
    public float maxY = 0f; // Limita superioară Y relativă.
    public float returnSpeed = 2f; // Viteza de revenire.
    public float triggerDistance = -1.8f; // Distanța la care se declanșează acțiunea.

    private bool isDragging = false; // Flag dacă este trasă.
    private Vector3 mouseStartPos; // Poziție inițială mouse.
    private Vector3 chainStartPos; // Poziție inițială sfoară.
    private bool hasTriggered = false; // Flag dacă a fost declanșat.
    private Camera cam; // Referință la camera principală.

    private void Start() {
        // Inițializează camera și poziția inițială.
        cam = Camera.main;
        chainStartPos = transform.position;
    }

    private void OnMouseDown() {
        // Verifică dacă este activat și cameră există.
        if (!enabled || cam == null)
            return;

        // Începe tragerea.
        isDragging = true;
        mouseStartPos = cam.ScreenToWorldPoint(Input.mousePosition);
        hasTriggered = false;
    }

    private void OnMouseUp() {
        // Oprește tragerea.
        isDragging = false;
    }

    private void Update() {
        if (isDragging) {
            // Dacă cameră lipsește, oprește.
            if (cam == null) {
                isDragging = false;
                return;
            }

            // Calculează noua poziție Y clampată.
            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            float deltaY = mouseWorld.y - mouseStartPos.y;
            float newY = Mathf.Clamp(chainStartPos.y + deltaY, chainStartPos.y + minY, chainStartPos.y + maxY);
            transform.position = new Vector3(chainStartPos.x, newY, chainStartPos.z);

            // Declanșează dacă a ajuns la distanța necesară.
            if (!hasTriggered && newY <= chainStartPos.y + triggerDistance) {
                hasTriggered = true;
                OnChainPulled();
            }
        }
        else {
            // Revine lin la poziția inițială.
            transform.position = Vector3.Lerp(transform.position, chainStartPos, Time.deltaTime * returnSpeed);
        }
    }

    private void OnChainPulled() {
        // Notifică GameManager dacă există.
        if (GameManager.Instance != null) {
            GameManager.Instance.OnRopePulled();
        }
    }

    public void Reset() {
        // Resetează flag-uri și poziție.
        hasTriggered = false;
        transform.position = chainStartPos;
    }
}