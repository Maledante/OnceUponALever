using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public LeverInteract[] levers;
    public RopeInteract rope;
    public CortineMovement[] curtains;
    public TypewriterEffect writer;
    public Animator cei3;

    public enum GameState {
        Intro,
        Interaction,
        ReadyToPullRope,
        Transition,
        End
    }

    public GameState currentState = GameState.Intro;
    private int leversPulledCount = 0;
    private int currentScene = 1;
    public string[] storyTexts = new string[]
    {
        "There was once a King who had three sons. He ruled a fair land in a big castle",
        "Povestea scenei 2: Noi provocări aici."
    };
    public int[] requiredLeversPerScene;

    private DropzoneManager dropManager;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    void Start() {
        dropManager = FindAnyObjectByType<DropzoneManager>();
        if (dropManager == null) {
            Debug.LogError("DropZoneManager nu a fost găsit în scenă!");
        }

        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame() {
        if (FadeManager.Instance != null)
            yield return StartCoroutine(FadeManager.Instance.FadeIn());

        SetState(GameState.Intro);
        yield return StartCoroutine(writer.StartTyping(storyTexts[currentScene - 1]));
        SetState(GameState.Interaction);
    }

    private void SetState(GameState newState) {
        currentState = newState;
        switch (newState) {
            case GameState.Intro:
                DisableInteractions();
                break;
            case GameState.Interaction:
                ResetScene(); // Resetează scena la intrarea în Interaction
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
        leversPulledCount++;
        if (leversPulledCount >= requiredLeversPerScene[currentScene - 1]) {
            SetState(GameState.ReadyToPullRope);
        }
    }

    public void OnRopePulled() {
        if (currentState == GameState.ReadyToPullRope) {
            SetState(GameState.Transition);
        }
    }

    private IEnumerator TransitionToNextScene() {
        foreach (var curtain in curtains) {
            StartCoroutine(curtain.InchideCopertine());
        }

        ResetLevers();
        leversPulledCount = 0;

        yield return new WaitForSeconds(2f);

        foreach (var curtain in curtains) {
            StartCoroutine(curtain.DeschideCopertine());
        }
        cei3.SetBool("Sari", true);

        yield return new WaitForSeconds(3f);

        foreach (var curtain in curtains) {
            StartCoroutine(curtain.InchideCopertine());
        }
        yield return new WaitForSeconds(4f);
        cei3.SetBool("Sari", false);

        currentScene++;
        if (currentScene > storyTexts.Length) {
            SetState(GameState.End);
            yield break;
        }

        foreach (var curtain in curtains) {
            StartCoroutine(curtain.DeschideCopertine());
        }
        yield return new WaitForSeconds(4f);
        writer.SkipToEnd();
        yield return StartCoroutine(writer.StartTyping(storyTexts[currentScene - 1]));

        SetState(GameState.Interaction); // Va reseta scena din nou
    }

    private void DisableInteractions() {
        DisableLevers();
        DisableRope();
    }

    private void EnableLevers() {
        foreach (var lever in levers) {
            lever.enabled = true;
        }
    }

    private void DisableLevers() {
        foreach (var lever in levers) {
            lever.enabled = false;
        }
    }

    private void EnableRope() {
        rope.enabled = true;
    }

    private void DisableRope() {
        rope.enabled = false;
    }

    private void ResetLevers() {
        foreach (var lever in levers) {
            lever.Reset();
            lever.ResetSprite();
        }
    }

    private void ResetScene() {
        // Resetează manetele și sprite-urile asociate
        ResetLevers();

        // Resetează toate sprite-urile
        if (dropManager != null) {
            DragableObject[] allDragables = FindObjectsByType<DragableObject>(FindObjectsSortMode.None);
            foreach (var dragable in allDragables) {
                dropManager.RemoveObjectFromPosition((Vector2)dragable.transform.position);
                dragable.Unlock();
                dragable.ReturnToOriginal();
            }
        }

        // Resetează contorul de manete
        leversPulledCount = 0;
    }
}