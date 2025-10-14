using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LeverInteract : MonoBehaviour {
    [Header("Setări Pivot")]
    public Transform pivot;

    [Header("Setări Rotație")]
    public float minAngle = -180f;
    public float maxAngle = 0f;
    public float sensitivity = 180f;

    [Header("Setări Revenire")]
    public float returnDuration = 1f;

    [Header("Main Menu Settings")]
    public string leverType = "Play";

    [Header("Integrare Drag & Drop")]
    public Vector2 associatedPosition;

    [Header("Setări Mutare Obiect")]
    public float moveDistance = 5f;
    public float moveDuration = 1f;
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private float initialAngle = 0f;
    private bool isReturning = false;
    private float startTime;
    private float startAngle;
    private bool isDragging = false;
    private Camera cam;
    private float initialMouseY;
    private bool hasTriggered = false;

    private DropzoneManager dropManager;

    private void Start() {
        cam = Camera.main;
        if (pivot != null) {
            transform.position = pivot.position;
        }

        dropManager = FindAnyObjectByType<DropzoneManager>();
        if (dropManager == null) {
            Debug.LogError("DropZoneManager nu a fost găsit în scenă!");
        }
    }

    private void OnMouseDown() {
        if (!enabled || cam == null)
            return;

        isDragging = true;
        isReturning = false;
        transform.localScale *= 1.1f;
        initialMouseY = cam.ScreenToWorldPoint(Input.mousePosition).y;
    }

    private void OnMouseUp() {
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
            if (cam == null) {
                isDragging = false;
                return;
            }

            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            float deltaY = mouseWorldPos.y - initialMouseY;
            float angle = startAngle - deltaY * sensitivity;
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
            transform.rotation = Quaternion.Euler(angle, 0f, 0f);

            if (!hasTriggered && Mathf.Abs(angle - minAngle) < 1f) {
                hasTriggered = true;
                TriggerLeverAction();
            }
        }
        else if (isReturning) {
            float elapsed = Time.time - startTime;
            float t = Mathf.Clamp01(elapsed / returnDuration);
            t = EaseOutCubic(t);
            float newAngle = Mathf.LerpAngle(startAngle, initialAngle, t);
            transform.rotation = Quaternion.Euler(newAngle, 0f, 0f);

            if (t >= 1f) {
                isReturning = false;
            }
        }
    }

    private void TriggerLeverAction() {
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

        if (dropManager != null) {
            GameObject associatedSprite = dropManager.GetObjectAtPosition(associatedPosition);
            if (associatedSprite != null) {
                DragableObject dragable = associatedSprite.GetComponent<DragableObject>();
                if (dragable != null) {
                    dragable.Lock();
                    if (dragable.associatedObject != null) {
                        Debug.Log($"Maneta {name} a detectat sprite-ul: {associatedSprite.name} la poziția {associatedPosition}. Mută obiectul {dragable.associatedObject.name}.");
                        StartCoroutine(MoveObjectToRight(dragable.associatedObject));
                    }
                    else {
                        Debug.LogWarning($"Sprite-ul {associatedSprite.name} nu are un obiect asociat.");
                    }
                }
                else {
                    Debug.LogWarning($"Sprite-ul {associatedSprite.name} nu are componenta DragableObject.");
                }
            }
            else {
                Debug.LogWarning($"Niciun sprite la poziția {associatedPosition} pentru maneta {name}.");
            }
        }
    }

    private IEnumerator MoveObjectToRight(GameObject target) {
        if (target == null) yield break;

        Vector3 startPos = target.transform.position;
        Vector3 endPos = startPos + new Vector3(moveDistance, 0f, 0f);
        float elapsed = 0f;

        while (elapsed < moveDuration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            t = moveCurve.Evaluate(t);
            target.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        target.transform.position = endPos;
    }

    private float EaseOutCubic(float t) {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    public void Reset() {
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