using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Layout Group References")]
    [SerializeField] private Transform agreeGroup1;
    [SerializeField] private Transform agreeGroup2;
    [SerializeField] private Transform mostlyAgreeGroup1;
    [SerializeField] private Transform mostlyAgreeGroup2;
    [SerializeField] private Transform neutralGroup1;
    [SerializeField] private Transform neutralGroup2;
    [SerializeField] private Transform mostlyDisagreeGroup1;
    [SerializeField] private Transform mostlyDisagreeGroup2;
    [SerializeField] private Transform disagreeGroup1;
    [SerializeField] private Transform disagreeGroup2;


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
            InstantiatePlayerVoteIcons();
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
    public void InstantiatePlayerVoteIcons()
    {
        if (NetworkManager.instance.allPlayers.Count == 0) return;

        foreach (GameObject playerObject in NetworkManager.instance.allPlayers)
        {
            Player playerScript = playerObject.GetComponent<Player>();
            VoteTypes playerVote = playerScript.GetFirstVote();

            switch (playerVote)
            {
                case VoteTypes.Agree:
                    if ((agreeGroup1.childCount + agreeGroup2.childCount) % 2 == 0)
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, agreeGroup1);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                    }
                    else
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, agreeGroup2);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                    }
                    break;
                case VoteTypes.MostlyAgree:
                    if ((mostlyAgreeGroup1.childCount + mostlyAgreeGroup2.childCount) % 2 == 0)
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, mostlyAgreeGroup1);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                    }
                    else
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, mostlyAgreeGroup2);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                    }
                    break;
                case VoteTypes.Neutral:
                    if ((neutralGroup1.childCount + neutralGroup2.childCount) % 2 == 0)
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, neutralGroup1);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                    }
                    else
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, neutralGroup2);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                    }
                    break;
                case VoteTypes.MostlyDisagree:
                    if ((mostlyDisagreeGroup1.childCount + mostlyAgreeGroup2.childCount) % 2 == 0)
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, mostlyDisagreeGroup1);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                    }
                    else
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, mostlyDisagreeGroup2);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                    }
                    break;
                case VoteTypes.Disagree:
                    if ((disagreeGroup1.childCount + disagreeGroup2.childCount) % 2 == 0)
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, disagreeGroup1);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                    }
                    else
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, disagreeGroup2);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                    }
                    break;
            }
        }
    }

}
