using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;


// Gestionază interacțiunea cu manetele, permițând rotația prin drag și revenirea.
// Detectează scena și declanșează acțiuni specifice (Menu sau Game).
// Adăugat: Integrare cu DropZoneManager pentru a verifica sprite-ul asociat poziției sub manetă.
// Când maneta este trasă, mută un GameObject asociat sprite-ului cu Lerp.
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

    [Header("Integrare Drag & Drop")]
    public Vector2 associatedPosition; // Poziția fixă asociată sub această manetă (din DropZoneManager.dropPositions).

    [Header("Setări Mutare Obiect")]
    public float moveDistance = 10f; // Distanța de mutare spre dreapta (pe axa X).
    public float moveDuration = 1f; // Durata mutării cu Lerp.
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Curba de easing.

    private float initialAngle = 0f; // Unghi inițial.
    private bool isReturning = false; // Flag revenire.
    private float startTime; // Timp start revenire.
    private float startAngle; // Unghi start revenire.
    private bool isDragging = false; // Flag drag.
    private Camera cam; // Camera principală.
    private float initialMouseY; // Y inițial mouse.
    private bool hasTriggered = false; // Flag declanșat.

    private DropzoneManager dropManager; // Referință la managerul de drop zones.

    private void Start() {
        // Inițializează camera și poziția la pivot.
        cam = Camera.main;
        if (pivot != null) {
            transform.position = pivot.position;
        }

        // Găsește DropZoneManager în scenă.
        dropManager = FindAnyObjectByType<DropzoneManager>();
        if (dropManager == null) {
            Debug.LogError("DropZoneManager nu a fost găsit în scenă! Asigură-te că există.");
        }
    }

    private void OnMouseDown() {
        // Verifică activat și cameră.
        if (!enabled || cam == null)
            return;

        // Începe drag.
        isDragging = true;
        isReturning = false;
        transform.localScale *= 1.1f;
        initialMouseY = cam.ScreenToWorldPoint(Input.mousePosition).y;
    }

    private void OnMouseUp() {
        // Oprește drag și începe revenire dacă nu deja.
        isDragging = false;
        transform.localScale /= 1.1f;
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
        // Detectează scena și declanșează acțiune corespunzătoare (păstrat original).
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu") {
            if (MenuManager.Instance != null) {
                MenuManager.Instance.OnLeverPulled(leverType);
            }
        }
        else {
            if (GameManager.Instance != null) {
                // Adăugat: Verifică sprite-ul asociat și mută obiectul corespunzător.
                if (dropManager != null) {
                    GameObject associatedSprite = dropManager.GetObjectAtPosition(associatedPosition);
                    if (associatedSprite != null) {
                        DragableObject dragable = associatedSprite.GetComponent<DragableObject>();
                        if (dragable != null) {
                            // Verifică dacă sprite-ul este unul required pentru scena curentă
                            List<string> required = GameManager.Instance.requiredSpritesPerScene[GameManager.Instance.currentScene - 1];
                            if (required.Contains(associatedSprite.name)) {
                                // Sprite corect: Blochează sprite-ul, mută obiectul și apelează OnLeverPulled
                                dragable.Lock();

                                if (dragable.associatedObject != null) {
                                    Debug.Log($"Maneta {name} a detectat sprite-ul corect: {associatedSprite.name} la poziția {associatedPosition}. Mută obiectul {dragable.associatedObject.name}.");
                                    StartCoroutine(MoveObjectToRight(dragable.associatedObject));
                                }
                                GameManager.Instance.OnLeverPulled(this); // Apelează OnLeverPulled doar dacă corect
                            }
                            else {
                                Debug.LogWarning($"Sprite-ul {associatedSprite.name} nu este corect pentru scena curentă. Resetează maneta.");
                                Reset(); // Resetează maneta dacă sprite-ul nu e corect
                            }
                        }
                        else {
                            Debug.LogWarning($"Sprite-ul {associatedSprite.name} nu are componenta DragableObject.");
                            Reset(); // Resetează dacă nu are componentă
                        }
                    }
                    else {
                        Debug.LogWarning($"Niciun sprite asociat la poziția {associatedPosition} pentru maneta {name}.");
                        Reset(); // Resetează dacă nu există sprite
                    }
                }
            }
        }
    }

    private IEnumerator MoveObjectToRight(GameObject target) {
        if (target == null) yield break;

        Vector3 startPos = target.transform.position;
        Vector3 endPos = startPos + new Vector3(moveDistance, 0f, 0f); // Mută spre dreapta pe X.
        float elapsed = 0f;

        while (elapsed < moveDuration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            t = moveCurve.Evaluate(t); // Aplică curba de easing.
            target.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        target.transform.position = endPos; // Asigură poziția finală exactă.
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

    public void ResetSprite() {
        if (dropManager != null) {
            GameObject associatedSprite = dropManager.GetObjectAtPosition(associatedPosition);
            if (associatedSprite != null) {
                DragableObject dragable = associatedSprite.GetComponent<DragableObject>();
                if (dragable != null) {
                    dragable.Unlock();
                    dragable.ReturnToOriginal();
                    dropManager.RemoveObjectFromPosition(associatedPosition);
                }
            }
        }
    }
}