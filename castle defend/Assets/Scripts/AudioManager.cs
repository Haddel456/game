using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioSource backgroundMusic;

    // Singleton pattern to ensure only one instance exists
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        // If an instance already exists, destroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set this as the instance and make it persistent
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure music continues playing
        if (backgroundMusic != null && !backgroundMusic.isPlaying)
        {
            backgroundMusic.Play();
        }
    }

    // Toggle music on/off
    public void ToggleMusic(bool isOn)
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.mute = !isOn;
            PlayerPrefs.SetInt("MusicMuted", isOn ? 0 : 1); // Save preference
        }
    }

    // Check if music is muted
    public bool IsMusicMuted()
    {
        return backgroundMusic != null && backgroundMusic.mute;
    }

}