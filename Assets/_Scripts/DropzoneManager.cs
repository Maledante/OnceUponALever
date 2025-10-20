using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Page {
    public GameObject[] draggableObjects; // Draggable objects for this page
}

public class DropzoneManager : MonoBehaviour {
    [Header("Drop Positions")]
    [SerializeField] private Vector2[] dropPositions; // Shared drop positions for all pages

    [Header("Page Settings")]
    [SerializeField] public List<Page> pages; // List of pages, each with its draggable objects

    private int currentPage = 0; // Current active page index
    private Dictionary<Vector2, GameObject> occupiedPositions = new Dictionary<Vector2, GameObject>(); // Shared occupied positions across pages
    private List<Vector2> leverPositions = new List<Vector2>(); // Positions under levers, fetched from GameManager
    private bool isInitialized = false; // Tracks if initialization is complete

    public bool IsInitialized { get { return isInitialized; } } // Public getter for initialization status

    void Awake() {
        if (pages == null) {
            pages = new List<Page>();
            Debug.LogWarning($"pages was null in {name}. Initialized as empty list.");
        }
    }

    void Start() {
        occupiedPositions = new Dictionary<Vector2, GameObject>();
        if (pages.Count == 0) {
            Debug.LogWarning($"No pages defined in DropzoneManager! Initializing with one empty page.");
            pages.Add(new Page { draggableObjects = new GameObject[0] });
        }

        for (int i = 0; i < pages.Count; i++) {
            Page currentPageStruct = pages[i];
            if (currentPageStruct.draggableObjects == null) {
                currentPageStruct.draggableObjects = new GameObject[0];
                pages[i] = currentPageStruct;
                Debug.LogWarning($"Page {i} had null draggable objects array in DropzoneManager. Initialized as empty array.");
            }
            else {
                for (int j = 0; j < currentPageStruct.draggableObjects.Length; j++) {
                    if (currentPageStruct.draggableObjects[j] == null) {
                        Debug.LogWarning($"Null object at index {j} in page {i} of pages.");
                    }
                }
                Debug.Log($"Page {i} has {currentPageStruct.draggableObjects.Length} draggable objects assigned.");
            }
        }

        GameManager gm = FindAnyObjectByType<GameManager>();
        if (gm != null) {
            foreach (LeverInteract lever in gm.levers) {
                if (lever != null) {
                    leverPositions.Add(lever.associatedPosition);
                }
            }
            Debug.Log($"Found {leverPositions.Count} lever positions from GameManager.");
        }
        else {
            Debug.LogWarning($"No GameManager found in scene. Lever positions not collected.");
        }

        if (dropPositions == null || dropPositions.Length == 0) {
            Debug.LogWarning($"No drop positions defined in DropzoneManager!");
            dropPositions = new Vector2[0];
        }
        else {
            Debug.Log($"DropzoneManager initialized with {dropPositions.Length} drop positions: {string.Join(", ", dropPositions)} and {pages.Count} pages.");
        }

        isInitialized = true;
        // Do not call SwitchToPage here to avoid activating sprites
    }

    [ContextMenu("Reset Component")]
    private void ResetComponent() {
        pages = new List<Page>();
        dropPositions = new Vector2[0];
        Debug.Log($"Reset DropzoneManager component on {name}. Reassign drop positions and pages in Inspector.");
    }

    public void SwitchToPage(int pageIndex, GameManager gameManager) {
        if (!isInitialized) {
            Debug.LogWarning($"Cannot switch to page {pageIndex}. DropzoneManager is not initialized.");
            return;
        }
        if (pageIndex < 0 || pageIndex >= pages.Count) {
            Debug.LogWarning($"Cannot switch to page {pageIndex}. Valid range: 0 to {pages.Count - 1}");
            return;
        }

        // Deactivate old page's inventory objects (not at lever positions)
        foreach (GameObject obj in pages[currentPage].draggableObjects) {
            if (obj != null && !leverPositions.Contains((Vector2)obj.transform.position)) {
                obj.SetActive(false);
                Debug.Log($"Deactivated inventory object {obj.name} on page {currentPage}");
            }
        }

        currentPage = pageIndex;

        // Activate new page's inventory objects, respecting GameManager's active state
        foreach (GameObject obj in pages[currentPage].draggableObjects) {
            if (obj != null) {
                int sceneIndex = gameManager.currentScene - 1;
                if (gameManager.GetAvailableSpritesForScene(sceneIndex).Contains(obj.name)) {
                    obj.SetActive(true);
                    Debug.Log($"Activated object {obj.name} on page {currentPage}");
                }
            }
        }

        Debug.Log($"Switched to page {currentPage}");

        gameManager.EnableArrows();
    }

