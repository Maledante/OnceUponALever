using System.Collections;
using UnityEngine;

public class DragableObject : MonoBehaviour {
    private Vector3 offset;
    private Vector3 originalPosition;
    private bool isDragging = false;
    private bool isSnapping = false; // Flag pentru a preveni interferența cu snap
    private float snapThreshold = 0.5f;
    private float snapSpeed = 10f;

    private DropzoneManager dropManager;

    [Header("Setări Obiect Asociat")]
    public GameObject associatedObject;
    [HideInInspector]
    public bool isLocked = false;

    SpriteRenderer sr;

    void Start() {
        dropManager = FindAnyObjectByType<DropzoneManager>();
        if (dropManager == null) {
            Debug.LogError("DropZoneManager not found! Please add it to the scene.");
            return;
        }
        originalPosition = transform.position;

        dropManager.AssignObjectToPosition((Vector2)originalPosition, gameObject);
        sr = GetComponent<SpriteRenderer>();
    }

    void Update() {
        // Mută sprite-ul doar dacă e în drag și nu în snap
        if (isDragging && !isSnapping && Input.GetMouseButton(0)) {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = transform.position.z;
            transform.position = mouseWorldPos + offset;
        }
    }

    void OnMouseDown() {
        if (!isDragging && !isLocked && dropManager != null && !isSnapping) {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = transform.position.z;
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

            Vector2 nearestPosition = dropManager.GetNearestPosition((Vector2)transform.position, snapThreshold);

            if (nearestPosition != Vector2.zero) {
                GameObject existingObj = dropManager.GetObjectAtPosition(nearestPosition);
                if (existingObj != null && existingObj != gameObject) {
                    DragableObject existingDragable = existingObj.GetComponent<DragableObject>();
                    if (existingDragable != null) {
                        if (existingDragable.isLocked) {
                            // Dacă sprite-ul existent este blocat, trimite sprite-ul curent (nou) înapoi la original
                            StartCoroutine(SmoothSnap(originalPosition));
                            return; // Oprește plasarea sprite-ului nou
                        }
                        else {
                            // Dacă nu este blocat, trimite-l pe cel existent înapoi
                            StartCoroutine(existingDragable.SmoothSnap(existingDragable.originalPosition));
                            dropManager.RemoveObjectFromPosition(nearestPosition);
                        }
                    }
                }
                StartCoroutine(SmoothSnap(nearestPosition));
            }
            else {
                StartCoroutine(SmoothSnap(originalPosition));
            }
        }
    }

    private IEnumerator SmoothSnap(Vector3 targetPosition) {
        isSnapping = true; // Setează flag-ul pentru a preveni drag în timpul snap

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f) {
            transform.position = Vector3.Lerp(transform.position, targetPosition, snapSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        dropManager.AssignObjectToPosition((Vector2)targetPosition, gameObject);
        isSnapping = false; // Resetează flag-ul după snap
    }

    public void ReturnToOriginal() {
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
}