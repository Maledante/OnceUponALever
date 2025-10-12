using UnityEngine;

public class maneta : MonoBehaviour
{
    [Header("Setări Pivot")]
    public Transform pivot;

    [Header("Setări Rotație")]
    public float minAngle = 0;
    public float maxAngle = 180f;
    public float rotationSpeed = 5f; // Păstrat pentru compatibilitate, dar nu mai folosit

    [Header("Setări Revenire")]
    public float returnDuration = 1f; // Durata totală de revenire (în secunde; scade pentru mai rapid)
    private float initialAngle = 0f; // Unghiul inițial (poți seta manual în Inspector dacă vrei)
    private bool isReturning = false;
    private float startTime;
    private float startAngle;

    private bool isDragging = false;
    private Camera cam;
    private Vector3 initialOffset; // Offset inițial de la pivot la manetă
    private float leverLength; // Lungimea manetei pentru normalizare

    void Start()
    {
        cam = Camera.main;
        if (pivot != null)
        {
            initialOffset = transform.position - pivot.position;
            leverLength = initialOffset.magnitude;
            initialAngle = minAngle; // Sau setează la o valoare custom dacă vrei
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
        isReturning = false; // Oprește revenirea dacă începi dragging
    }

    void OnMouseUp()
    {
        isDragging = false;
        if (!isReturning)
        {
            // Inițializează lerp-ul la eliberare
            startAngle = transform.eulerAngles.x;
            if (startAngle < 0f) startAngle += 360f;
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

            // Calculează doar diferența Y (sus/jos), ignoră X pentru a preveni mișcarea laterală
            float deltaY = mouseWorldPos.y - pivot.position.y;

            // Normalizează deltaY la lungimea manetei (pentru rotație naturală)
            float normalizedY = Mathf.Clamp(deltaY / leverLength, -1f, 1f);

            // Calculează unghiul bazat pe Acos pentru full range 0-180 (peste cap când jos)
            float angle = Mathf.Acos(normalizedY) * Mathf.Rad2Deg;

            // Limitează unghiul între min și max
            angle = Mathf.Clamp(angle, minAngle, maxAngle);

            // Aplică rotația pe axa X (out-of-plane) și poziția în jurul pivotului (doar pe verticală)
            Vector3 rotatedOffset = Quaternion.Euler(angle, 0f, 0f) * initialOffset;
            rotatedOffset.x = 0f; // Forțează X=0 pentru a bloca mișcarea laterală complet
            transform.position = pivot.position + rotatedOffset;
            transform.rotation = Quaternion.Euler(angle, 0f, 0f);
        }
        else if (!isDragging && isReturning)
        {
            // Lerp smooth pe unghi
            float targetAngle = initialAngle;
            if (targetAngle < 0f) targetAngle += 360f;

            float elapsed = Time.time - startTime;
            float t = Mathf.Clamp01(elapsed / returnDuration);

            // Aplică easing smooth (ease-out cubic)
            t = EaseOutCubic(t);

            float newAngle = Mathf.LerpAngle(startAngle, targetAngle, t);
            newAngle = (newAngle + 360f) % 360f; // Normalizează înapoi la [0, 360)

            // Aplică rotația și poziția consistent cu newAngle
            transform.rotation = Quaternion.Euler(newAngle, 0f, 0f);

            Vector3 rotatedOffset = Quaternion.Euler(newAngle, 0f, 0f) * initialOffset;
            rotatedOffset.x = 0f; // Blochează X pentru consistență
            transform.position = pivot.position + rotatedOffset;

            // Oprește revenirea când t=1
            if (t >= 1f)
            {
                isReturning = false;
            }
        }
    }

    // Funcție de easing smooth (ease-out cubic: lent la început, rapid spre sfârșit)
    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}