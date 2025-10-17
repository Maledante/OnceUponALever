// Modified GameManager.cs
// Changes:
// - Removed application of moveOffset, sortingOrder, and flipX from UpdateDragableObjectSettings, as these should only apply when a lever is pulled.
// - Kept SetMoveOffset in UpdateDragableObjectSettings to store the offset for use in LeverInteract.
// - Preserved the specific configurations in InitializePerSceneConfigurations for scenes 0, 1, 2, and 3, and defaults for others.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public LeverInteract[] levers;
    public RopeInteract rope;
    public CortineMovement[] curtains;
    public TypewriterEffect writer;

    public enum GameState {
        Intro,
        Interaction,
        Transition,
        End
    }

    public GameState currentState = GameState.Intro;
    private int leversPulledCount = 0;
    public int currentScene = 1;
    public string[] storyTexts = new string[]
    {
        "Once, a King with three Sons—Eldest, Middle, Youngest—ruled a castle grim. 'One heir must rise,' quoth he, as shadows whispered doom’s cold breath.",
        "A ghostly Messenger brought a letter. 'Emperor, thy brother, dieth,' it wailed. The King paled, the parchment’s curse chilling the throne room’s heart.",
        "'Sons—Eldest, Middle, Youngest!' cried the King. 'My brother seeks an heir. Prove thy worth, or death’s shadow claims us!' The court trembled in dread.",
        "In a haunted wood, Eldest, Middle, Youngest hunted a spectral Bear. 'Slay it!' roared the wind, as unseen eyes watched their fated trial.",
        "The Bear roared; Eldest and Middle fled. Youngest struck, but—O horror!—it was the King, slain by his son, his blood cursing the land.",
        "Youngest, stained by patricide, was chosen. 'To Emperor’s throne,' wailed Courtier. The King’s Ghost lingered, cursing the hall with eternal grief.",
        "On Horse pale as death, Youngest rode through fog. 'Father’s blood haunts me,' quoth he. Wraiths whispered betrayal, foretelling a journey damned.",
        "A Stranger, voice like serpents, begged water. Youngest shared, but was cast into a well. 'Thy mercy damns thee,' laughed the fiend, shadows coiling.",
        "In the well’s gloom, Youngest swore to Stranger: 'Silence, or death.' Freed, he rode, bound by an oath that chilled his soul with dread.",
        "At Emperor’s castle, Stranger, as Youngest, claimed the throne. Emperor, frail, welcomed him, unaware of the dark spirit lurking in his smile.",
        "Youngest, disguised, watched Stranger reign. Emperor’s Daughter, beloved, sickened under a curse. 'My oath binds me,' wept Youngest, as her life faded.",
        "Emperor died, whispering, 'Thou art false, Stranger.' The impostor’s eyes glowed, demonic. Youngest saw his beloved Daughter turn to ash, voiceless.",
        "Stranger, a demon unveiled, cursed Youngest to silence. 'Thy kin are dust!' it roared. The castle fell, trapping Youngest in endless, haunted sorrow."
    };
    public int[] requiredLeversPerScene;

    private DropzoneManager dropManager;

    [Header("Sprite Management")]
    public GameObject[] allSprites;
    public List<string>[] availableSpritesPerScene;
    public List<string>[] requiredSpritesPerScene;

    [Header("Per-Scene Configurations")]
    public Dictionary<string, Vector3>[] moveOffsetsPerScene;
    public Dictionary<string, int>[] characterSortingOrdersPerScene;
    public Dictionary<string, bool>[] flipSpritesPerScene;

    [Header("Sound Settings")]
    public AudioSource audioSource; // Assign in Inspector
    public AudioClip appearSound; // Assign your sound clip in Inspector

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
        InitializeSpriteData();
        InitializePerSceneConfigurations();
        UpdateDragableObjectSettings();
        StartCoroutine(StartGame());
    }

    private void InitializeSpriteData() {
        availableSpritesPerScene = new List<string>[storyTexts.Length];
        requiredSpritesPerScene = new List<string>[storyTexts.Length];
        availableSpritesPerScene[0] = new List<string> { "coroanaalbastraicon", "frate2icon", "harapicon", "frate1icon", "castelicon" };
        requiredSpritesPerScene[0] = new List<string> { "coroanaalbastraicon", "frate2icon", "harapicon", "frate1icon", "castelicon" };
        availableSpritesPerScene[1] = new List<string> { "coroanaalbastraicon", "frate2icon", "harapicon", "frate1icon", "castelicon", "mesagericon", "tronicon" };
        requiredSpritesPerScene[1] = new List<string> { "coroanaalbastraicon", "mesagericon", "tronicon" };
        availableSpritesPerScene[2] = new List<string> { "coroanaalbastraicon", "frate2icon", "harapicon", "frate1icon", "castelicon", "mesagericon", "tronicon" };
        requiredSpritesPerScene[2] = new List<string> { "coroanaalbastraicon", "frate2icon", "harapicon", "frate1icon", "tronicon" };
        availableSpritesPerScene[3] = new List<string> { "coroanaalbastraicon", "frate2icon", "harapicon", "frate1icon", "castelicon", "mesagericon", "tronicon", "copacicon" };
        requiredSpritesPerScene[3] = new List<string> { "frate2icon", "harapicon", "frate1icon", "copacicon" };

        for (int i = 4; i < storyTexts.Length; i++) {
            availableSpritesPerScene[i] = new List<string>(availableSpritesPerScene[0]);
            requiredSpritesPerScene[i] = new List<string>(requiredSpritesPerScene[0]);
        }
    }

    private void InitializePerSceneConfigurations() {
        moveOffsetsPerScene = new Dictionary<string, Vector3>[storyTexts.Length];
        characterSortingOrdersPerScene = new Dictionary<string, int>[storyTexts.Length];
        flipSpritesPerScene = new Dictionary<string, bool>[storyTexts.Length];

        // Initialize dictionaries for each scene
        for (int i = 0; i < storyTexts.Length; i++) {
            moveOffsetsPerScene[i] = new Dictionary<string, Vector3>();
            characterSortingOrdersPerScene[i] = new Dictionary<string, int>();
            flipSpritesPerScene[i] = new Dictionary<string, bool>();
        }

        // Scene 1 (index 0): Configurations for offsets, sorting orders, and flips
        moveOffsetsPerScene[0]["coroanaalbastraicon"] = new Vector3(5f, 0f, 0f);
        moveOffsetsPerScene[0]["frate2icon"] = new Vector3(4.67f, 0f, 0f);
        moveOffsetsPerScene[0]["harapicon"] = new Vector3(4.78f, 0f, 0f);
        moveOffsetsPerScene[0]["frate1icon"] = new Vector3(5.69f, 0f, 0f);
        moveOffsetsPerScene[0]["castelicon"] = new Vector3(9f, 0f, 0f);

        characterSortingOrdersPerScene[0]["coroanaalbastraicon"] = 1;
        characterSortingOrdersPerScene[0]["frate2icon"] = 2;
        characterSortingOrdersPerScene[0]["harapicon"] = 2;
        characterSortingOrdersPerScene[0]["frate1icon"] = 2;
        characterSortingOrdersPerScene[0]["castelicon"] = 1;

        // Scene 2 (index 1): Configurations for offsets, sorting orders, and flips
        moveOffsetsPerScene[1]["tronicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[1]["coroanaalbastraicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[1]["mesagericon"] = new Vector3(-9f, -1.5f, 0f);

        characterSortingOrdersPerScene[1]["tronicon"] = 1;
        characterSortingOrdersPerScene[1]["regeicon"] = 2;
        characterSortingOrdersPerScene[1]["mesagericon"] = 1;

        flipSpritesPerScene[1]["tronicon"] = false;
        flipSpritesPerScene[1]["regeicon"] = true; // Flip regeicon in scene 2
        flipSpritesPerScene[1]["mesagericon"] = true;

        // Scene 3 (index 2): Configurations for offsets, sorting orders, and flips
        moveOffsetsPerScene[2]["tronicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[2]["coroanaalbastraicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[2]["frate2icon"] = new Vector3(4.67f, 0f, 0f);
        moveOffsetsPerScene[2]["harapicon"] = new Vector3(4.78f, 0f, 0f);
        moveOffsetsPerScene[2]["frate1icon"] = new Vector3(5.69f, 0f, 0f);

        characterSortingOrdersPerScene[2]["tronicon"] = 1;
        characterSortingOrdersPerScene[2]["regeicon"] = 2;

        // Scene 4 (index 3): Configurations for offsets, sorting orders, and flips
        moveOffsetsPerScene[3]["copacicon"] = new Vector3(14f, 0f, 0f);
        moveOffsetsPerScene[3]["frate2icon"] = new Vector3(4.67f, 0f, 0f);
        moveOffsetsPerScene[3]["harapicon"] = new Vector3(4.78f, 0f, 0f);
        moveOffsetsPerScene[3]["frate1icon"] = new Vector3(5.69f, 0f, 0f);

        characterSortingOrdersPerScene[3]["copacicon"] = 1;
        characterSortingOrdersPerScene[3]["frate2icon"] = 2;
        characterSortingOrdersPerScene[3]["harapicon"] = 2;
        characterSortingOrdersPerScene[3]["frate1icon"] = 2;

        // Initialize remaining scenes with defaults
        for (int i = 4; i < storyTexts.Length; i++) {
            foreach (string spriteName in availableSpritesPerScene[i]) {
                moveOffsetsPerScene[i][spriteName] = new Vector3(5f, 0f, 0f); // Default offset
                characterSortingOrdersPerScene[i][spriteName] = 5; // Default sorting order
                flipSpritesPerScene[i][spriteName] = false; // Default no flip
            }
        }
    }

    private void UpdateDragableObjectSettings() {
        int sceneIndex = currentScene - 1;
        DragableObject[] allDragables = FindObjectsByType<DragableObject>(FindObjectsSortMode.None);
        foreach (var dragable in allDragables) {
            string spriteName = dragable.gameObject.name;
            // Only set the moveOffset for use in LeverInteract
            if (moveOffsetsPerScene[sceneIndex].ContainsKey(spriteName)) {
                dragable.SetMoveOffset(moveOffsetsPerScene[sceneIndex][spriteName]);
                Debug.Log($"Set moveOffset for {spriteName} to {moveOffsetsPerScene[sceneIndex][spriteName]} in scene {currentScene}");
            }
        }
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
                StartCoroutine(InteractionRoutine());
                break;
            case GameState.Transition:
                StartCoroutine(TransitionToNextScene());
                break;
            case GameState.End:
                break;
        }
    }

    private IEnumerator InteractionRoutine() {
        yield return StartCoroutine(ResetScene());
        yield return StartCoroutine(UpdateSpriteVisibilitySequential());
        UpdateDragableObjectSettings();
        EnableLevers();
        EnableRope();
    }

    private IEnumerator UpdateSpriteVisibilitySequential() {
        List<string> available = availableSpritesPerScene[currentScene - 1];
        List<GameObject> newSprites = new List<GameObject>();

        // Do not deactivate anything; only collect inactive available sprites
        foreach (string spriteName in available) {
            GameObject sprite = System.Array.Find(allSprites, s => s.name == spriteName);
            if (sprite != null && !sprite.activeSelf) {
                newSprites.Add(sprite);
            }
        }

        // Activate only new ones sequentially
        foreach (GameObject sprite in newSprites) {
            sprite.SetActive(true);
            if (audioSource != null && appearSound != null) {
                audioSource.PlayOneShot(appearSound);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void OnLeverPulled(LeverInteract lever) {
        leversPulledCount++;
    }

    public void OnRopePulled() {
        if (currentState == GameState.Interaction) {
            if (CheckCorrectSprites()) {
                SetState(GameState.Transition);
            }
            else {
                StartCoroutine(FakeTransition());
            }
        }
    }

    private bool CheckCorrectSprites() {
        List<string> placedSprites = new List<string>();
        foreach (LeverInteract lever in levers) {
            GameObject spriteAtPos = dropManager.GetObjectAtPosition((Vector2)lever.associatedPosition);
            if (spriteAtPos != null) {
                placedSprites.Add(spriteAtPos.name);
            }
        }
        List<string> required = requiredSpritesPerScene[currentScene - 1];
        if (placedSprites.Count != required.Count) return false;
        foreach (string req in required) {
            if (!placedSprites.Contains(req)) return false;
        }
        return true;
    }

    private IEnumerator FakeTransition() {
        foreach (var curtain in curtains) {
            StartCoroutine(curtain.InchideCopertine());
        }
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(ResetScene());
        foreach (var curtain in curtains) {
            StartCoroutine(curtain.DeschideCopertine());
        }
        SetState(GameState.Interaction);
    }

    private IEnumerator TransitionToNextScene() {
        foreach (var curtain in curtains) {
            StartCoroutine(curtain.InchideCopertine());
        }
        ResetLevers();
        leversPulledCount = 0;
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(ResetScene());
        foreach (var curtain in curtains) {
            StartCoroutine(curtain.DeschideCopertine());
        }
        currentScene++;
        if (currentScene > storyTexts.Length) {
            SetState(GameState.End);
            yield break;
        }
        UpdateDragableObjectSettings();
        yield return new WaitForSeconds(4f);
        writer.SkipToEnd();
        yield return StartCoroutine(writer.StartTyping(storyTexts[currentScene - 1]));
        SetState(GameState.Interaction);
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

    private IEnumerator ResetScene() {
        Debug.Log("Resetting scene...");
        ResetLevers();
        if (dropManager != null) {
            DragableObject[] allDragables = FindObjectsByType<DragableObject>(FindObjectsSortMode.None);
            foreach (var dragable in allDragables) {
                Debug.Log($"Resetting dragable {dragable.name}...");
                dropManager.RemoveObjectFromPosition((Vector2)dragable.transform.position);
                dragable.Unlock();
                dragable.ResetAssociatedObject(); // Sync reset
                dragable.ReturnToOriginal(); // Starts SmoothSnap coroutine
            }
            // Wait for all snaps to complete
            bool allSnapsDone = false;
            while (!allSnapsDone) {
                allSnapsDone = true;
                foreach (var dragable in allDragables) {
                    if (dragable.IsSnapping) {
                        allSnapsDone = false;
                        break;
                    }
                }
                yield return null;
            }
        }
        leversPulledCount = 0;
        Debug.Log("Scene reset complete.");
    }
}