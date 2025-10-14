using UnityEngine;
using UnityEngine.SceneManagement;

// Acest script gestionează interacțiunea cu manetele, permițând rotația prin drag și revenirea.
// Detectează scena și declanșează acțiuni specifice (Menu sau Game).
public class LeverInteract : MonoBehaviour {
    [Header("Setări Pivot")]
    public Transform pivot; // Pivotul de rotație.

    [Header("Setări Rotație")]
    public float minAngle = -180f; // Unghi minim (jos).
    public float maxAngle = 0f; // Unghi maxim (sus).
    public float sensitivity = 180f; // Sensibilitate drag.

    [Header("Setări Revenire")]
    public float returnDuration = 1f; // Durata revenirii.

    [Header("Main Menu Settings")]
    public string leverType = "Play"; // Tip pentru MainMenu: "Play", "Options", "Tutorial".

    private float initialAngle = 0f; // Unghi inițial.
    private bool isReturning = false; // Flag revenire.
    private float startTime; // Timp start revenire.
    private float startAngle; // Unghi start revenire.
    private bool isDragging = false; // Flag drag.
    private Camera cam; // Camera principală.
    private float initialMouseY; // Y inițial mouse.
    private bool hasTriggered = false; // Flag declanșat.

    private void Start() {
        // Inițializează camera și poziția la pivot.
        cam = Camera.main;
        if (pivot != null) {
            transform.position = pivot.position;
        }
    }

    private void OnMouseDown() {
        // Verifică activat și cameră.
        if (!enabled || cam == null)
            return;

        // Începe drag.
        isDragging = true;
        isReturning = false;
        initialMouseY = cam.ScreenToWorldPoint(Input.mousePosition).y;
    }

    private void OnMouseUp() {
        // Oprește drag și începe revenire dacă nu deja.
        isDragging = false;
        if (!isReturning) {
            startAngle = transform.eulerAngles.x;
            if (startAngle > 180f) startAngle -= 360f;
            startTime = Time.time;
            isReturning = true;
        }
    }

    private void Update() {
        if (isDragging) {
            // Dacă cameră lipsește, oprește.
            if (cam == null) {
                isDragging = false;
                return;
            }

            // Calculează și aplică rotație clampată.
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            float deltaY = mouseWorldPos.y - initialMouseY;
            float angle = startAngle - deltaY * sensitivity;
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
            transform.rotation = Quaternion.Euler(angle, 0f, 0f);

            // Declanșează doar dacă aproape de minAngle (prag strict <1f).
            if (!hasTriggered && Mathf.Abs(angle - minAngle) < 1f) {
                hasTriggered = true;
                TriggerLeverAction();
            }
        }
        else if (isReturning) {
            // Revenire lin cu easing.
            float elapsed = Time.time - startTime;
            float t = Mathf.Clamp01(elapsed / returnDuration);
            t = EaseOutCubic(t);
            float newAngle = Mathf.LerpAngle(startAngle, initialAngle, t);
            transform.rotation = Quaternion.Euler(newAngle, 0f, 0f);

            // Oprește revenire la final.
            if (t >= 1f) {
                isReturning = false;
            }
        }
    }

    private void TriggerLeverAction() {
        // Detectează scena și declanșează acțiune corespunzătoare.
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu") {
            if (MenuManager.Instance != null) {
                MenuManager.Instance.OnLeverPulled(leverType);
            }
        }
        else {
            if (GameManager.Instance != null) {
                GameManager.Instance.OnLeverPulled();
            }
        }
    }

    private float EaseOutCubic(float t) {
        // Funcție easing pentru revenire smooth.
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    public void Reset() {
        // Resetează flag-uri și rotație/poziție.
        hasTriggered = false;
        isDragging = false;
        isReturning = false;
        transform.rotation = Quaternion.Euler(initialAngle, 0f, 0f);
        if (pivot != null) {
            transform.position = pivot.position;
        }
    }
}