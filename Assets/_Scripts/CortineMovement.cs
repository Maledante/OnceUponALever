using UnityEngine;

public class CortineMovement : MonoBehaviour
{
    public float durataInchidere = 2f; // Duration of the animation in seconds
    private Vector3 pozitieInitiala;
    private Vector3 pozitieFinala;
    private bool inMiscarare = false;

    private void Start()
    {
        // Store the initial local position
        pozitieInitiala = transform.localPosition;

        // Set final local positions based on curtain name
        if (gameObject.name == "CortinaStanga")
        {
            // Left curtain: move from local x=-11.5 to x=-7.5 (4 units right)
            pozitieFinala = new Vector3(-5f, transform.localPosition.y, transform.localPosition.z);
        }

        if (gameObject.name == "CortinaDreapta")
        {
            // Right curtain: move 4 units left (assuming initial local x=11.5, move to x=7.5)
            pozitieFinala = new Vector3(7.5f, transform.localPosition.y, transform.localPosition.z);
        }
    }

    public void InchideCopertine()
    {
        if (!inMiscarare)
        {
            Debug.Log($"{gameObject.name} se mișcă!");
            StartCoroutine(MiscaCortina());
        }
    }

    private System.Collections.IEnumerator MiscaCortina()
    {
        inMiscarare = true;
        float elapsed = 0f;

        Vector3 startPos = transform.localPosition;

        while (elapsed < durataInchidere)
        {
            float t = elapsed / durataInchidere;
            // Smoothly interpolate between start and final local position
            transform.localPosition = Vector3.Lerp(startPos, pozitieFinala, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the curtain reaches the exact final local position
        transform.localPosition = pozitieFinala;
        inMiscarare = false;
    }

    // Optional: Reset curtains to initial local positions
    public void ReseteazaCortine()
    {
        if (!inMiscarare)
        {
            Debug.Log($"{gameObject.name} se resetează!");
            StartCoroutine(MiscaSpreInitial());
        }
    }

    private System.Collections.IEnumerator MiscaSpreInitial()
    {
        inMiscarare = true;
        float elapsed = 0f;

        Vector3 startPos = transform.localPosition;

        while (elapsed < durataInchidere)
        {
            float t = elapsed / durataInchidere;
            transform.localPosition = Vector3.Lerp(startPos, pozitieInitiala, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = pozitieInitiala;
        inMiscarare = false;
    }
}