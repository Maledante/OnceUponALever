using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Gestionază starea jocului, inclusiv resetarea sprite-urilor la tranziție falsă și vizibilitatea per scenă.
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public LeverInteract[] levers;
    public RopeInteract rope;
    public CortineMovement[] curtains;
    public TypewriterEffect writer;

    public enum GameState {
        Intro,
        Interaction,
        ReadyToPullRope,
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

    private DropzoneManager dropManager; // Corectat la DropZoneManager

    [Header("Sprite Management")]
    public GameObject[] allSprites; // All sprite GameObjects (DragableObject attached)
    public List<string>[] availableSpritesPerScene; // Names of available sprites per scene (cumulative)
    public List<string>[] requiredSpritesPerScene; // Names of required correct sprites per scene

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

        // Inițializează available și required sprites per scene
        InitializeSpriteData();

        StartCoroutine(StartGame());
    }

    private void InitializeSpriteData() {
        availableSpritesPerScene = new List<string>[storyTexts.Length];
        requiredSpritesPerScene = new List<string>[storyTexts.Length];

        // Scena 1: Exemplu cu "coroanaalbastraicon" (ajustează după nevoie)
        availableSpritesPerScene[0] = new List<string> { "coroanaalbastraicon" };
        requiredSpritesPerScene[0] = new List<string> { "coroanaalbastraicon" };

        // Scena 2: Adaugă "harapicon", "castelicon"
        availableSpritesPerScene[1] = new List<string>(availableSpritesPerScene[0]) { "harapicon", "castelicon" };
        requiredSpritesPerScene[1] = new List<string> { "coroanaalbastraicon", "harapicon", "castelicon" };
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
                UpdateSpriteVisibility(); // Actualizează vizibilitatea sprite-urilor
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

    private void UpdateSpriteVisibility() {
        List<string> available = availableSpritesPerScene[currentScene - 1];

        foreach (GameObject sprite in allSprites) {
            bool isAvailable = available.Contains(sprite.name);
            sprite.SetActive(isAvailable); // Activează doar sprite-urile disponibile pentru scena curentă
        }
    }

    public void OnLeverPulled(LeverInteract lever) {
        // Verifică dacă sprite-ul sub maneta trasă este corect
        GameObject spriteAtPos = dropManager.GetObjectAtPosition(lever.associatedPosition);
        if (spriteAtPos != null) {
            List<string> required = requiredSpritesPerScene[currentScene - 1];
            if (required.Contains(spriteAtPos.name)) {
                leversPulledCount++;
                if (leversPulledCount >= requiredLeversPerScene[currentScene - 1]) {
                    SetState(GameState.ReadyToPullRope);
                }
                return; // Succes, maneta trasă contează
            }
        }

        // Dacă nu e corect, resetează maneta (nu contează în count)
        lever.Reset();
    }

    public void OnRopePulled() {
        if (currentState == GameState.ReadyToPullRope) {
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
            GameObject spriteAtPos = dropManager.GetObjectAtPosition(lever.associatedPosition);
            if (spriteAtPos != null) {
                placedSprites.Add(spriteAtPos.name);
            }
        }

        List<string> required = requiredSpritesPerScene[currentScene - 1];

        // Verifică dacă toate required sunt plasate și nu sunt extra
        if (placedSprites.Count != required.Count) return false;
        foreach (string req in required) {
            if (!placedSprites.Contains(req)) return false;
        }
        return true;
    }

    private IEnumerator FakeTransition() {
        // Închide cortinele
        foreach (var curtain in curtains) {
            StartCoroutine(curtain.InchideCopertine());
        }

        yield return new WaitForSeconds(2f);

        // Resetează sprite-urile
        ResetScene();

        // Deschide cortinele
        foreach (var curtain in curtains) {
            StartCoroutine(curtain.DeschideCopertine());
        }

        // Revine la Interaction
        SetState(GameState.Interaction);
    }

    private IEnumerator TransitionToNextScene() {
        // Închide cortinele
        foreach (var curtain in curtains) {
            StartCoroutine(curtain.InchideCopertine());
        }

        ResetLevers();
        leversPulledCount = 0;

        yield return new WaitForSeconds(2f);

        // Deschide cortinele (fără animatorul cei3, conform cererii tale de simplificare)
        foreach (var curtain in curtains) {
            StartCoroutine(curtain.DeschideCopertine());
        }

        currentScene++;
        if (currentScene > storyTexts.Length) {
            SetState(GameState.End);
            yield break;
        }

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

    private void ResetScene() {
        // Resetează manetele și sprite-urile asociate
        ResetLevers();

        // Resetează toate sprite-urile
        if (dropManager != null) {
            DragableObject[] allDragables = Object.FindObjectsByType<DragableObject>(FindObjectsSortMode.None);
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