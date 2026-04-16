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

    private void Awake()
    {
        /*
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        crossfade.SetActive(false);
        */
    }

    public void LoadNextScene()
    {
        StartCoroutine(LoadSceneWithAnimation(SceneManager.GetActiveScene().buildIndex + 1));
    }
    private IEnumerator LoadSceneWithAnimation(int index)
    {
        waitingSceneTransition.SetTrigger("Start");
        /*
        switch (index)
        {
            case 0:
                Debug.Log("Wrong Scene Index");
                break;
            case 1:
                crossfade.SetActive(true);
                waitingSceneTransition.SetTrigger("Start");
                break;
        }
        */
        yield return new WaitForSeconds(animationDelay);

        SceneManager.LoadScene(index);
    }
}
