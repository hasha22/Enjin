using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [Header("Variables")]
    public List<Topic> allTopics = new List<Topic>();
    [SerializeField] private Topic currentTopic;
    [SerializeField] private Policy currentPolicy;
    [SerializeField] private int currentScreen;

    [Header("Main references")]
    public List<GameObject> allScreens = new List<GameObject>();
    public GameObject timer;
    public TextMeshProUGUI timerText;

    [Header("UI references")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI topText;
    public GameObject headers;
    public GameObject keywordContainers;
    public GameObject posContainer;
    public GameObject negContainer;

    [Header("Prefab references")]
    public GameObject keywordCardPrefab;
    public GameObject playerIcon;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void NextScreen()
    {
        currentScreen++;
        if (currentScreen >= 5)
        {
            currentScreen = 1;
        }
        for (int i = 0; i < allScreens.Count; i++){
            if (i == currentScreen)
            {
                allScreens[i].SetActive(true);
            }
            else
            {
                allScreens[i].SetActive(false);
            }
        }
        if (currentScreen != 0){headers.SetActive(true);} else {headers.SetActive(false);}
        if (currentScreen == 2 || currentScreen == 3){keywordContainers.SetActive(true);} else {keywordContainers.SetActive(false);}
    }
}
