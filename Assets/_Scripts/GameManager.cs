using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public LeverInteract[] levers;
    public RopeInteract rope;
    public CortineMovement[] curtains;
    public TypewriterEffect writer;
    public GameObject imageToFade;

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
        ".",
        ".",
        ".",
        ".",
        ".",
        ".",
        ".",
        ".",
        ".",
        "."
    };
    public int[] requiredLeversPerScene;

    private DropzoneManager dropManager;

    [Header("Sprite Management")]
    public GameObject[] allSprites;
    public List<string>[] availableSpritesPerScene;
    public List<string>[] requiredSpritesPerScene;
    public List<string>[] newSpritesPerScene;

    [Header("Per-Scene Configurations")]
    public Dictionary<string, Vector3>[] moveOffsetsPerScene;
    public Dictionary<string, int>[] characterSortingOrdersPerScene;
    public Dictionary<string, bool>[] flipSpritesPerScene;

    [Header("Sound Settings")]
    public AudioSource audioSource; // Assign in Inspector
    public AudioClip appearSound; // Assign your sound clip in Inspector

    private GameObject nextArrow;
    private GameObject previousArrow;

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
        nextArrow = GameObject.Find("NextArrow");
        previousArrow = GameObject.Find("PreviousArrow");
        if (nextArrow == null || previousArrow == null) {
            Debug.LogWarning("NextArrow or PreviousArrow not found. Arrows will not be managed.");
        }
        InitializeSpriteData();
        InitializePerSceneConfigurations();
        DeactivateAllSpritesAndArrows();
        UpdateDragableObjectSettings();
        StartCoroutine(StartGame());
    }

    private void InitializeSpriteData() {
        availableSpritesPerScene = new List<string>[storyTexts.Length];
        requiredSpritesPerScene = new List<string>[storyTexts.Length];
        newSpritesPerScene = new List<string>[storyTexts.Length];
        List<string> cumulativeAvailable = new List<string>();

        for (int i = 0; i < storyTexts.Length; i++) {
            List<string> newForScene;
            switch (i) {
                case 0:
                    newForScene = new List<string> { "coroanaalbastraicon", "frate2icon", "harapicon", "frate1icon", "castelicon" };
                    requiredSpritesPerScene[i] = new List<string> { "coroanaalbastraicon", "frate2icon", "harapicon", "frate1icon", "castelicon" };
                    break;
                case 1:
                    newForScene = new List<string> { "mesagericon", "tronicon" };
                    requiredSpritesPerScene[i] = new List<string> { "coroanaalbastraicon", "mesagericon", "tronicon" };
                    break;
                case 2:
                    newForScene = new List<string>() { };
                    requiredSpritesPerScene[i] = new List<string> { "coroanaalbastraicon", "frate2icon", "harapicon", "frate1icon", "tronicon" };
                    break;
                case 3:
                    newForScene = new List<string> { "copacicon" };
                    requiredSpritesPerScene[i] = new List<string> { "frate2icon", "harapicon", "frate1icon", "copacicon" };
                    break;
                case 4:
                    newForScene = new List<string>() { "bushicon", "ursicon" };
                    requiredSpritesPerScene[i] = new List<string> { "harapicon", "ursicon", "bushicon", "copacicon" };
                    break;
                case 5:
                    newForScene = new List<string>() { "regeursicon" };
                    requiredSpritesPerScene[i] = new List<string> { "harapicon", "regeursicon", "bushicon", "copacicon" };
                    break;
                case 6:
                    newForScene = new List<string>() { "fantanaicon", "spanicon" };
                    requiredSpritesPerScene[i] = new List<string> { "harapicon", "fantanaicon", "spanicon", "copacicon" };
                    break;
                case 7:
                    newForScene = new List<string>() { };
                    requiredSpritesPerScene[i] = new List<string> { "harapicon", "fantanaicon", "spanicon", "copacicon" };
                    break;
                case 8:
                    newForScene = new List<string>() { "castelverdeicon" };
                    requiredSpritesPerScene[i] = new List<string> { "castelverdeicon", "spanicon", "harapicon" };
                    break;
                case 9:
                    newForScene = new List<string>() { "tronverdeicon", "coroanaverdeicon" };
                    requiredSpritesPerScene[i] = new List<string> { "tronverdeicon", "coroanaverdeicon", "spanicon", "harapicon" };
                    break;
                case 10:
                    newForScene = new List<string>() { "evilspanicon", "sangeicon" };
                    requiredSpritesPerScene[i] = new List<string> { "tronverdeicon", "sangeicon", "harapicon", "evilspanicon" }; ;
                    break;
                case 11:
                    newForScene = new List<string>() { };
                    requiredSpritesPerScene[i] = new List<string> { "tronverdeicon", "sangeicon", "harapicon", }; ;
                    break;
                default:
                    newForScene = new List<string> { };
                    requiredSpritesPerScene[i] = new List<string>(requiredSpritesPerScene[0]);
                    break;
            }
            newSpritesPerScene[i] = newForScene;
            cumulativeAvailable.AddRange(newForScene);
            availableSpritesPerScene[i] = new List<string>(cumulativeAvailable);
        }
    }

    private void InitializePerSceneConfigurations() {
        moveOffsetsPerScene = new Dictionary<string, Vector3>[storyTexts.Length];
        characterSortingOrdersPerScene = new Dictionary<string, int>[storyTexts.Length];
        flipSpritesPerScene = new Dictionary<string, bool>[storyTexts.Length];

        for (int i = 0; i < storyTexts.Length; i++) {
            moveOffsetsPerScene[i] = new Dictionary<string, Vector3>();
            characterSortingOrdersPerScene[i] = new Dictionary<string, int>();
            flipSpritesPerScene[i] = new Dictionary<string, bool>();
        }

        //Scena 0
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

        //Scena 1
        moveOffsetsPerScene[1]["tronicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[1]["coroanaalbastraicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[1]["mesagericon"] = new Vector3(-9f, -1.5f, 0f);
        moveOffsetsPerScene[1]["frate2icon"] = new Vector3(4.67f, 0f, 0f);
        moveOffsetsPerScene[1]["harapicon"] = new Vector3(4.78f, 0f, 0f);
        moveOffsetsPerScene[1]["frate1icon"] = new Vector3(5.69f, 0f, 0f);
        moveOffsetsPerScene[1]["castelicon"] = new Vector3(9f, 0f, 0f);

        characterSortingOrdersPerScene[1]["tronicon"] = 1;
        characterSortingOrdersPerScene[1]["coroanaalbastraicon"] = 2;
        characterSortingOrdersPerScene[1]["mesagericon"] = 1;
        characterSortingOrdersPerScene[1]["frate2icon"] = 2;
        characterSortingOrdersPerScene[1]["harapicon"] = 2;
        characterSortingOrdersPerScene[1]["frate1icon"] = 2;
        characterSortingOrdersPerScene[1]["castelicon"] = 1;

        flipSpritesPerScene[1]["tronicon"] = false;
        flipSpritesPerScene[1]["coroanaalbastraicon"] = true;
        flipSpritesPerScene[1]["mesagericon"] = true;

        //Scena 2
        moveOffsetsPerScene[2]["tronicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[2]["coroanaalbastraicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[2]["frate2icon"] = new Vector3(4.67f, 0f, 0f);
        moveOffsetsPerScene[2]["harapicon"] = new Vector3(4.78f, 0f, 0f);
        moveOffsetsPerScene[2]["frate1icon"] = new Vector3(5.69f, 0f, 0f);
        moveOffsetsPerScene[2]["castelicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[2]["mesagericon"] = new Vector3(-9f, -1.5f, 0f);

        characterSortingOrdersPerScene[2]["tronicon"] = 1;
        characterSortingOrdersPerScene[2]["coroanaalbastraicon"] = 2;

        //Scena 3
        moveOffsetsPerScene[3]["copacicon"] = new Vector3(14f, 0f, 0f);
        moveOffsetsPerScene[3]["frate2icon"] = new Vector3(4.67f, 0f, 0f);
        moveOffsetsPerScene[3]["harapicon"] = new Vector3(4.78f, 0f, 0f);
        moveOffsetsPerScene[3]["frate1icon"] = new Vector3(5.69f, 0f, 0f);
        moveOffsetsPerScene[3]["tronicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[3]["coroanaalbastraicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[3]["castelicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[3]["mesagericon"] = new Vector3(-9f, -1.5f, 0f);

        characterSortingOrdersPerScene[3]["copacicon"] = 1;
        characterSortingOrdersPerScene[3]["frate2icon"] = 2;
        characterSortingOrdersPerScene[3]["harapicon"] = 2;
        characterSortingOrdersPerScene[3]["frate1icon"] = 2;

        //Scena 4
        moveOffsetsPerScene[4]["copacicon"] = new Vector3(14f, 0f, 0f);
        moveOffsetsPerScene[4]["frate2icon"] = new Vector3(4.67f, 0f, 0f);
        moveOffsetsPerScene[4]["harapicon"] = new Vector3(4.78f, 0f, 0f);
        moveOffsetsPerScene[4]["frate1icon"] = new Vector3(5.69f, 0f, 0f);
        moveOffsetsPerScene[4]["bushicon"] = new Vector3(-6f, 0f, 0f);
        moveOffsetsPerScene[4]["ursicon"] = new Vector3(-7f, 0.5f, 0f);
        moveOffsetsPerScene[4]["cei3icon"] = new Vector3(4.78f, 0f, 0f);
        moveOffsetsPerScene[4]["tronicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[4]["coroanaalbastraicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[4]["castelicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[4]["mesagericon"] = new Vector3(-9f, -1.5f, 0f);

        characterSortingOrdersPerScene[4]["copacicon"] = 1;
        characterSortingOrdersPerScene[4]["frate2icon"] = 2;
        characterSortingOrdersPerScene[4]["harapicon"] = 2;
        characterSortingOrdersPerScene[4]["frate1icon"] = 2;
        characterSortingOrdersPerScene[4]["bushicon"] = 2;
        characterSortingOrdersPerScene[4]["ursicon"] = 3;
        characterSortingOrdersPerScene[4]["cei3icon"] = 2;


        //Scena 5
        moveOffsetsPerScene[5]["copacicon"] = new Vector3(14f, 0f, 0f);
        moveOffsetsPerScene[5]["frate2icon"] = new Vector3(4.67f, 0f, 0f);
        moveOffsetsPerScene[5]["harapicon"] = new Vector3(4.78f, 0f, 0f);
        moveOffsetsPerScene[5]["frate1icon"] = new Vector3(5.69f, 0f, 0f);
        moveOffsetsPerScene[5]["bushicon"] = new Vector3(-6f, 0f, 0f);
        moveOffsetsPerScene[5]["ursicon"] = new Vector3(-7f, 0.5f, 0f);
        moveOffsetsPerScene[5]["regeursicon"] = new Vector3(-10f, 0f, 0f);
        moveOffsetsPerScene[5]["tronicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[5]["coroanaalbastraicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[5]["castelicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[5]["mesagericon"] = new Vector3(-9f, -1.5f, 0f);

        characterSortingOrdersPerScene[5]["copacicon"] = 1;
        characterSortingOrdersPerScene[5]["frate2icon"] = 2;
        characterSortingOrdersPerScene[5]["harapicon"] = 2;
        characterSortingOrdersPerScene[5]["frate1icon"] = 2;
        characterSortingOrdersPerScene[5]["bushicon"] = 2;
        characterSortingOrdersPerScene[5]["ursicon"] = 3;
        characterSortingOrdersPerScene[5]["regeursicon"] = 3;

        //scena 6

        flipSpritesPerScene[6]["spanicon"] = true;

        moveOffsetsPerScene[6]["copacicon"] = new Vector3(14f, 0f, 0f);
        moveOffsetsPerScene[6]["frate2icon"] = new Vector3(4.67f, 0f, 0f);
        moveOffsetsPerScene[6]["harapicon"] = new Vector3(6f, 0f, 0f);
        moveOffsetsPerScene[6]["frate1icon"] = new Vector3(5.69f, 0f, 0f);
        moveOffsetsPerScene[6]["bushicon"] = new Vector3(-6f, 0f, 0f);
        moveOffsetsPerScene[6]["ursicon"] = new Vector3(-7f, 0.5f, 0f);
        moveOffsetsPerScene[6]["regeursicon"] = new Vector3(-10f, 0f, 0f);
        moveOffsetsPerScene[6]["fantanaicon"] = new Vector3(-11f, 0f, 0f);
        moveOffsetsPerScene[6]["spanicon"] = new Vector3(12.81f, -1.36f, 0f);
        moveOffsetsPerScene[6]["tronicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[6]["coroanaalbastraicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[6]["castelicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[6]["mesagericon"] = new Vector3(-9f, -1.5f, 0f);

        characterSortingOrdersPerScene[6]["copacicon"] = 1;
        characterSortingOrdersPerScene[6]["frate2icon"] = 2;
        characterSortingOrdersPerScene[6]["harapicon"] = 3;
        characterSortingOrdersPerScene[6]["frate1icon"] = 2;
        characterSortingOrdersPerScene[6]["bushicon"] = 2;
        characterSortingOrdersPerScene[6]["ursicon"] = 3;
        characterSortingOrdersPerScene[6]["regeursicon"] = 3;
        characterSortingOrdersPerScene[6]["fantanaicon"] = 2;
        characterSortingOrdersPerScene[6]["spanicon"] = 3;

        //scena 7
        moveOffsetsPerScene[7]["copacicon"] = new Vector3(14f, 0f, 0f);
        moveOffsetsPerScene[7]["frate2icon"] = new Vector3(4.67f, 0f, 0f);
        moveOffsetsPerScene[7]["harapicon"] = new Vector3(9f, 0.6f, 0f);
        moveOffsetsPerScene[7]["frate1icon"] = new Vector3(10f, 1f, 0f);
        moveOffsetsPerScene[7]["bushicon"] = new Vector3(-6f, 0f, 0f);
        moveOffsetsPerScene[7]["ursicon"] = new Vector3(-7f, 0.5f, 0f);
        moveOffsetsPerScene[7]["regeursicon"] = new Vector3(-10f, 0f, 0f);
        moveOffsetsPerScene[7]["fantanaicon"] = new Vector3(-11f, 0f, 0f);
        moveOffsetsPerScene[7]["spanicon"] = new Vector3(12.81f, -1.36f, 0f);
        moveOffsetsPerScene[7]["tronicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[7]["coroanaalbastraicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[7]["castelicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[7]["mesagericon"] = new Vector3(-9f, -1.5f, 0f);

        flipSpritesPerScene[7]["harapicon"] = true;

        characterSortingOrdersPerScene[7]["copacicon"] = 1;
        characterSortingOrdersPerScene[7]["frate2icon"] = 2;
        characterSortingOrdersPerScene[7]["harapicon"] = 3;
        characterSortingOrdersPerScene[7]["frate1icon"] = 2;
        characterSortingOrdersPerScene[7]["bushicon"] = 2;
        characterSortingOrdersPerScene[7]["ursicon"] = 3;
        characterSortingOrdersPerScene[7]["regeursicon"] = 3;
        characterSortingOrdersPerScene[7]["fantanaicon"] = 2;
        characterSortingOrdersPerScene[7]["spanicon"] = 3;

        //scena 8
        moveOffsetsPerScene[8]["copacicon"] = new Vector3(14f, 0f, 0f);
        moveOffsetsPerScene[8]["frate2icon"] = new Vector3(4.67f, 0f, 0f);
        moveOffsetsPerScene[8]["harapicon"] = new Vector3(5.28f, -0.5f, 0f);
        moveOffsetsPerScene[8]["frate1icon"] = new Vector3(10f, 1f, 0f);
        moveOffsetsPerScene[8]["bushicon"] = new Vector3(-6f, 0f, 0f);
        moveOffsetsPerScene[8]["ursicon"] = new Vector3(-7f, 0.5f, 0f);
        moveOffsetsPerScene[8]["regeursicon"] = new Vector3(-10f, 0f, 0f);
        moveOffsetsPerScene[8]["fantanaicon"] = new Vector3(-11f, 0f, 0f);
        moveOffsetsPerScene[8]["spanicon"] = new Vector3(12f, -1.3f, 0f);
        moveOffsetsPerScene[8]["tronicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[8]["coroanaalbastraicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[8]["castelicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[8]["mesagericon"] = new Vector3(-9f, -1.5f, 0f);
        moveOffsetsPerScene[8]["castelverdeicon"] = new Vector3(-10f, 0f, 0f);

        characterSortingOrdersPerScene[8]["copacicon"] = 1;
        characterSortingOrdersPerScene[8]["frate2icon"] = 2;
        characterSortingOrdersPerScene[8]["harapicon"] = 3;
        characterSortingOrdersPerScene[8]["frate1icon"] = 2;
        characterSortingOrdersPerScene[8]["bushicon"] = 2;
        characterSortingOrdersPerScene[8]["ursicon"] = 3;
        characterSortingOrdersPerScene[8]["regeursicon"] = 3;
        characterSortingOrdersPerScene[8]["fantanaicon"] = 2;
        characterSortingOrdersPerScene[8]["spanicon"] = 3;

        //scena 9
        moveOffsetsPerScene[9]["copacicon"] = new Vector3(14f, 0f, 0f);
        moveOffsetsPerScene[9]["frate2icon"] = new Vector3(4.67f, 0f, 0f);
        moveOffsetsPerScene[9]["harapicon"] = new Vector3(5f, -0.42f, 0f);
        moveOffsetsPerScene[9]["frate1icon"] = new Vector3(10f, 1f, 0f);
        moveOffsetsPerScene[9]["bushicon"] = new Vector3(-6f, 0f, 0f);
        moveOffsetsPerScene[9]["ursicon"] = new Vector3(-7f, 0.5f, 0f);
        moveOffsetsPerScene[9]["regeursicon"] = new Vector3(-10f, 0f, 0f);
        moveOffsetsPerScene[9]["fantanaicon"] = new Vector3(-11f, 0f, 0f);
        moveOffsetsPerScene[9]["spanicon"] = new Vector3(11f, -1.3f, 0f);
        moveOffsetsPerScene[9]["tronicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[9]["coroanaalbastraicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[9]["castelicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[9]["mesagericon"] = new Vector3(-9f, -1.5f, 0f);
        moveOffsetsPerScene[9]["castelverdeicon"] = new Vector3(-10f, 0f, 0f);
        moveOffsetsPerScene[9]["tronverdeicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[9]["coroanaverdeicon"] = new Vector3(-11f, 0f, 0f);

        characterSortingOrdersPerScene[9]["copacicon"] = 1;
        characterSortingOrdersPerScene[9]["frate2icon"] = 2;
        characterSortingOrdersPerScene[9]["harapicon"] = 3;
        characterSortingOrdersPerScene[9]["frate1icon"] = 2;
        characterSortingOrdersPerScene[9]["bushicon"] = 2;
        characterSortingOrdersPerScene[9]["ursicon"] = 3;
        characterSortingOrdersPerScene[9]["regeursicon"] = 3;
        characterSortingOrdersPerScene[9]["fantanaicon"] = 2;
        characterSortingOrdersPerScene[9]["spanicon"] = 3;

        //scena 10
        moveOffsetsPerScene[10]["copacicon"] = new Vector3(14f, 0f, 0f);
        moveOffsetsPerScene[10]["frate2icon"] = new Vector3(4.67f, 0f, 0f);
        moveOffsetsPerScene[10]["harapicon"] = new Vector3(13f, -0.42f, 0f);
        moveOffsetsPerScene[10]["frate1icon"] = new Vector3(10f, 1f, 0f);
        moveOffsetsPerScene[10]["bushicon"] = new Vector3(-6f, 0f, 0f);
        moveOffsetsPerScene[10]["ursicon"] = new Vector3(-7f, 0.5f, 0f);
        moveOffsetsPerScene[10]["regeursicon"] = new Vector3(-10f, 0f, 0f);
        moveOffsetsPerScene[10]["fantanaicon"] = new Vector3(-11f, 0f, 0f);
        moveOffsetsPerScene[10]["spanicon"] = new Vector3(11f, -1.3f, 0f);
        moveOffsetsPerScene[10]["tronicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[10]["coroanaalbastraicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[10]["castelicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[10]["mesagericon"] = new Vector3(-9f, -1.5f, 0f);
        moveOffsetsPerScene[10]["castelverdeicon"] = new Vector3(-10f, 0f, 0f);
        moveOffsetsPerScene[10]["tronverdeicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[10]["coroanaverdeicon"] = new Vector3(-11f, 0f, 0f);
        moveOffsetsPerScene[10]["evilspanicon"] = new Vector3(11f, -1.3f, 0f);
        moveOffsetsPerScene[10]["sangeicon"] = new Vector3(-12f, -0.3f, 0f);

        flipSpritesPerScene[10]["harapicon"] = true;

        characterSortingOrdersPerScene[10]["copacicon"] = 1;
        characterSortingOrdersPerScene[10]["frate2icon"] = 2;
        characterSortingOrdersPerScene[10]["harapicon"] = 3;
        characterSortingOrdersPerScene[10]["frate1icon"] = 2;
        characterSortingOrdersPerScene[10]["bushicon"] = 2;
        characterSortingOrdersPerScene[10]["ursicon"] = 3;
        characterSortingOrdersPerScene[10]["regeursicon"] = 3;
        characterSortingOrdersPerScene[10]["fantanaicon"] = 2;
        characterSortingOrdersPerScene[10]["spanicon"] = 3;
        characterSortingOrdersPerScene[10]["tronicon"] = 2;
        characterSortingOrdersPerScene[10]["sangeicon"] = 3;

        //scena 11
        moveOffsetsPerScene[11]["copacicon"] = new Vector3(14f, 0f, 0f);
        moveOffsetsPerScene[11]["frate2icon"] = new Vector3(4.67f, 0f, 0f);
        moveOffsetsPerScene[11]["harapicon"] = new Vector3(13f, -0.42f, 0f);
        moveOffsetsPerScene[11]["frate1icon"] = new Vector3(10f, 1f, 0f);
        moveOffsetsPerScene[11]["bushicon"] = new Vector3(-6f, 0f, 0f);
        moveOffsetsPerScene[11]["ursicon"] = new Vector3(-7f, 0.5f, 0f);
        moveOffsetsPerScene[11]["regeursicon"] = new Vector3(-10f, 0f, 0f);
        moveOffsetsPerScene[11]["fantanaicon"] = new Vector3(-11f, 0f, 0f);
        moveOffsetsPerScene[11]["spanicon"] = new Vector3(11f, -1.3f, 0f);
        moveOffsetsPerScene[11]["tronicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[11]["coroanaalbastraicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[11]["castelicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[11]["mesagericon"] = new Vector3(-9f, -1.5f, 0f);
        moveOffsetsPerScene[11]["castelverdeicon"] = new Vector3(-10f, 0f, 0f);
        moveOffsetsPerScene[11]["tronverdeicon"] = new Vector3(9f, 0f, 0f);
        moveOffsetsPerScene[11]["coroanaverdeicon"] = new Vector3(-11f, 0f, 0f);
        moveOffsetsPerScene[11]["evilspanicon"] = new Vector3(11f, -1.3f, 0f);
        moveOffsetsPerScene[11]["sangeicon"] = new Vector3(-12f, -0.3f, 0f);

        flipSpritesPerScene[11]["harapicon"] = true;

        characterSortingOrdersPerScene[11]["copacicon"] = 1;
        characterSortingOrdersPerScene[11]["frate2icon"] = 2;
        characterSortingOrdersPerScene[11]["harapicon"] = 3;
        characterSortingOrdersPerScene[11]["frate1icon"] = 2;
        characterSortingOrdersPerScene[11]["bushicon"] = 2;
        characterSortingOrdersPerScene[11]["ursicon"] = 3;
        characterSortingOrdersPerScene[11]["regeursicon"] = 3;
        characterSortingOrdersPerScene[11]["fantanaicon"] = 2;
        characterSortingOrdersPerScene[11]["spanicon"] = 3;
        characterSortingOrdersPerScene[11]["tronicon"] = 2;
        characterSortingOrdersPerScene[11]["sangeicon"] = 3;

        for (int i = 12; i < storyTexts.Length; i++) {
            foreach (string spriteName in availableSpritesPerScene[i]) {
                moveOffsetsPerScene[i][spriteName] = new Vector3(5f, 0f, 0f);
                characterSortingOrdersPerScene[i][spriteName] = 5;
                flipSpritesPerScene[i][spriteName] = false;
            }
        }
    }

    private void DeactivateAllSpritesAndArrows() {
        foreach (GameObject sprite in allSprites) {
            if (sprite != null) {
                sprite.SetActive(false);
                Debug.Log($"Deactivated sprite {sprite.name} at start.");
            }
        }
        if (nextArrow != null) {
            nextArrow.SetActive(false);
            Debug.Log("Deactivated NextArrow at start.");
        }
        if (previousArrow != null) {
            previousArrow.SetActive(false);
            Debug.Log("Deactivated PreviousArrow at start.");
        }
    }

    private void UpdateDragableObjectSettings() {
        DragableObject[] dragables = FindObjectsByType<DragableObject>(FindObjectsSortMode.None);
        foreach (var dragable in dragables) {
            if (dragable.associatedObject != null) {
                int sceneIndex = Mathf.Clamp(currentScene - 1, 0, moveOffsetsPerScene.Length - 1);
                if (moveOffsetsPerScene[sceneIndex].ContainsKey(dragable.name)) {
                    dragable.SetMoveOffset(moveOffsetsPerScene[sceneIndex][dragable.name]);
                }
                else {
                    dragable.SetMoveOffset(Vector3.zero);
                }
                Debug.Log($"Updated {dragable.name}: moveOffset={(moveOffsetsPerScene[sceneIndex].ContainsKey(dragable.name) ? moveOffsetsPerScene[sceneIndex][dragable.name] : Vector3.zero)}");
            }
        }
    }

    private IEnumerator StartGame() {
        SetState(GameState.Intro);
        yield return null;
    }

    public void SetState(GameState newState) {
        currentState = newState;
        Debug.Log($"Game state changed to: {currentState}");
        switch (currentState) {
            case GameState.Intro:
                StartCoroutine(IntroRoutine());
                break;
            case GameState.Interaction:
                StartCoroutine(InteractionRoutine());
                break;
            case GameState.Transition:
                StartCoroutine(TransitionToNextScene());
                break;
            case GameState.End:
                StartCoroutine(EndGameRoutine());
                break;
        }
    }


    private IEnumerator EndGameRoutine() {
        // disable interactions and arrows while fading
        DisableInteractions();
        DisableArrows();

        if (imageToFade != null) {
            imageToFade.SetActive(true);

            // ensure image starts transparent
            Image img = imageToFade.GetComponent<Image>();
            Color col = img.color;
            col.a = 0f;
            img.color = col;

            float fadeDuration = 2f;
            float t = 0f;
            while (t < fadeDuration) {
                t += Time.deltaTime;
                col.a = Mathf.Clamp01(t / fadeDuration);
                img.color = col;
                yield return null;
            }
            col.a = 1f;
            img.color = col;
        }

        // keep the image on screen for 10 seconds
        yield return new WaitForSeconds(25f);

        // go back to main menu
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator IntroRoutine() {
        writer.SkipToEnd();
        yield return StartCoroutine(writer.StartTyping(storyTexts[0]));
        SetState(GameState.Interaction);
    }

    private IEnumerator InteractionRoutine() {
        if (dropManager != null) {
            dropManager.SwitchToPage(dropManager.GetCurrentPage(), this);
        }
        UpdateDragableObjectSettings();
        EnableLevers();
        EnableRope();
        yield break;
    }

    public void OnLeverPulled(LeverInteract lever) {
        leversPulledCount++;
        Debug.Log($"Lever pulled: {lever.name}, Total levers pulled: {leversPulledCount}");
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
        int activatedCount = 0;

        List<string> required = requiredSpritesPerScene[currentScene - 1];

        foreach (LeverInteract lever in levers) {
            GameObject spriteAtPos = dropManager.GetObjectAtPosition((Vector2)lever.associatedPosition);
            if (spriteAtPos != null) {
                string spriteName = spriteAtPos.name;
                placedSprites.Add(spriteName);
                if (required.Contains(spriteName) && lever.IsActivated) {
                    activatedCount++;
                }
            }
        }

        if (placedSprites.Count != required.Count) {
            Debug.Log($"CheckCorrectSprites failed: Placed sprites ({placedSprites.Count}) != Required sprites ({required.Count})");
            return false;
        }
        foreach (string req in required) {
            if (!placedSprites.Contains(req)) {
                Debug.Log($"CheckCorrectSprites failed: Required sprite {req} not found in placed sprites");
                return false;
            }
        }

        if (activatedCount != required.Count) {
            Debug.Log($"CheckCorrectSprites failed: Only {activatedCount} of {required.Count} required levers are activated");
            return false;
        }

        Debug.Log("CheckCorrectSprites passed: All required sprites are placed and levers are activated");
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
        Debug.Log($"Resetting scene {currentScene}...");
        ResetLevers();

        if (dropManager != null) {
            // 1) Dezactivează toate obiectele care nu sunt pe pagina curentă
            int currentPageIndex = dropManager.GetCurrentPage();
            if (dropManager.pages != null) {
                for (int p = 0; p < dropManager.pages.Count; p++) {
                    if (p == currentPageIndex) continue;
                    Page page = dropManager.pages[p];
                    if (page.draggableObjects == null) continue;
                    foreach (GameObject obj in page.draggableObjects) {
                        if (obj == null) continue;
                        // curățăm orice referință din occupiedPositions pentru acest obiect
                        var occupied = dropManager.GetAllOccupiedPositions();
                        foreach (var kv in new List<KeyValuePair<Vector2, GameObject>>(occupied)) {
                            if (kv.Value == obj) {
                                dropManager.RemoveObjectFromPosition(kv.Key);
                            }
                        }
                        obj.SetActive(false);
                        Debug.Log($"Deactivated inventory object {obj.name} because it's not on current page {currentPageIndex}");
                    }
                }
            }

            // 2) Resetăm și repoziționăm obiectele active din scenă (doar cele încă active)
            DragableObject[] allDragables = FindObjectsByType<DragableObject>(FindObjectsSortMode.None);
            int sceneIndex = Mathf.Clamp(currentScene - 1, 0, requiredSpritesPerScene.Length - 1);
            foreach (var dragable in allDragables) {
                if (dragable == null) continue;
                if (!dragable.gameObject.activeSelf) {
                    // obiectele inactive deja nu trebuie procesate aici
                    continue;
                }

                bool shouldStayActive = requiredSpritesPerScene[sceneIndex].Contains(dragable.name) ||
                                       (currentScene > 1 && availableSpritesPerScene[sceneIndex].Contains(dragable.name));
                bool isUnderLever = dropManager.IsPositionOccupied((Vector2)dragable.transform.position);
                if (!isUnderLever) {
                    dropManager.RemoveObjectFromPosition((Vector2)dragable.transform.position);
                    dragable.Unlock();
                    dragable.ResetAssociatedObject();
                    dragable.ReturnToInitial();
                    if (!shouldStayActive) {
                        dragable.gameObject.SetActive(false);
                        Debug.Log($"Deactivated {dragable.name} because it's not required for scene {currentScene}");
                    }
                }
            }

            // 3) Așteptăm până termină toate snap-urile active
            bool allSnapsDone = false;
            while (!allSnapsDone) {
                allSnapsDone = true;
                foreach (var dragable in allDragables) {
                    if (dragable != null && dragable.IsSnapping) {
                        allSnapsDone = false;
                        break;
                    }
                }
                yield return null;
            }

            // 4) Reaplicăm pagina curentă (va activa doar obiectele disponibile pentru scenă)
            dropManager.SwitchToPage(dropManager.GetCurrentPage(), this);
        }

        leversPulledCount = 0;
    }

    private void DisableArrows() {
        if (nextArrow != null) {
            nextArrow.SetActive(false);
        }
        if (previousArrow != null) {
            previousArrow.SetActive(false);
        }
        Debug.Log($"Arrows hidden during sprite appearance in scene {currentScene}.");
    }

    public void EnableArrows() {
        if (nextArrow != null) {
            nextArrow.SetActive(dropManager.GetCurrentPage() < dropManager.pages.Count - 1);
        }
        if (previousArrow != null) {
            previousArrow.SetActive(dropManager.GetCurrentPage() > 0);
        }
        Debug.Log($"Arrows visible after sprite appearance in scene {currentScene}.");
    }

    public List<string> GetAvailableSpritesForScene(int sceneIndex) {
        sceneIndex = Mathf.Clamp(sceneIndex, 0, availableSpritesPerScene.Length - 1);
        return new List<string>(availableSpritesPerScene[sceneIndex]);
    }
}