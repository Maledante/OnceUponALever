using UnityEngine;

public class LeverInteract : MonoBehaviour
{
    [Header("Setări Pivot")]
    public Transform pivot; // Pivotul în jurul căruia se rotește maneta

    [Header("Setări Rotație")]
    public float minAngle = -180f; // Unghi minim (jos)
    public float maxAngle = 0f; // Unghi maxim (sus)
    public float sensitivity = 180f; // Sensibilitatea mișcării mouse-ului

    [Header("Setări Revenire")]
    public float returnDuration = 1f; // Durata totală de revenire (în secunde)
    private float initialAngle = 0f; // Unghiul inițial de revenire
    private bool isReturning = false;
    private float startTime;
    private float startAngle;

    private bool isDragging = false;
    private Camera cam;
    private float initialMouseY; // Poziția Y inițială a mouse-ului

    void Start()
    {
        cam = Camera.main;
        if (pivot != null)
        {
            // Setează poziția manetei la pivot (doar rotație, fără offset)
            transform.position = pivot.position;
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
        isReturning = false; // Oprește revenirea dacă începi dragging
        initialMouseY = cam.ScreenToWorldPoint(Input.mousePosition).y; // Salvează poziția Y inițială
    }

    void OnMouseUp()
    {
        isDragging = false;
        if (!isReturning)
        {
            // Inițializează lerp-ul pentru revenire
            startAngle = transform.eulerAngles.x;
            if (startAngle > 180f) startAngle -= 360f; // Normalizează la [-180, 0]
            startTime = Time.time;
            isReturning = true;
        }
    }

    void Update()
    {
        if (pivot == null) return;

        if (isDragging)
        {
            // Convertește poziția mouse-ului în coordonate world
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f; // Ignoră axa Z în 2D

            // Calculează diferența de mișcare pe axa Y (sus/jos)
            float deltaY = mouseWorldPos.y - initialMouseY;

            // Calculează unghiul bazat pe mișcarea mouse-ului
            float angle = startAngle - deltaY * sensitivity;

            // Limitează unghiul între minAngle (-180) și maxAngle (0)
            angle = Mathf.Clamp(angle, minAngle, maxAngle);

            // Aplică rotația doar pe axa X
            transform.rotation = Quaternion.Euler(angle, 0f, 0f);
        }
        else if (!isDragging && isReturning)
        {
            // Lerp lin pentru revenirea la unghiul inițial
            float targetAngle = initialAngle;
            float elapsed = Time.time - startTime;
            float t = Mathf.Clamp01(elapsed / returnDuration);

            // Aplică easing smooth (ease-out cubic)
            t = EaseOutCubic(t);

            float newAngle = Mathf.LerpAngle(startAngle, targetAngle, t);

            // Aplică rotația
            transform.rotation = Quaternion.Euler(newAngle, 0f, 0f);

            // Oprește revenirea când t=1
            if (t >= 1f)
            {
                isReturning = false;
            }
        }
    }

    // Funcție de easing smooth (ease-out cubic)
    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}