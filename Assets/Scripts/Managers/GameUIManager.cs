using System.Collections.Generic;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;

    [Header("Screeb References")]
    public List<GameObject> allScreens = new List<GameObject>();

    [Header("Text References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI topText;
    public TextMeshProUGUI scenarioDescText;
    [SerializeField] private TextMeshProUGUI enjinDescText;
    [SerializeField] private TextMeshProUGUI roundIndicator;
    [SerializeField] private TextMeshProUGUI voting1Title;
    [SerializeField] private TextMeshProUGUI enjinVersionText;
    [SerializeField] private TextMeshProUGUI enjinVersionDescriptionText;

    [Header("Keyword References")]
    [SerializeField] private GameObject voting1Screen;
    [SerializeField] private GameObject voting2Screen;
    [SerializeField] private Transform discussionKeywordContainer;
    [SerializeField] private Color32 innovation = new Color32(0, 255, 181, 255);
    [SerializeField] private Color32 riskManagement = new Color32(255, 0, 111, 255);
    [SerializeField] private Color32 workerSatisfaction = new Color32(155, 0, 243, 255);
    [SerializeField] private Color32 profit = new Color32(255, 241, 0, 255);

    [Header("Enjin Update References")]
    [SerializeField] private GameObject enjinTitle;
    [SerializeField] private GameObject backgroundImage;
    private string enjinVersion;

    [Header("Nonscreen References")]
    [SerializeField] private GameObject headers;
    [SerializeField] private GameObject timer;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject iconCircle;

    [Header("Prefab References")]
    [SerializeField] private GameObject discussionKeywordCard;
    [SerializeField] private GameObject votingKeywordCard;
    [SerializeField] private GameObject discussionPlayerIcon;
    [SerializeField] private GameObject votingPlayerIcon;

    [Header("Discussion Round Layout Group References")]
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

    [Header("Voting Round Layout Group References")]
    [SerializeField] private Transform votingPosKeywordContainer;
    [SerializeField] private Transform votingNegKeywordContainer;

    [Header("List of players")]
    [SerializeField] public List<GameObject> allPlayerIcons = new List<GameObject>();


    private void Awake()
    {
        if (instance == null) { instance = this; }

    }
    private void Start()
    {
        roundIndicator.text = $"{GameManager.instance.currentRound}/{GameManager.instance.totalRounds}";

        //resetting automatically to screen 0 (character intro)
        foreach (GameObject screen in allScreens)
        {
            screen.SetActive(false);
        }
        allScreens[0].SetActive(true);
        headers.SetActive(false);
        timer.SetActive(false);
        continueButton.SetActive(true);
    }
    public void NextScreen()
    {
        GameManager.instance.currentScreen++;
        GameScreens currentScreen = GameManager.instance.currentScreen;
        int currentRound = GameManager.instance.currentRound;
        int totalRounds = GameManager.instance.totalRounds;
        UpdateCurrentScreenNumber(currentScreen);
        enjinVersion = $"{currentRound}.0";

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
        if (currentScreen == GameScreens.CharacterIntroScreen || currentScreen == GameScreens.ResultsScreen || currentScreen == GameScreens.EnjinUpdateScreen) 
        { headers.SetActive(false); } else { headers.SetActive(true); }

        //Turns timer & continue button off or on
        if (currentScreen == GameScreens.FirstPolicyVotingScreen || currentScreen == GameScreens.SecondPolicyVotingScreen)
        {
            if (currentScreen == GameScreens.FirstPolicyVotingScreen)
            {
                voting1Screen.SetActive(true);
                voting1Title.gameObject.SetActive(true);
                voting2Screen.SetActive(false);
                InstantiateKeywordCards();
            }
            else
            {
                voting1Screen.SetActive(false);
                voting1Title.gameObject.SetActive(false);
                voting2Screen.SetActive(true);
                InstantiateKeywordCards();
            }
            timer.SetActive(true);
            TimerScript.instance.StartTimer(GameManager.instance.votingTime);
            continueButton.SetActive(false);
        }
        else if (currentScreen == GameScreens.DiscussionScreen)
        {
            voting1Screen.SetActive(true);
            voting2Screen.SetActive(false);
            voting1Title.gameObject.SetActive(false);

            timer.SetActive(true);
            continueButton.SetActive(false);
            InstantiatePlayerVoteIcons();
            StartCoroutine(Discussion());
        }
        else if(currentScreen == GameScreens.EnjinUpdateScreen)
        {
            timer.SetActive(false);
            continueButton.SetActive(true);
            enjinTitle.SetActive(true);
            enjinVersionText.text = enjinVersion;
            enjinVersionDescriptionText.text = GameManager.instance.GetCurrentTopic().enjinPolicy.policyDescription;
        }
        else
        {
            voting1Screen.SetActive(false);
            voting2Screen.SetActive(false);
            timer.SetActive(false);
            continueButton.SetActive(true);
        }
        if (currentScreen == GameScreens.ResultsScreen) { ValueManager.instance.MakeBig(); } else { ValueManager.instance.MakeSmall(); }
    }

    public void UpdateCurrentScreenNumber(GameScreens scrin)
    {
        GameManager.instance.currentScreenNumber = (int)scrin;
    }
    public void InstantiateKeywordCards()
    {
        Topic currentTopic = GameManager.instance.GetCurrentTopic();
        if (GameManager.instance.currentScreenNumber == 2)
        {
            foreach(PolicyKeywords keyword in currentTopic.formulatedPolicy.keywords)
            {
                GameObject newKeyword = Instantiate(discussionKeywordCard, discussionKeywordContainer);
                TextMeshProUGUI text = newKeyword.GetComponentInChildren<TextMeshProUGUI>();
                Image image = newKeyword.GetComponent<Image>();

                (image.color, text.text) = DetermineKeywordCardColorAndName((int)keyword);
            }
        }
        else if (GameManager.instance.currentScreenNumber == 5)
        {
            foreach(PolicyKeywords keyword in currentTopic.formulatedPolicy.keywords)
            {
                GameObject newKeyword = Instantiate(votingKeywordCard, votingPosKeywordContainer);
                TextMeshProUGUI text = newKeyword.GetComponentInChildren<TextMeshProUGUI>();
                Image image = newKeyword.GetComponent<Image>();

                (image.color, text.text) = DetermineKeywordCardColorAndName((int)keyword);
            }
            foreach (PolicyKeywords keyword in currentTopic.enjinPolicy.keywords)
            {
                GameObject newKeyword = Instantiate(votingKeywordCard, votingNegKeywordContainer);
                TextMeshProUGUI text = newKeyword.GetComponentInChildren<TextMeshProUGUI>();
                Image image = newKeyword.GetComponent<Image>();

                (image.color, text.text) = DetermineKeywordCardColorAndName((int)keyword);
            }
        }
    }
    public void InstantiatePlayerVoteIcons()
    {
        if (NetworkManager.instance == null || NetworkManager.instance.allPlayers.Count == 0) return;

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
                        allPlayerIcons.Add(playerIconPrefab);
                    }
                    else
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, agreeGroup2);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                        allPlayerIcons.Add(playerIconPrefab);
                    }
                    break;
                case VoteTypes.MostlyAgree:
                    if ((mostlyAgreeGroup1.childCount + mostlyAgreeGroup2.childCount) % 2 == 0)
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, mostlyAgreeGroup1);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                        allPlayerIcons.Add(playerIconPrefab);
                    }
                    else
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, mostlyAgreeGroup2);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                        allPlayerIcons.Add(playerIconPrefab);
                    }
                    break;
                case VoteTypes.Neutral:
                    if ((neutralGroup1.childCount + neutralGroup2.childCount) % 2 == 0)
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, neutralGroup1);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                        allPlayerIcons.Add(playerIconPrefab);
                    }
                    else
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, neutralGroup2);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                        allPlayerIcons.Add(playerIconPrefab);
                    }
                    break;
                case VoteTypes.MostlyDisagree:
                    if ((mostlyDisagreeGroup1.childCount + mostlyAgreeGroup2.childCount) % 2 == 0)
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, mostlyDisagreeGroup1);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                        allPlayerIcons.Add(playerIconPrefab);
                    }
                    else
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, mostlyDisagreeGroup2);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                        allPlayerIcons.Add(playerIconPrefab);
                    }
                    break;
                case VoteTypes.Disagree:
                    if ((disagreeGroup1.childCount + disagreeGroup2.childCount) % 2 == 0)
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, disagreeGroup1);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                        allPlayerIcons.Add(playerIconPrefab);
                    }
                    else
                    {
                        GameObject playerIconPrefab = Instantiate(discussionPlayerIcon, disagreeGroup2);
                        Image playerIcon = playerIconPrefab.transform.Find("Icon").GetComponent<Image>();
                        if (playerIcon != null) playerIcon.sprite = playerScript.selectedCharacter.characterImage;
                        allPlayerIcons.Add(playerIconPrefab);
                    }
                    break;
            }
        }
    }
    private (Color32, string) DetermineKeywordCardColorAndName(int value)
    {
        switch(value)
        {
            case 1:
                return (innovation, "Innovation");
            case 2:
                return (riskManagement, "Risk Management");
            case 3:
                return (workerSatisfaction, "Worker Satisfaction");
            case 4:
                return (profit, "Profit");
        }
        return (Color.white, "Error"); //no match
    }
        
    private IEnumerator Discussion()
    {
        iconCircle.SetActive(true);
        yield return null;
        int playerAmount = NetworkManager.instance.allPlayers.Count;
        Debug.Log(playerAmount);
        iconCircle.transform.position = allPlayerIcons[0].transform.position;
        for (int i = 0; i < playerAmount; i++)
        {
            GameObject speaker = allPlayerIcons[i];
            StartCoroutine(MoveIcon(iconCircle.transform, speaker.transform.position, 0.35f));
            TimerScript.instance.StartTimer(GameManager.instance.discussionTime);
            yield return new WaitForSeconds(GameManager.instance.discussionTime);
        }
        TimerDone();
        iconCircle.SetActive(false);
    }

    private IEnumerator MoveIcon(Transform obj, Vector3 targetPos, float duration)
    {
        Vector3 startPos = obj.position;
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            t = t * t * (3f - 2f * t);
            obj.position = Vector3.Lerp(startPos, targetPos, t);
            time += Time.deltaTime;
            yield return null;
        }

        obj.position = targetPos;
    }

    public void TimerDone()
    {
        timer.SetActive(false);
        continueButton.SetActive(true);
    }
}
