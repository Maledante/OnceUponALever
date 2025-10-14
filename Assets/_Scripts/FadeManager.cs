using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

// Acest script gestionează fade in/out pentru tranziții între scene.
public class FadeManager : MonoBehaviour {
    public static FadeManager Instance { get; private set; } // Singleton pentru acces global.

    public Image fadeImage; // Imaginea neagră pentru fade (asignată în Inspector).
    public float fadeDuration = 1f; // Durata fade-ului.

    void Awake() {
        // Inițializează singleton și DontDestroy pentru persistență între scene.
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }

        // Inițializează alpha la 1 pentru fade in inițial dacă necesar.
        if (fadeImage != null) {
            fadeImage.color = new Color(0, 0, 0, 0);
        }
    }

    public IEnumerator FadeOut(Action callback) {
        // Fade to black.
        if (fadeImage == null) yield break;

        float t = 0;
        while (t < 1) {
            t += Time.deltaTime / fadeDuration;
            fadeImage.color = new Color(0, 0, 0, t);
            yield return null;
        }

        // Execută callback (ex: load scena).
        callback?.Invoke();

        // Așteaptă un frame pentru load, apoi fade in.
        yield return null;
    }

    public IEnumerator FadeIn() {
        // Fade from black.
        if (fadeImage == null) yield break;

        float t = 1;
        while (t > 0) {
            t -= Time.deltaTime / fadeDuration;
            fadeImage.color = new Color(0, 0, 0, t);
            yield return null;
        }
    }
}