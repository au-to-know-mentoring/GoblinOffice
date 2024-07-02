using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    public Slider volumeSlider;
    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();

            if (SceneManager.GetActiveScene().name == "Game Over")
            {
                audioSource.pitch = .75f;
            }
            else
            {
                audioSource.pitch = 1f;
            }

            if (volumeSlider == null)
            {
                volumeSlider = GetComponentInChildren<Slider>();
            }

            // Subscribe to the sceneLoaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (volumeSlider != null)
        {
            // Set the slider's value to the current volume
            volumeSlider.value = audioSource.volume;

            // Add a listener to call the OnVolumeChange method whenever the slider's value changes
            volumeSlider.onValueChanged.AddListener(OnVolumeChange);
        }
    }

    void OnVolumeChange(float value)
    {
        // Set the volume of the AudioSource to the slider's value
        audioSource.volume = value;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Function to run when the scene changes
        Debug.Log("Scene loaded: " + scene.name);

        // Example: Adjust pitch based on the new scene
        if (scene.name == "Game Over")
        {
            audioSource.pitch = .75f;
        }
        else
        {
            audioSource.pitch = 1f;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from the sceneLoaded event to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}