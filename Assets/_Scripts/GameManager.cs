using UnityEngine;
using System.Collections;

// Acest script gestionează starea generală a jocului, inclusiv tranzițiile între scenele poveștii,
// interacțiunile cu manetele și sfoara, și controlul cortinelor și textului.
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; } // Singleton pentru acces global.

    // Referințe asignate în Inspector.
    public LeverInteract[] levers; // Array cu manetele de interacțiune.
    public RopeInteract rope; // Referință la scriptul sfoarei.
    public CortineMovement[] curtains; // Array cu cortinele (stânga și dreapta).
    public TypewriterEffect writer; // Referință la scriptul typewriter pentru text.

    // Enum pentru stările jocului.
    public enum GameState {
        Intro, // Afișare poveste inițială.
        Interaction, // Interacțiune cu manete.
        ReadyToPullRope, // Gata pentru tragerea sfoarei.
        Transition, // Tranziție la scena următoare.
        End // Sfârșit joc.
    }

    public GameState currentState = GameState.Intro; // Starea inițială.
    private int leversPulledCount = 0; // Contor manete trase.
    private int currentScene = 1; // Scena curentă (începe de la 1).

    // Texte pentru fiecare scenă a poveștii.
    public string[] storyTexts = new string[]
    {
        "There was once a King who had three sons. He ruled a fair land in a big castle",
        "Povestea scenei 2: Noi provocări aici."
    };

    // Numărul necesar de manete trase per scenă (asignat în Inspector).
    public int[] requiredLeversPerScene; // Ex: {5, 3} pentru scena 1 și 2.

    void Awake() {
        // Inițializează singleton-ul.
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    void Start() {
        // Pornește corutina de start joc și fade in.
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame() {
        // Fade in la începutul jocului.
        if (FadeManager.Instance != null)
            yield return StartCoroutine(FadeManager.Instance.FadeIn());

        // Setează starea inițială și afișează textul primei scene.
        SetState(GameState.Intro);
        yield return StartCoroutine(writer.StartTyping(storyTexts[currentScene - 1]));
        SetState(GameState.Interaction);
    }

    private void SetState(GameState newState) {
        // Schimbă starea și execută acțiuni specifice.
        currentState = newState;
        switch (newState) {
            case GameState.Intro:
                DisableInteractions();
                break;
            case GameState.Interaction:
                EnableLevers();
                DisableRope();
                break;
            case GameState.ReadyToPullRope:
                EnableRope();
                break;
            case GameState.Transition:
                StartCoroutine(TransitionToNextScene());
                break;
            case GameState.End:
                break;
        }
    }

    public void OnLeverPulled() {
        // Increment contor și verifică dacă sunt toate necesare trase.
        leversPulledCount++;
        if (leversPulledCount >= requiredLeversPerScene[currentScene - 1]) {
            SetState(GameState.ReadyToPullRope);
        }
    }

    public void OnRopePulled() {
        // Verifică starea și trece la tranziție.
        if (currentState == GameState.ReadyToPullRope) {
            SetState(GameState.Transition);
        }
    }

    private IEnumerator TransitionToNextScene() {
        // Închide cortinele.
        foreach (var curtain in curtains) {
            yield return StartCoroutine(curtain.InchideCopertine());
        }

        // Resetează manete și contor.
        ResetLevers();
        leversPulledCount = 0;

        // Avansează scena și verifică sfârșit.
        currentScene++;
        if (currentScene > storyTexts.Length) {
            SetState(GameState.End);
            yield break;
        }

        // Skip text curent, afișează text nou, deschide cortinele.
        writer.SkipToEnd();
        yield return StartCoroutine(writer.StartTyping(storyTexts[currentScene - 1]));

        foreach (var curtain in curtains) {
            yield return StartCoroutine(curtain.DeschideCopertine());
        }

        SetState(GameState.Interaction);
    }

    private void DisableInteractions() {
        // Dezactivează manete și sfoară.
        DisableLevers();
        DisableRope();
    }

    private void EnableLevers() {
        // Activează manetele.
        foreach (var lever in levers) {
            lever.enabled = true;
        }
    }

    private void DisableLevers() {
        // Dezactivează manetele.
        foreach (var lever in levers) {
            lever.enabled = false;
        }
    }

    private void EnableRope() {
        // Activează sfoara.
        rope.enabled = true;
    }

    private void DisableRope() {
        // Dezactivează sfoara.
        rope.enabled = false;
    }

    private void ResetLevers() {
        // Resetează fiecare manetă.
        foreach (var lever in levers) {
            lever.Reset();
        }
    }
}