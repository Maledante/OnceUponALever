// Modified DropzoneManager.cs
// Changes:
// - Modified GetNearestPosition and GetNearestFreePosition to return Vector3 with z=1f, but since dropPositions are Vector2, return new Vector3(pos.x, pos.y, 1f).
// - Updated AssignObjectToPosition, RemoveObjectFromPosition, etc., to handle Vector2 but assume z=1.

using System.Collections.Generic;
using UnityEngine;

public class DropzoneManager : MonoBehaviour {
    public Vector2[] dropPositions; // Include ALL possible positions, including initial ones

    private Dictionary<Vector2, GameObject> occupiedPositions = new Dictionary<Vector2, GameObject>();

    void Start() {
        // Now, initial positions should be registered in DragableObject Start()
    }

    public Vector3 GetNearestPosition(Vector2 currentPos, float threshold) {  // Changed return to Vector3
        Vector2 nearest = Vector2.zero;
        float minDist = float.MaxValue;

        foreach (Vector2 pos in dropPositions) {
            float dist = Vector2.Distance(currentPos, pos);
            if (dist < minDist && dist <= threshold) {
                minDist = dist;
                nearest = pos;
            }
        }

        return new Vector3(nearest.x, nearest.y, 1f);  // Force z=1
    }

    public Vector3 GetNearestFreePosition(Vector2 currentPos, float threshold) {  // Changed return to Vector3
        Vector2 nearest = Vector2.zero;
        float minDist = float.MaxValue;

        foreach (Vector2 pos in dropPositions) {
            if (!occupiedPositions.ContainsKey(pos)) {
                float dist = Vector2.Distance(currentPos, pos);
                if (dist < minDist && dist <= threshold) {
                    minDist = dist;
                    nearest = pos;
                }
            }
        }

        return new Vector3(nearest.x, nearest.y, 1f);  // Force z=1
    }

    public void AssignObjectToPosition(Vector2 pos, GameObject obj) {
        // Check if it's a valid drop position
        bool isValid = false;
        foreach (Vector2 dropPos in dropPositions) {
            if (Vector2.Distance(pos, dropPos) < 0.01f) // Approximate equality
            {
                isValid = true;
                pos = dropPos; // Snap to exact
                break;
            }
        }

        if (isValid) {
            occupiedPositions[pos] = obj;
        }
    }

    public void RemoveObjectFromPosition(Vector2 pos) {
        Vector2 keyToRemove = Vector2.zero;
        foreach (var key in occupiedPositions.Keys) {
            if (Vector2.Distance(key, pos) < 0.01f) {
                keyToRemove = key;
                break;
            }
        }

        if (keyToRemove != Vector2.zero) {
            occupiedPositions.Remove(keyToRemove);
        }
    }

    public GameObject GetObjectAtPosition(Vector2 pos) {
        foreach (var key in occupiedPositions.Keys) {
            if (Vector2.Distance(key, pos) < 0.01f) {
                if (occupiedPositions.TryGetValue(key, out GameObject obj)) {
                    return obj;
                }
            }
        }
        return null;
    }

    public Dictionary<Vector2, GameObject> GetAllOccupiedPositions() {
        return new Dictionary<Vector2, GameObject>(occupiedPositions);
    }

    public bool IsPositionOccupied(Vector2 pos) {
        foreach (var key in occupiedPositions.Keys) {
            if (Vector2.Distance(key, pos) < 0.01f) {
                return true;
            }
        }
        return false;
    }
}