using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private AudioSource audioSource;

    void Awake()
    {
        // Empêche les doublons
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
