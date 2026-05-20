using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    [Header("AudioSources")]
    public AudioSource sourcePrefab;
    public AudioSource continueButtonSource;
    public AudioSource typingSource;

    [Header("SFX")]
    public AudioClip buttonSfx;
    public AudioClip barSfx;
    public AudioClip typingSfx;

    public static GameAudioManager instance;

    void Awake()
    {
        if (instance == null){instance = this;}
        buttonSfx.LoadAudioData();
        continueButtonSource.clip = buttonSfx;
        typingSource.clip = typingSfx;
        typingSource.loop = true;
    }

    public void PlayButtonSfx()
    {
        continueButtonSource.volume = 0.5f;
        continueButtonSource.Play();
    }

    public void PlaySFX(AudioClip audioClip, float volume)
    {
        AudioSource audioSource = Instantiate(sourcePrefab, this.transform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        if (audioSource.clip == barSfx) {Debug.Log("what will you do now netanyahu"); audioSource.pitch = Random.Range(0.75f, 1.05f);}
        audioSource.Play();
        Destroy(audioSource.gameObject, audioSource.clip.length);
    }


}