    public void NextPage() {
        if (!isInitialized) {
            Debug.LogWarning($"Cannot switch to next page. DropzoneManager is not initialized.");
            return;
        }
        int nextPage = currentPage + 1;
        if (nextPage < pages.Count) {
            GameManager gm = FindAnyObjectByType<GameManager>();
            if (gm != null) {
                SwitchToPage(nextPage, gm);
            }
            else {
                Debug.LogWarning($"No GameManager found. Cannot switch to page {nextPage}.");
            }
        }
        else {
            Debug.Log($"Already on last page ({currentPage}). Cannot go to next page.");
        }
    }

    public void PreviousPage() {
        if (!isInitialized) {
            Debug.LogWarning($"Cannot switch to previous page. DropzoneManager is not initialized.");
            return;
        }
        int prevPage = currentPage - 1;
        if (prevPage >= 0) {
            GameManager gm = FindAnyObjectByType<GameManager>();
            if (gm != null) {
                SwitchToPage(prevPage, gm);
            }
            else {
                Debug.LogWarning($"No GameManager found. Cannot switch to page {prevPage}.");
            }
        }
        else {
            Debug.Log($"Already on first page ({currentPage}). Cannot go to previous page.");
        }
    }

    public Vector3 GetNearestPosition(Vector2 currentPos, float threshold) {
        if (dropPositions.Length == 0) {
            Debug.LogWarning($"No drop positions defined in DropzoneManager! Returning Vector3.zero");
            return Vector3.zero;
        }

        Vector2 nearest = Vector2.zero;
        float minDist = float.MaxValue;
        bool foundValid = false;

        foreach (Vector2 pos in dropPositions) {
            float dist = Vector2.Distance(currentPos, pos);
            if (dist < 0.05f) {
                Debug.Log($"Found near-exact match for {currentPos} at {pos}");
                return new Vector3(pos.x, pos.y, -1f);
            }
            if (dist < minDist && dist <= threshold) {
                minDist = dist;
                nearest = pos;
                foundValid = true;
            }
        }

        if (foundValid) {
            Debug.Log($"Nearest valid drop position for {currentPos} is {nearest}");
            return new Vector3(nearest.x, nearest.y, -1f);
        }

        Debug.Log($"No drop position within threshold {threshold} for {currentPos}. Returning Vector3.zero");
        return Vector3.zero;
    }

    public bool IsExactDropPosition(Vector2 pos) {
        foreach (Vector2 dropPos in dropPositions) {
            if (Vector2.Distance(pos, dropPos) < 0.05f) {
                Debug.Log($"Position {pos} is an exact drop position (matches {dropPos})");
                return true;
            }
        }
        Debug.Log($"Position {pos} is not an exact drop position");
        return false;
    }

