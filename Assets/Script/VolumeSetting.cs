using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        // Set the slider to the current volume on start
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetVolume();
        }
    }

    public void SetVolume()
    {
        float volume = volumeSlider.value;
        // The mixer uses a logarithmic scale (-80dB to 20dB)
        // We convert the 0-1 slider value to decibels
        myMixer.SetFloat("MasterVol", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    private void LoadVolume()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("musicVolume");
        SetVolume();
    }
}