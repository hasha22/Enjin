using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;

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
    public GameObject discussionPlayerIcon;
    public GameObject votingPlayerIcon;

    public void Awake()
    {
        if (instance == null) { instance = this; }

    }
    void Start()
    {
        roundIndicator.text = $"{GameManager.instance.currentRound}/{GameManager.instance.totalRounds}";
    }
    public void NextScreen()
    {
        GameManager.instance.currentScreen++;
        int currentScreen = GameManager.instance.currentScreen;
        int currentRound = GameManager.instance.currentRound;
        int totalRounds = GameManager.instance.totalRounds;

        if (currentScreen >= 7)
        {
            GameManager.instance.currentScreen = 1;
            GameManager.instance.currentRound++;
            currentScreen = GameManager.instance.currentScreen;
            currentRound = GameManager.instance.currentRound;
            if (currentRound > totalRounds) { SceneManager.LoadScene("OutcomeScene"); return; }
            roundIndicator.text = $"{currentRound}/{totalRounds}";

        }

        //Turns the correct screen on and the rest off
        for (int i = 0; i < allScreens.Count; i++)
        {
            if (i == currentScreen)
            {
                allScreens[i].SetActive(true);
                GameManager.instance.UpdateData();
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
            TimerScript.instance.StartTimer(5);
            continueButton.SetActive(false);
        }
        else if (currentScreen == 3)
        {
            keywordContainers.SetActive(true);
            timer.SetActive(true);
            TimerScript.instance.StartTimer(10);
            continueButton.SetActive(false);
        }
        else { keywordContainers.SetActive(false); timer.SetActive(false); continueButton.SetActive(true); }
        if (currentScreen == 6) { ValueManager.instance.MakeBig(); } else { ValueManager.instance.MakeSmall(); }
    }

    public void SetLocalVariables()
    {

    }

}
