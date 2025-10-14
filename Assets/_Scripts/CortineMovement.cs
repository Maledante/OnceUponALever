using UnityEngine;
using System.Collections;

// Acest script controlează mișcarea cortinelor (copertinelor) pentru tranziții.
public class CortineMovement : MonoBehaviour {
    public float durataInchidere = 2f; // Durata animației de închidere/deschidere.
    private Vector3 pozitieInitiala; // Poziția inițială deschisă.
    private Vector3 pozitieFinala; // Poziția finală închisă.
    private bool inMiscarare = false; // Flag dacă cortina se mișcă.

    private void Start() {
        // Salvează poziția inițială și calculează poziția finală bazat pe nume.
        pozitieInitiala = transform.localPosition;

        if (gameObject.name.Contains("Stanga")) {
            pozitieFinala = new Vector3(pozitieInitiala.x - 5f, pozitieInitiala.y, pozitieInitiala.z);
        }
        else if (gameObject.name.Contains("Dreapta")) {
            pozitieFinala = new Vector3(pozitieInitiala.x + 5f, pozitieInitiala.y, pozitieInitiala.z);
        }
    }

    public IEnumerator InchideCopertine() {
        // Dacă deja în mișcare, oprește.
        if (inMiscarare)
            yield break;

        // Animație de închidere folosind Lerp.
        inMiscarare = true;
        float elapsed = 0f;
        Vector3 startPos = transform.localPosition;

        while (elapsed < durataInchidere) {
            float t = elapsed / durataInchidere;
            transform.localPosition = Vector3.Lerp(startPos, pozitieFinala, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = pozitieFinala;
        inMiscarare = false;
    }

    public IEnumerator DeschideCopertine() {
        // Dacă deja în mișcare, oprește.
        if (inMiscarare)
            yield break;

        // Animație de deschidere folosind Lerp.
        inMiscarare = true;
        float elapsed = 0f;
        Vector3 startPos = transform.localPosition;

        while (elapsed < durataInchidere) {
            float t = elapsed / durataInchidere;
            transform.localPosition = Vector3.Lerp(startPos, pozitieInitiala, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = pozitieInitiala;
        inMiscarare = false;
    }
}