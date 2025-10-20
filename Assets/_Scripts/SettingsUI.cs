using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour {
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start() {
        if (SoundManager.Instance == null) {
            Debug.LogWarning("SoundManager not found! Ensure it's loaded from MainMenu.");
            return;
        }

        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0f);

        musicSlider.onValueChanged.AddListener(SoundManager.Instance.SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SoundManager.Instance.SetSFXVolume);
    }
}