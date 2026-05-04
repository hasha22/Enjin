using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("Audio Prefab")]
    [SerializeField] private AudioSource SFXSource;

    [Header("Music")]
    [SerializeField] private AudioSource waitingSceneSource;
    [SerializeField] private AudioSource gameSceneSource;
    public AudioClip waitingSceneBGM;
    public AudioClip gameSceneBGM;
    [SerializeField][Range(0, 1f)] private float bgmVolume = 1f;
    [Header("SFX")]
    public AudioClip startGameSFX;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private void Start()
    {
        PlayBGM();
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            waitingSceneSource.clip = waitingSceneBGM;
            waitingSceneSource.volume = bgmVolume;
            waitingSceneSource.Play();
            waitingSceneSource.loop = true;
        }
        else if (scene.buildIndex == 1)
        {
            gameSceneSource.clip = gameSceneBGM;
            gameSceneSource.volume = bgmVolume;
            gameSceneSource.Play();
            gameSceneSource.loop = true;
        }

    }
    public void PlaySFX(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(SFXSource, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
        Destroy(audioSource.gameObject, audioSource.clip.length);
    }
    public void PlayBGM()
    {
        if (waitingSceneSource != null)
        {
            waitingSceneSource.clip = waitingSceneBGM;
            waitingSceneSource.volume = bgmVolume;
            waitingSceneSource.Play();
            waitingSceneSource.loop = true;
        }
        else if (gameSceneSource != null)
        {
            gameSceneSource.clip = gameSceneBGM;
            gameSceneSource.volume = bgmVolume;
            gameSceneSource.Play();
            gameSceneSource.loop = true;
        }
    }
    public void StopBGM()
    {
        waitingSceneSource.Stop();
    }
}