    public Vector3 GetNearestFreePosition(Vector2 currentPos, float threshold) {
        if (!isInitialized) {
            Debug.LogWarning($"Cannot get nearest free position. DropzoneManager is not initialized.");
            return Vector3.zero;
        }
        if (dropPositions.Length == 0) {
            Debug.LogWarning($"No drop positions defined in DropzoneManager! Returning Vector3.zero");
            return Vector3.zero;
        }

        Vector2 nearest = Vector2.zero;
        float minDist = float.MaxValue;
        bool foundValid = false;

        foreach (Vector2 pos in dropPositions) {
            if (!occupiedPositions.ContainsKey(pos)) {
                float dist = Vector2.Distance(currentPos, pos);
                if (dist < 0.05f) {
                    Debug.Log($"Found near-exact free match for {currentPos} at {pos}");
                    return new Vector3(pos.x, pos.y, -1f);
                }
                if (dist < minDist && dist <= threshold) {
                    minDist = dist;
                    nearest = pos;
                    foundValid = true;
                }
            }
        }

        if (foundValid) {
            Debug.Log($"Nearest free drop position for {currentPos} is {nearest}");
            return new Vector3(nearest.x, nearest.y, -1f);
        }

        Debug.Log($"No free drop position within threshold {threshold} for {currentPos}. Returning Vector3.zero");
        return Vector3.zero;
    }

    public void AssignObjectToPosition(Vector2 pos, GameObject obj) {
        if (!isInitialized) {
            Debug.LogWarning($"Cannot assign object to position {pos}. DropzoneManager is not initialized.");
            return;
        }
        bool isValid = false;
        Vector2 exactPos = pos;
        foreach (Vector2 dropPos in dropPositions) {
            if (Vector2.Distance(pos, dropPos) < 0.05f) {
                isValid = true;
                exactPos = dropPos;
                break;
            }
        }

        if (isValid) {
            if (occupiedPositions.ContainsKey(exactPos) && occupiedPositions[exactPos] != obj) {
                Debug.LogWarning($"Position {exactPos} already occupied by {occupiedPositions[exactPos].name}. Overwriting with {obj.name}");
            }
            occupiedPositions[exactPos] = obj;
            Debug.Log($"Assigned {obj.name} to position {exactPos}");
        }
        else {
            Debug.LogWarning($"Attempted to assign {obj.name} to invalid position {pos}");
        }
    }

    public void RemoveObjectFromPosition(Vector2 pos) {
        if (!isInitialized) {
            Debug.LogWarning($"Cannot remove object from position {pos}. DropzoneManager is not initialized.");
            return;
        }
        Vector2 keyToRemove = Vector2.zero;
        foreach (var key in occupiedPositions.Keys) {
            if (Vector2.Distance(key, pos) < 0.05f) {
                keyToRemove = key;
                break;
            }
        }

        if (keyToRemove != Vector2.zero) {
            Debug.Log($"Removed object {occupiedPositions[keyToRemove].name} from position {keyToRemove}");
            occupiedPositions.Remove(keyToRemove);
        }
    }

    public GameObject GetObjectAtPosition(Vector2 pos) {
        if (!isInitialized) {
            Debug.LogWarning($"Cannot get object at position {pos}. DropzoneManager is not initialized.");
            return null;
        }
        foreach (var key in occupiedPositions.Keys) {
            if (Vector2.Distance(key, pos) < 0.05f) {
                if (occupiedPositions.TryGetValue(key, out GameObject obj)) {
                    Debug.Log($"Found object {obj.name} at position {key}");
                    return obj;
                }
            }
        }
        Debug.Log($"No object found at position {pos}");
        return null;
    }

    public Dictionary<Vector2, GameObject> GetAllOccupiedPositions() {
        if (!isInitialized) {
            Debug.LogWarning($"Cannot get occupied positions. DropzoneManager is not initialized.");
            return new Dictionary<Vector2, GameObject>();
        }
        return new Dictionary<Vector2, GameObject>(occupiedPositions);
    }

    public bool IsPositionOccupied(Vector2 pos) {
        if (!isInitialized) {
            Debug.LogWarning($"Cannot check if position {pos} is occupied. DropzoneManager is not initialized.");
            return false;
        }
        foreach (var key in occupiedPositions.Keys) {
            if (Vector2.Distance(key, pos) < 0.05f) {
                Debug.Log($"Position {pos} is occupied by {occupiedPositions[key].name}");
                return true;
            }
        }
        Debug.Log($"Position {pos} is not occupied");
        return false;
    }

    public int GetCurrentPage() {
        return currentPage;
    }
}