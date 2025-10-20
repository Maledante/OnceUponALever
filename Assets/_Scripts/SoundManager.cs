using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour {
    public static SoundManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer mainMixer; // Assign your MainMixer asset in Inspector

    [Header("Audio Sources")]
    public AudioSource bgmSource; // For background music (assign in Inspector or auto-create)
    public AudioSource sfxSource; // For sound effects (assign in Inspector or auto-create)

    [Header("Clips")]
    public AudioClip mainMenuBGM; // Example: Assign your main menu music clip
    // Add more clips as needed, e.g., for other scenes or SFX

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else {
            Destroy(gameObject); // Ensure only one instance
            return;
        }

        // Auto-create AudioSources if not assigned
        if (bgmSource == null) {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true; // Loop BGM by default
        }
        if (sfxSource == null) {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        // Assign mixer groups (drag from Mixer in Inspector or find by path)
        bgmSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("Music")[0];
        sfxSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("SFX")[0];

        // Load saved volumes
        SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 0f));
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 0f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 0f));
    }

    private void Start() {
        // Example: Play initial BGM
        PlayBGM(mainMenuBGM);
    }

    // Volume control methods (call from your settings UI)
    public void SetMasterVolume(float volume) {
        mainMixer.SetFloat("MasterVolume", volume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float volume) {
        mainMixer.SetFloat("MusicVolume", volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume) {
        mainMixer.SetFloat("SFXVolume", volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    // Play background music (with optional fade-in if you add coroutines later)
    public void PlayBGM(AudioClip clip) {
        if (clip != null && bgmSource.clip != clip) {
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }

    // Play a sound effect (one-shot)
    public void PlaySFX(AudioClip clip) {
        if (clip != null) {
            sfxSource.PlayOneShot(clip);
        }
    }
}