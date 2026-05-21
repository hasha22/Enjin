using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    //public static SceneTransitionManager instance { get; private set; }

    [Header("Hierarchy Objects")]
    [SerializeField] private GameObject crossfade;

    [Header("Animations")]
    [SerializeField] private Animator waitingSceneTransition;
    [SerializeField] private float animationDelay = 1f;

    public void LoadNextScene()
    {
        AudioManager.instance.StopBGM();
        AudioManager.instance.PlaySFX(AudioManager.instance.startGameSFX, 0.5f);
        StartCoroutine(LoadSceneWithAnimation(SceneManager.GetActiveScene().buildIndex + 1));
    }
    private IEnumerator LoadSceneWithAnimation(int index)
    {
        yield return new WaitForSeconds(1.5f);
        waitingSceneTransition.SetTrigger("Start");
        yield return new WaitForSeconds(animationDelay);

        SceneManager.LoadScene(index);
    }
}
