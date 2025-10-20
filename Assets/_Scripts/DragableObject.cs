using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class DragableObject : MonoBehaviour {
    private Vector3 initialPosition;
    private DropzoneManager dropManager;
    private bool isDragging = false;
    private Vector3 offset;
    public GameObject associatedObject; // Associated object to move when lever is pulled
    private Vector3 moveOffset;
    private bool wasMoved = false; // Tracks if associated object was moved
    private bool isSnapping = false; // Tracks if the object is currently lerping
    public bool IsSnapping { get { return isSnapping; } }
    public DropzoneManager DropzoneManager { get { return dropManager; } }

    void Start() {

        dropManager = FindAnyObjectByType<DropzoneManager>();
        if (dropManager == null) {
            Debug.LogError($"No DropzoneManager found for {name}!");
            return;
        }
        initialPosition = transform.position;
        // Delay position assignment until DropzoneManager is initialized
        StartCoroutine(InitializePosition());
    }

    private IEnumerator InitializePosition() {
        // Wait until DropzoneManager is initialized
        while (dropManager != null && !dropManager.IsInitialized) {
            Debug.Log($"Waiting for DropzoneManager to initialize for {name}");
            yield return null;
        }
        if (dropManager != null && dropManager.IsExactDropPosition(transform.position)) {
            dropManager.AssignObjectToPosition(transform.position, gameObject);
            Debug.Log($"Assigned {name} to initial position {transform.position} in Start");
        }
    }

    void OnMouseDown() {
        if (!enabled) {
            Debug.Log($"Cannot drag {name}: Component is disabled.");
            return;
        }
        if (GameManager.Instance != null && GameManager.Instance.writer.IsTyping) {
            Debug.Log($"Cannot drag {name}: Typewriter is typing.");
            return;
        }
        isDragging = true;
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset.z = 0;
        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
        sr.sortingOrder = 8; // Bring to front while dragging
        dropManager.RemoveObjectFromPosition(transform.position);
        Debug.Log($"Started dragging {name} from position {transform.position}");

    }

    void OnMouseDrag() {
        if (!isDragging) return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;
        mousePos.z = -1f;
        transform.position = mousePos;
    }

    void OnMouseUp() {
        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
        sr.sortingOrder = 5;
        if (!isDragging) return;
        isDragging = false;
        Vector3 nearestPos = dropManager.GetNearestFreePosition(transform.position, 0.5f);
        if (nearestPos != Vector3.zero) {
            isSnapping = true;
            StartCoroutine(LerpToPosition(nearestPos, 0.5f, () => {
                dropManager.AssignObjectToPosition(nearestPos, gameObject);
                Debug.Log($"Dropped {name} at position {nearestPos}");
            }));
        }
        else {
            isSnapping = true;
            StartCoroutine(LerpToPosition(initialPosition, 0.5f, () => {
                Debug.Log($"No valid drop position found for {name}. Returned to initial position {initialPosition}");
                // Only deactivate this inventory object if it's not present on the DropzoneManager's current page
                if (dropManager != null) {
                    int currentPage = dropManager.GetCurrentPage();
                    bool existsOnCurrentPage = false;
                    if (dropManager.pages != null && currentPage >= 0 && currentPage < dropManager.pages.Count) {
                        var page = dropManager.pages[currentPage];
                        if (page.draggableObjects != null) {
                            foreach (var obj in page.draggableObjects) {
                                if (obj == this.gameObject) {
                                    existsOnCurrentPage = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!existsOnCurrentPage) {
                        gameObject.SetActive(false);
                        Debug.Log($"Deactivated {name} because it's not on current page {currentPage}");
                    }
                    else {
                        Debug.Log($"Kept {name} active because it exists on current page {currentPage}");
                    }
                }
                else {
                    // Fallback: if there's no dropManager, keep previous behavior but log
                    Debug.LogWarning($"DropzoneManager missing when deciding active state for {name}; keeping it active.");
                }
            }));
        }
    }

    public void SetMoveOffset(Vector3 offset) {
        moveOffset = offset;
        Debug.Log($"Set moveOffset for {name} to {offset}");
    }

    public void Lock() {
        enabled = false;
        Debug.Log($"Locked {name}");
    }

    public void Unlock() {
        enabled = true;
        Debug.Log($"Unlocked {name}");
    }

    public void ReturnToInitial() {
        isSnapping = true;
        StartCoroutine(LerpToPosition(initialPosition, 0.5f, () => {
            Debug.Log($"Returned {name} to initial position {initialPosition}");
        }));
    }

    public void ResetAssociatedObject() {
        if (associatedObject != null && wasMoved) {
            associatedObject.transform.position -= moveOffset;
            wasMoved = false;
            Debug.Log($"Reset associated object {associatedObject.name} by reversing moveOffset {moveOffset} to position {associatedObject.transform.position}");
        }
        else if (associatedObject != null) {
            Debug.Log($"No reset needed for associated object {associatedObject.name}: wasMoved={wasMoved}");
        }
    }

    public void MarkAsMoved() {
        wasMoved = true;
        Debug.Log($"Marked {name}'s associated object as moved");
    }

    private IEnumerator LerpToPosition(Vector3 target, float duration, System.Action onComplete) {
        float time = 0;
        Vector3 startPosition = transform.position;

        while (time < duration) {
            transform.position = Vector3.Lerp(startPosition, target, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        isSnapping = false;
        onComplete?.Invoke();
    }
}