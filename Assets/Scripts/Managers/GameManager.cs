using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [Header("Variables")]
    public List<Topic> allTopics = new List<Topic>();
    [SerializeField] private Topic currentTopic;
    [SerializeField] private Policy currentPolicy; //Discuss with daniel if this is necessary, since you can just access it from currenttopic anyway
    [SerializeField] private int currentScreen;
    [SerializeField] private int currentRound;

    [Header("Main references")]
    public List<GameObject> allScreens = new List<GameObject>();

    [Header("UI references")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI topText;
    public GameObject headers;
    public GameObject keywordContainers;
    public GameObject posContainer;
    public GameObject negContainer;
    public GameObject timer;

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

    void Start()
    {
        //TimerScript.instance.StartTimer(62);
    }

    public void NextScreen()
    {
        currentScreen++;
        if (currentScreen >= 5)
        {
            currentScreen = 1;
        }
        for (int i = 0; i < allScreens.Count; i++)
        {
            if (i == currentScreen)
            {
                allScreens[i].SetActive(true);
            }
            else
            {
                allScreens[i].SetActive(false);
            }
        }
        if (currentScreen != 0) { headers.SetActive(true); } else { headers.SetActive(false); }
        if (currentScreen == 2 || currentScreen == 3)
        {
            keywordContainers.SetActive(true);
            timer.SetActive(true);
            TimerScript.instance.StartTimer(10);
        }
        else { keywordContainers.SetActive(false); timer.SetActive(false); }
    }

    public void TimesUp()
    {
        NextScreen();
    }
}
