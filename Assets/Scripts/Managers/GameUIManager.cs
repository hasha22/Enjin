using System.Collections.Generic;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;

    [Header("Screen References")]
    public List<GameObject> allScreens = new List<GameObject>();
    private List<GameObject> trashCan = new List<GameObject>();

    [Header("Text References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI topText;
    public TextMeshProUGUI scenarioDescText;
    [SerializeField] private TextMeshProUGUI enjinDescText;
    [SerializeField] private TextMeshProUGUI roundIndicator;
    [SerializeField] private TextMeshProUGUI voting1Title;
    [SerializeField] public TextMeshProUGUI enjinVersionText;
    [SerializeField] public TextMeshProUGUI enjinVersionDescriptionText;

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
    private Dictionary<VoteTypes, (Transform group1, Transform group2)> voteGroups;
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
    [SerializeField] private Transform yesGroup1;
    [SerializeField] private Transform yesGroup2;
    [SerializeField] private Transform noGroup1;
    [SerializeField] private Transform noGroup2;

    [Header("List of players")]
    [SerializeField] public List<GameObject> allPlayerIcons = new List<GameObject>();


    private void Awake()
    {
        if (instance == null) { instance = this; }
        voteGroups = new Dictionary<VoteTypes, (Transform, Transform)>
        {
            { VoteTypes.Agree,          (agreeGroup1,         agreeGroup2)         },
            { VoteTypes.MostlyAgree,    (mostlyAgreeGroup1,   mostlyAgreeGroup2)   },
            { VoteTypes.Neutral,        (neutralGroup1,       neutralGroup2)       },
            { VoteTypes.MostlyDisagree, (mostlyDisagreeGroup1, mostlyDisagreeGroup2) },
            { VoteTypes.Disagree,       (disagreeGroup1,      disagreeGroup2)      },
        };
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

        if ((int)currentScreen >= 7)
        {
            GameManager.instance.currentScreen = GameScreens.SituationExplanationScreen;
            GameManager.instance.currentRound++;
            ResetUI();
            currentScreen = GameManager.instance.currentScreen;
            currentRound = GameManager.instance.currentRound;
            UpdateCurrentScreenNumber(currentScreen);
            if (currentRound > totalRounds) { SceneManager.LoadScene("OutcomeScene"); return; }
            GameManager.instance.DetermineTopic();
            roundIndicator.text = $"{currentRound}/{totalRounds}";
        }

        //Turns the correct screen on and the rest off
        for (int i = 0; i < allScreens.Count; i++)
        {
            if (i == (int)currentScreen)
            {
                allScreens[i].SetActive(true);
                UpdateData();
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
            InstantiatePlayerDiscussionIcons();
			            StartCoroutine(Discussion());
        }
        else if(currentScreen == GameScreens.EnjinUpdateScreen)
        {
            iconCircle.SetActive(false);
            timer.SetActive(false);
            continueButton.SetActive(true);
            enjinTitle.SetActive(true);
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
            foreach (PolicyKeywords keyword in currentTopic.formulatedPolicy.keywords)
            {
                InstantiateKeywordInContainer(discussionKeywordContainer, keyword, true);
            }
        }
        else if (GameManager.instance.currentScreenNumber == 5)
        {
            foreach (PolicyKeywords keyword in currentTopic.formulatedPolicy.keywords)
            {
                InstantiateKeywordInContainer(votingPosKeywordContainer, keyword, false);
            }
            foreach (PolicyKeywords keyword in currentTopic.enjinPolicy.keywords)
            {
                InstantiateKeywordInContainer(votingNegKeywordContainer, keyword, false);
            }
        }
    }
    public void InstantiatePlayerDiscussionIcons()
    {
        if (NetworkManager.instance == null || NetworkManager.instance.allPlayers.Count == 0) return;

        foreach (GameObject playerObject in NetworkManager.instance.allPlayers)
        {
            Player playerScript = playerObject.GetComponent<Player>();
            VoteTypes playerVote = playerScript.GetFirstVote();

            if (!voteGroups.TryGetValue(playerVote, out var groups)) continue;

            bool useSecondGroup = (groups.group1.childCount + groups.group2.childCount) % 2 != 0;
            Transform group = useSecondGroup ? groups.group2 : groups.group1;

            InstantiateIconInGroup(group, playerScript.selectedCharacter.characterImage);
        }
    }
    public void InstantiateVotePlayerIcon(Player playerScript)
    {
        if (NetworkManager.instance == null || NetworkManager.instance.allPlayers.Count == 0) return;

        bool playerVote = playerScript.GetSecondVote();

        if (playerVote)
        {
            bool useFirstGroup = (yesGroup1.childCount < 3);
            Transform group = useFirstGroup ? yesGroup1 : yesGroup2;
            InstantiateIconInGroup(group, playerScript.selectedCharacter.characterImage);
        }
        else if (!playerVote)
        {
            bool useFirstGroup = (noGroup1.childCount < 3);
            Transform group = useFirstGroup ? noGroup1 : noGroup2;
            InstantiateIconInGroup(group, playerScript.selectedCharacter.characterImage);        
        }
    }
    private (Color32, string) DetermineKeywordCardColorAndName(int value)
    {
        switch (value)
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
    private void InstantiateIconInGroup(Transform group, Sprite sprite)
    {
        GameObject prefab = Instantiate(discussionPlayerIcon, group);
        Image icon = prefab.transform.Find("Icon").GetComponent<Image>();
        if (icon != null) icon.sprite = sprite;

        trashCan.Add(prefab);
    }
    private void InstantiateKeywordInContainer(Transform container, PolicyKeywords keyword, bool isVoting1 = true)
    {
        GameObject newKeyword = null;

        if (isVoting1) newKeyword = Instantiate(discussionKeywordCard, container);
        else newKeyword = Instantiate(votingKeywordCard, container);

        TextMeshProUGUI text = newKeyword.GetComponentInChildren<TextMeshProUGUI>();
        Image image = newKeyword.GetComponent<Image>();

        (image.color, text.text) = DetermineKeywordCardColorAndName((int)keyword);

        trashCan.Add(newKeyword);
    }
    private void ResetUI()
    {
        foreach (GameObject gameObject in trashCan)
        {
            Destroy(gameObject);
        }
	}
        
    private IEnumerator Discussion()
    {
        iconCircle.SetActive(true);
        yield return null;
        if (NetworkManager.instance == null) {iconCircle.SetActive(false); yield break;}
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

    public void UpdateData()
    {
        string enjinVersion = $"{GameManager.instance.currentRound}.0";
        switch (GameManager.instance.currentScreen)
        {
            case GameScreens.SituationExplanationScreen:
                GameUIManager.instance.topText.text = "Current Situation";
                GameUIManager.instance.titleText.text = GameManager.instance.currentTopic.topicName;
                StartCoroutine(TypeText(GameManager.instance.currentTopic.topicDescription, GameUIManager.instance.scenarioDescText));
                break;
            case GameScreens.FirstPolicyVotingScreen:
                GameUIManager.instance.topText.text = "Proposed Policy";
                GameUIManager.instance.titleText.text = GameManager.instance.currentTopic.formulatedPolicy.policyDescription;
                break;
            case GameScreens.EnjinUpdateScreen:
                GameUIManager.instance.enjinVersionText.text = enjinVersion;
                StartCoroutine(TypeText(GameManager.instance.currentTopic.enjinPolicy.policyDescription, GameUIManager.instance.enjinVersionDescriptionText));
                break;
        }
    }

    private IEnumerator TypeText(string text, TextMeshProUGUI targetText)
    {
        GameAudioManager.instance.typingSource.Play();
        targetText.text = "";
        foreach (char c in text)
        {
            targetText.text += c;
            yield return new WaitForSeconds(GameManager.instance.typingSpeed);
        }
        GameAudioManager.instance.typingSource.Stop();
    }
}
