using System;
using System.Collections;
using UnityEngine;
using TMPro;
using Object = UnityEngine.Object;

// Acest script gestionează efectul de typewriter pentru textul TMP (TextMeshPro).
// El afișează textul caracter cu caracter, cu pauze la punctuație.
[RequireComponent(typeof(TMP_Text))]
public class TypewriterEffect : MonoBehaviour {
    private TMP_Text _textBox; // Referință la componenta TMP_Text.
    private int _currentVisibleCharacterIndex; // Indexul caracterului curent vizibil.
    private Coroutine _typewriterCoroutine; // Corutina curentă pentru efectul typewriter.
    private bool _readyForNewText = true; // Flag dacă scriptul este gata pentru text nou.

    private WaitForSeconds _simpleDelay; // Delay standard între caractere.
    private WaitForSeconds _interpunctuationDelay; // Delay mai lung pentru punctuație.

    [Header("Typewriter Settings")]
    [SerializeField] private float charactersPerSecond = 20f; // Caractere pe secundă.
    [SerializeField] private float interpunctuationDelay = 0.5f; // Delay pentru punctuație.

    public static event Action CompleteTextRevealed; // Eveniment când textul este complet revelat.
    public static event Action<char> CharacterRevealed; // Eveniment pentru fiecare caracter revelat.

    private void Awake() {
        // Inițializează componenta TMP_Text și delay-urile.
        _textBox = GetComponent<TMP_Text>();
        _simpleDelay = new WaitForSeconds(1f / charactersPerSecond);
        _interpunctuationDelay = new WaitForSeconds(interpunctuationDelay);
    }

    private void OnEnable() {
        // Subscrie la evenimentul de schimbare text pentru a pregăti text nou.
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(PrepareForNewText);
    }

    private void OnDisable() {
        // Dezsubscrie de la evenimentul de schimbare text.
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(PrepareForNewText);
    }

    private void Start() {
        // Setează text inițial gol și forțează update mesh.
        _textBox.text = "";
        _textBox.ForceMeshUpdate();
    }

    private void PrepareForNewText(Object obj) {
        // Verifică dacă obiectul este textul curent și dacă este gata pentru text nou.
        if (obj != _textBox || !_readyForNewText)
            return;

        StartTyping(_textBox.text);
    }

    private IEnumerator Typewriter() {
        // Blochează text nou până la finalizare.
        _readyForNewText = false;
        _textBox.ForceMeshUpdate();
        var textInfo = _textBox.textInfo;

        // Ascunde toate caracterele inițial.
        _textBox.maxVisibleCharacters = 0;

        for (_currentVisibleCharacterIndex = 0;
             _currentVisibleCharacterIndex < textInfo.characterCount;
             _currentVisibleCharacterIndex++) {
            // Face vizibil caracterul curent.
            _textBox.maxVisibleCharacters = _currentVisibleCharacterIndex + 1;

            char c = textInfo.characterInfo[_currentVisibleCharacterIndex].character;

            // Pauză mai lungă pentru punctuație.
            if (c == '.' || c == ',' || c == '!' || c == '?' || c == ';' || c == ':')
                yield return _interpunctuationDelay;
            else
                yield return _simpleDelay;

            CharacterRevealed?.Invoke(c);
        }

        // Invocă evenimentul de finalizare și deblochează text nou.
        CompleteTextRevealed?.Invoke();
        _readyForNewText = true;
    }

    public IEnumerator StartTyping(string newText) {
        // Oprește corutina existentă dacă există.
        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);

        // Pregătește text nou și ascunde caracterele.
        _readyForNewText = false;
        _textBox.text = newText;
        _textBox.ForceMeshUpdate();
        _textBox.maxVisibleCharacters = 0;

        // Pornește corutina typewriter și așteaptă finalizarea ei.
        _typewriterCoroutine = StartCoroutine(Typewriter());
        yield return _typewriterCoroutine;
    }

    public void SkipToEnd() {
        // Oprește corutina și afișează tot textul imediat.
        if (_typewriterCoroutine != null) {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }

        _textBox.ForceMeshUpdate();
        _textBox.maxVisibleCharacters = _textBox.textInfo.characterCount;
        _readyForNewText = true;
        CompleteTextRevealed?.Invoke();
    }
}