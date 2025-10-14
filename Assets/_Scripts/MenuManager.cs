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
        }

        if (!string.IsNullOrEmpty(sceneToLoad)) {
            // Fade out înainte de load.
            if (FadeManager.Instance != null) {
                yield return StartCoroutine(FadeManager.Instance.FadeOut(() => SceneManager.LoadScene(sceneToLoad)));
            }
            else {
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}