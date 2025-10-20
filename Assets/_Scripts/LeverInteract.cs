using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

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
    public Vector2 associatedPosition; // Poziția fixă asociată sub această manetă.

    [Header("Setări Mutare Obiect")]
    public float moveDuration = 1f; // Durata mutării cu Lerp.
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Curba de easing.

    private float initialAngle = 0f;
    private bool isReturning = false;
    private float startTime;
    private float startAngle;
    private bool isDragging = false;
    private Camera cam;
    private float initialMouseY;
    private bool hasTriggered = false;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private DropzoneManager dropManager;
    private bool canInteract = true;
    private bool isActivated = false; // Track if the associated object is moved forward
    public bool IsActivated { get { return isActivated; } } // Public getter
    private Vector3 associatedObjectOriginalPosition; // Track original position of associated object

    private void Start() {
        cam = Camera.main;
        originalScale = transform.localScale; // Salvează scala inițială
        if (originalScale == Vector3.zero) {
            originalScale = Vector3.one; // Fallback pentru a preveni scala zero
            Debug.LogWarning($"Maneta {name} avea scala zero în Start. Setată la Vector3.one.");
        }
        originalPosition = transform.position; // Salvează poziția inițială
        if (pivot != null) {
            transform.position = pivot.position;
            originalPosition = pivot.position;
        }

        if (SceneManager.GetActiveScene().name != "MainMenu" && SceneManager.GetActiveScene().name != "Tutorials" && SceneManager.GetActiveScene().name != "Settings") {
            dropManager = Object.FindFirstObjectByType<DropzoneManager>();
            if (dropManager == null) {
                Debug.LogError($"DropZoneManager nu a fost găsit pentru maneta {name}!");
            }
        }

        if (pivot == null) {
            Debug.LogWarning($"Pivotul nu este setat pentru maneta {name}. Poziția poate fi incorectă.");
        }

        Debug.Log($"Maneta {name} inițializată. Scală: {originalScale}, Poziție: {originalPosition}");
    }

    private void OnMouseDown() {
        if (!enabled || cam == null || !canInteract) {
            Debug.Log($"Maneta {name} nu poate fi interacționată: enabled={enabled}, cam={cam != null}, canInteract={canInteract}");
            return;
        }
        if (GameManager.Instance != null && GameManager.Instance.writer.IsTyping) {
            Debug.Log($"Cannot interact with lever {name} while text is typing.");
            return;
        }
        isDragging = true;
        isReturning = false;
        canInteract = false;
        transform.localScale = originalScale * 1.1f;
        initialMouseY = cam.ScreenToWorldPoint(Input.mousePosition).y;

        // Inițializează startAngle cu unghiul curent la începutul drag-ului
        startAngle = transform.eulerAngles.x;
        if (startAngle > 180f) startAngle -= 360f;

        Debug.Log($"Maneta {name} apăsată. Scală curentă: {transform.localScale}, Poziție: {transform.position}, originalScale: {originalScale}, startAngle inițial: {startAngle}");
    }

    private void OnMouseUp() {
        if (isDragging) {
            isDragging = false;
            if (originalScale == Vector3.zero) {
                originalScale = Vector3.one; // Protecție împotriva scalei zero
                Debug.LogWarning($"Maneta {name} avea originalScale zero în OnMouseUp. Setată la Vector3.one.");
            }
            transform.localScale = originalScale;
            if (!isReturning) {
                startTime = Time.time;
                isReturning = true;
            }
            Debug.Log($"Maneta {name} eliberată. Scală curentă: {transform.localScale}, Poziție: {transform.position}, originalScale: {originalScale}");
            StartCoroutine(EnableInteractionAfterDelay(returnDuration));
        }
    }

    private IEnumerator EnableInteractionAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        canInteract = true;
        Debug.Log($"Maneta {name} este din nou interactivă.");
    }

    private void Update() {
        if (isDragging) {
            if (cam == null) {
                isDragging = false;
                canInteract = true;
                Debug.LogWarning($"Camera lipsește pentru maneta {name}. Oprire drag.");
                return;
            }
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            float deltaY = mouseWorldPos.y - initialMouseY;
            float angle = startAngle - deltaY * sensitivity;
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
            transform.rotation = Quaternion.Euler(angle, 0f, 0f);

            if (!hasTriggered && angle <= minAngle + 10f) {  // Trigger near minAngle (pulled down)
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
                hasTriggered = false;  // Reset hasTriggered to allow future pulls
            }
        }
    }

    private void LateUpdate() {
        // Force position to original every frame to prevent unwanted changes
        transform.position = originalPosition;
    }

    private void TriggerLeverAction() {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu" || sceneName == "Tutorials" || sceneName == "Settings") {
            MenuManager.Instance?.OnLeverPulled(leverType);
        }
        else {
            if (dropManager != null) {
                GameObject associatedSprite = dropManager.GetObjectAtPosition(associatedPosition);
                if (associatedSprite != null) {
                    DragableObject dragable = associatedSprite.GetComponent<DragableObject>();
                    if (dragable != null) {
                        // Process any sprite under the lever
                        if (!isActivated) {
                            dragable.Lock();
                            if (dragable.associatedObject != null) {
                                int sceneIndex = GameManager.Instance.currentScene - 1;
                                Vector3 moveOffset = GameManager.Instance.moveOffsetsPerScene[sceneIndex].ContainsKey(associatedSprite.name)
                                    ? GameManager.Instance.moveOffsetsPerScene[sceneIndex][associatedSprite.name]
                                    : Vector3.zero;
                                int sortingOrder = GameManager.Instance.characterSortingOrdersPerScene[sceneIndex].ContainsKey(associatedSprite.name)
                                    ? GameManager.Instance.characterSortingOrdersPerScene[sceneIndex][associatedSprite.name]
                                    : 5;
                                bool flipX = GameManager.Instance.flipSpritesPerScene[sceneIndex].ContainsKey(associatedSprite.name)
                                    ? GameManager.Instance.flipSpritesPerScene[sceneIndex][associatedSprite.name]
                                    : false;

                                associatedObjectOriginalPosition = dragable.associatedObject.transform.position; // Store original position
                                Debug.Log($"Maneta {name} a detectat sprite-ul: {associatedSprite.name} la poziția {associatedPosition}. Mută obiectul {dragable.associatedObject.name} cu offset {moveOffset}, sortingOrder {sortingOrder}, flipX {flipX}.");
                                dragable.SetMoveOffset(moveOffset); // Set the offset for reset purposes
                                dragable.MarkAsMoved(); // Mark as moved
                                StartCoroutine(MoveAssociatedObject(dragable.associatedObject, moveOffset));

                                SpriteRenderer sr = dragable.associatedObject.GetComponent<SpriteRenderer>();
                                if (sr != null) {
                                    sr.sortingOrder = sortingOrder;
                                    sr.flipX = flipX;
                                }
                                else {
                                    Debug.LogWarning($"Associated object {dragable.associatedObject.name} lacks SpriteRenderer for {associatedSprite.name}.");
                                }
                            }
                            isActivated = true;
                        }
                        else {
                            dragable.Unlock();
                            if (dragable.associatedObject != null) {
                                Debug.Log($"Maneta {name} reversează mutarea pentru {dragable.associatedObject.name} to original position {associatedObjectOriginalPosition}.");
                                StartCoroutine(MoveAssociatedObject(dragable.associatedObject, associatedObjectOriginalPosition - dragable.associatedObject.transform.position)); // Move back to original
                                SpriteRenderer sr = dragable.associatedObject.GetComponent<SpriteRenderer>();
                                if (sr != null) {
                                    sr.sortingOrder = 5; // Default sorting order
                                    sr.flipX = false; // Default flip state
                                }
                            }
                            isActivated = false;
                        }
                        GameManager.Instance.OnLeverPulled(this);
                    }
                    else {
                        Debug.LogWarning($"Sprite-ul {associatedSprite.name} nu are componenta DragableObject.");
                        GameManager.Instance.OnLeverPulled(this); // Still notify to increment count
                    }
                }
                else {
                    Debug.LogWarning($"Niciun sprite asociat la poziția {associatedPosition} pentru maneta {name}.");
                    GameManager.Instance.OnLeverPulled(this); // Still notify to increment count
                }
            }
        }
    }

    private IEnumerator MoveAssociatedObject(GameObject target, Vector3 offset) {
        if (target == null) {
            Debug.LogWarning($"Cannot move null associated object for lever {name}.");
            yield break;
        }
        Vector3 startPos = target.transform.position;
        Vector3 endPos = startPos + offset;
        float elapsed = 0f;

        while (elapsed < moveDuration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            t = moveCurve.Evaluate(t);
            target.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        target.transform.position = endPos;
        Debug.Log($"Moved associated object {target.name} to {endPos} for lever {name}.");
    }

    private float EaseOutCubic(float t) {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    public void Reset() {
        hasTriggered = false;
        isDragging = false;
        isReturning = false;
        canInteract = true;
        transform.rotation = Quaternion.Euler(initialAngle, 0f, 0f);
        if (originalScale == Vector3.zero) {
            originalScale = Vector3.one; // Protecție împotriva scalei zero
            Debug.LogWarning($"Maneta {name} avea originalScale zero în Reset. Setată la Vector3.one.");
        }
        transform.localScale = originalScale;
        transform.position = originalPosition;
        isActivated = false; // Reset activation state
        Debug.Log($"Maneta {name} resetată. Scală curentă: {transform.localScale}, Poziție: {transform.position}, originalScale: {originalScale}");
    }

    public void ResetSprite() {
        if (dropManager != null) {
            GameObject associatedSprite = dropManager.GetObjectAtPosition(associatedPosition);
            if (associatedSprite != null) {
                DragableObject dragable = associatedSprite.GetComponent<DragableObject>();
                if (dragable != null) {
                    dragable.Unlock();
                    dragable.ReturnToInitial();
                    dragable.ResetAssociatedObject();
                    dropManager.RemoveObjectFromPosition(associatedPosition);
                    isActivated = false; // Reset on scene reset
                    Debug.Log($"Reset sprite {associatedSprite.name} for lever {name}.");
                }
            }
        }
    }
}