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
        StartCoroutine(LoadSceneWithAnimation(SceneManager.GetActiveScene().buildIndex + 1));
    }
    private IEnumerator LoadSceneWithAnimation(int index)
    {
        waitingSceneTransition.SetTrigger("Start");
        yield return new WaitForSeconds(animationDelay);

        SceneManager.LoadScene(index);
    }
}
