using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// Acest script gestionează meniul principal, inclusiv încărcarea scenelor prin manete.
public class MenuManager : MonoBehaviour {
    public static MenuManager Instance { get; private set; } // Singleton pentru acces global.

    private void Awake() {
        // Inițializează singleton și DontDestroy pentru persistență.
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    private void OnEnable() {
        // Subscrie la evenimentul sceneLoaded pentru a gestiona fade in-ul
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        // Dezsubscrie de la eveniment pentru a evita memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnLeverPulled(string leverType) {
        // Încarcă scena corespunzătoare cu fade.
        StartCoroutine(LoadSceneWithFade(leverType));
    }

    private IEnumerator LoadSceneWithFade(string leverType) {
        string sceneToLoad = "";
        switch (leverType) {
            case "Play":
                sceneToLoad = "MainGame";
                break;
            case "Options":
                sceneToLoad = "Settings";
                break;
            case "Tutorial":
                sceneToLoad = "Tutorials";
                break;
            case "Back":
                sceneToLoad = "MainMenu";
                break;
        }

        if (!string.IsNullOrEmpty(sceneToLoad)) {
            // Fade out înainte de load.
            if (FadeManager.Instance != null) {
                yield return StartCoroutine(FadeManager.Instance.FadeOut(() => SceneManager.LoadScene(sceneToLoad)));
            }
            else {
                Debug.LogWarning("FadeManager.Instance is null, loading scene without fade.");
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // Fade in după ce scena s-a încărcat
        if (FadeManager.Instance != null) {
            StartCoroutine(FadeManager.Instance.FadeIn());
        }
        else {
            Debug.LogWarning("FadeManager.Instance is null, cannot perform fade in.");
        }
    }
}