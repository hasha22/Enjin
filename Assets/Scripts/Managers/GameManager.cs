using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Text References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI topText;
    public TextMeshProUGUI scenarioDescText;
    public TextMeshProUGUI enjinDescText;
    public TextMeshProUGUI roundIndicator;

    [Header("Visuals to turn off and on references")]
    public GameObject headers;
    public GameObject keywordContainers;
    public GameObject timer;
    public GameObject continueButton;

    [Header("Container references")]
    public GameObject posContainer;
    public GameObject negContainer;
    public GameObject posVotes;
    public GameObject negVotes;

    [Header("Prefab references")]
    public GameObject keywordCardPrefab;
    public GameObject playerIcon;

    [Header("Settings")]
    public int votingTime;
    public int discussionTime;
    public int totalRounds;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        roundIndicator.text = $"{currentRound}/{totalRounds}";
    }

    public void NextScreen()
    {
        currentScreen++;
        if (currentScreen >= 7)
        {
            currentScreen = 1;
            currentRound++;
            if (currentRound > totalRounds) { SceneManager.LoadScene("OutcomeScene"); return; }
            roundIndicator.text = $"{currentRound}/{totalRounds}";
        }
        //Turns the correct screen on and the rest off
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
        //Turns headers off or on
        if (currentScreen == 0 || currentScreen == 6) { headers.SetActive(false); } else { headers.SetActive(true); }
        //Turns timer & continue button off or on
        if (currentScreen == 2 || currentScreen == 5)
        {
            keywordContainers.SetActive(true);
            timer.SetActive(true);
            continueButton.SetActive(false);
            TimerScript.instance.StartTimer(votingTime);
        }
        else if (currentScreen == 3)
        {
            keywordContainers.SetActive(true);
            timer.SetActive(true);
            continueButton.SetActive(false);
            TimerScript.instance.StartTimer(discussionTime);
        }
        else { keywordContainers.SetActive(false); timer.SetActive(false); continueButton.SetActive(true); }
        if (currentScreen == 6) { ValueManager.instance.MakeBig(); } else { ValueManager.instance.MakeSmall(); }
    }

    public void TimesUp()
    {
        NextScreen();
    }
}
