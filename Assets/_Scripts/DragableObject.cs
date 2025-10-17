// Modified DragableObject.cs
// Changes:
// - Reverted Z manipulation for associatedObject to preserve its original Z position.
// - Only the DragableObject itself (the sprite with this script) is forced to Z=-1.
// - Ensured originalAssociatedPos stores the associatedObject's original Z (not forced to -1).

using System.Collections;
using UnityEngine;

public class DragableObject : MonoBehaviour {
    private Vector3 offset;
    private Vector3 originalPosition;
    private bool isDragging = false;
    private bool isSnapping = false; // Flag pentru a preveni interferența cu snap
    public bool IsSnapping { get { return isSnapping; } } // Public getter for waiting in reset
    private float snapThreshold = 0.5f;
    private float snapSpeed = 10f;

    private DropzoneManager dropManager;

    [Header("Setări Obiect Asociat")]
    public GameObject associatedObject;
    [HideInInspector]
    public bool isLocked = false;

    [Header("Setări Mutare Personalizată")]
    public Vector3 moveOffset = new Vector3(5f, 0f, 0f); // Offset-ul de mutare relativ

    private Vector3 originalAssociatedPos; // Poziția inițială a obiectului asociat
    private SpriteRenderer sr;

    void Start() {
        dropManager = FindAnyObjectByType<DropzoneManager>();
        if (dropManager == null) {
            Debug.LogError("DropZoneManager not found! Please add it to the scene.");
            return;
        }
        // Force Z=-1 only for the DragableObject
        transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
        originalPosition = transform.position;
        Debug.Log($"Dragable {name} originalPosition set to {originalPosition}");

        if (associatedObject != null) {
            // Store associatedObject's original position without modifying its Z
            originalAssociatedPos = associatedObject.transform.position;
            Debug.Log($"Associated {associatedObject.name} originalAssociatedPos set to {originalAssociatedPos}");
        }

        dropManager.AssignObjectToPosition((Vector2)originalPosition, gameObject);
        sr = GetComponent<SpriteRenderer>();
    }

    void Update() {
        if (isDragging && !isSnapping && Input.GetMouseButton(0)) {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = -1f; // Force Z=-1 for DragableObject
            transform.position = mouseWorldPos + offset;
        }
    }

    void OnMouseDown() {
        if (GameManager.Instance != null && GameManager.Instance.writer.IsTyping) {
            Debug.Log($"Cannot drag {name} while text is typing.");
            return;  // Prevent dragging while typing
        }

        if (!isDragging && !isLocked && dropManager != null && !isSnapping) {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = -1f; // Force Z=-1 for DragableObject
            offset = transform.position - mouseWorldPos;

            transform.localScale *= 1.1f;
            dropManager.RemoveObjectFromPosition((Vector2)transform.position);

            isDragging = true;
            sr.sortingOrder = 10;
        }
    }

    void OnMouseUp() {
        if (isDragging && dropManager != null) {
            isDragging = false;
            transform.localScale /= 1.1f;
            sr.sortingOrder = 5;

            Vector3 nearestPosition = dropManager.GetNearestPosition((Vector2)transform.position, snapThreshold);

            if (nearestPosition != Vector3.zero) {
                GameObject existingObj = dropManager.GetObjectAtPosition((Vector2)nearestPosition);
                if (existingObj != null && existingObj != gameObject) {
                    DragableObject existingDragable = existingObj.GetComponent<DragableObject>();
                    if (existingDragable != null) {
                        if (existingDragable.isLocked) {
                            StartCoroutine(SmoothSnap(originalPosition));
                            return;
                        }
                        else {
                            StartCoroutine(existingDragable.SmoothSnap(existingDragable.originalPosition));
                            dropManager.RemoveObjectFromPosition((Vector2)nearestPosition);
                        }
                    }
                }
                StartCoroutine(SmoothSnap(new Vector3(nearestPosition.x, nearestPosition.y, -1f)));
            }
            else {
                StartCoroutine(SmoothSnap(originalPosition));
            }
        }
    }

    private IEnumerator SmoothSnap(Vector3 targetPosition) {
        isSnapping = true;
        targetPosition.z = -1f; // Force Z=-1 for DragableObject
        Debug.Log($"Snapping {name} from {transform.position} to {targetPosition}");
        while (Vector3.Distance(transform.position, targetPosition) > 0.001f) { // Reduced threshold for precision
            transform.position = Vector3.Lerp(transform.position, targetPosition, snapSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
        dropManager.AssignObjectToPosition((Vector2)targetPosition, gameObject);
        isSnapping = false;
        Debug.Log($"Snapped {name} to {targetPosition}");
    }

    public void ReturnToOriginal() {
        Debug.Log($"Returning {name} to original {originalPosition} from current {transform.position}");
        StartCoroutine(SmoothSnap(originalPosition));
    }

    public void Lock() {
        isLocked = true;
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) {
            collider.enabled = false;
        }
    }

    public void Unlock() {
        isLocked = false;
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) {
            collider.enabled = true;
        }
    }

    public void ResetAssociatedObject() {
        if (associatedObject != null) {
            // Reset to originalAssociatedPos without modifying Z
            Vector3 resetPos = originalAssociatedPos;
            Debug.Log($"Resetting associated {associatedObject.name} to {resetPos} from {associatedObject.transform.position}");
            associatedObject.transform.position = resetPos;
        }
    }

    public void SetMoveOffset(Vector3 newOffset) {
        moveOffset = newOffset;
    }
}