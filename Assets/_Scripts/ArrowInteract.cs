using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ArrowInteract : MonoBehaviour {
    [SerializeField] private bool isNextArrow; // True for next page arrow, false for previous page arrow
    private DropzoneManager dropzoneManager;

    void Start() {
        dropzoneManager = FindAnyObjectByType<DropzoneManager>();
        if (dropzoneManager == null) {
            Debug.LogError($"No DropzoneManager found in scene for {name}!");
        }

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null) {
            Debug.LogError($"No BoxCollider2D found on {name}!");
        }
    }

    void OnMouseDown() {
        if (dropzoneManager == null) {
            Debug.LogWarning($"Cannot switch page: DropzoneManager not found for {name}");
            return;
        }

        if (isNextArrow) {
            dropzoneManager.NextPage();
            Debug.Log($"Clicked {name} (Next Arrow). Switching to next page.");
        }
        else {
            dropzoneManager.PreviousPage();
            Debug.Log($"Clicked {name} (Previous Arrow). Switching to previous page.");
        }
    }
}