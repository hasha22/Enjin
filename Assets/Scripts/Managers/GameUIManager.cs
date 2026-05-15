using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
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
        GameScreens currentScreen = GameManager.instance.currentScreen;
        int currentRound = GameManager.instance.currentRound;
        int totalRounds = GameManager.instance.totalRounds;
        UpdateCurrentScreenNumber(currentScreen);

        if ((int)currentScreen >= 7)
        {
            GameManager.instance.currentScreen = GameScreens.SituationExplanationScreen;
            GameManager.instance.currentRound++;
            currentScreen = GameManager.instance.currentScreen;
            currentRound = GameManager.instance.currentRound;
            UpdateCurrentScreenNumber(currentScreen);
            if (currentRound > totalRounds) { SceneManager.LoadScene("OutcomeScene"); return; }
            roundIndicator.text = $"{currentRound}/{totalRounds}";
        }

        //Turns the correct screen on and the rest off
        for (int i = 0; i < allScreens.Count; i++)
        {
            if (i == (int)currentScreen)
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
        if ((int)currentScreen == 0 || (int)currentScreen == 6) { headers.SetActive(false); } else { headers.SetActive(true); }
        //Turns timer & continue button off or on
        if ((int)currentScreen == 2 || (int)currentScreen == 5)
        {
            keywordContainers.SetActive(true);
            timer.SetActive(true);
            TimerScript.instance.StartTimer(5);
            continueButton.SetActive(false);
        }
        else if ((int)currentScreen == 3)
        {
            keywordContainers.SetActive(true);
            timer.SetActive(true);
            TimerScript.instance.StartTimer(10);
            continueButton.SetActive(false);
        }
        else { keywordContainers.SetActive(false); timer.SetActive(false); continueButton.SetActive(true); }
        if ((int)currentScreen == 6) { ValueManager.instance.MakeBig(); } else { ValueManager.instance.MakeSmall(); }
    }

    public void SetLocalVariables()
    {

    }

    public void UpdateCurrentScreenNumber(GameScreens scrin)
    {
        GameManager.instance.currentScreenNumber = (int)scrin;
    }

}
