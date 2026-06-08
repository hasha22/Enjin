using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [Header("Variables")]
    public List<Topic> allTopics = new List<Topic>();
    public Topic currentTopic;
    public GameScreens currentScreen;
    public int currentScreenNumber;
    public int currentRound;

    [Header("Settings")]
    public int votingTime;
    public int discussionTime;
    public int totalRounds;
    public float typingSpeed;

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
        DetermineTopic();
    }
    public void DetermineTopic()
    {
        switch (currentRound)
        {
            case 1:
                currentTopic = allTopics[0];
                break;
            case 2:
                currentTopic = allTopics[1];
                break;
            case 3:
                currentTopic = allTopics[2];
                break;
            case 4:
                currentTopic = allTopics[3];
                break;
            case 5:
                currentTopic = allTopics[4];
                break;
            case 6:
                currentTopic = allTopics[5];
                break;
        }
    }

    public void DeterminePolicyOutcome()
    {
        List<Player> votedYes = new List<Player>();
        List<Player> votedNo = new List<Player>();
        foreach (GameObject player in NetworkManager.instance.allPlayers)
        {
            Player playerScript = player.GetComponent<Player>();
            if (!playerScript.HasSecondVote())
            {
                continue;
            }

            if (playerScript.GetSecondVote()) { votedYes.Add(playerScript); }
            else { votedNo.Add(playerScript); }
        }
        if (votedYes.Count > votedNo.Count)
        {
            int enj = currentTopic.formulatedPolicy.enjinValue;
            int morale = currentTopic.formulatedPolicy.moraleValue;
            int ethic = currentTopic.formulatedPolicy.ethicValue;
            int profit = currentTopic.formulatedPolicy.profitValue;
            LogPolicyOutcome("formulatedPolicy", enj, morale, ethic, profit);
            ValueManager.instance.ChangeValue(enj, morale, ethic, profit);
        }
        else
        {
            int enj = currentTopic.enjinPolicy.enjinValue;
            int morale = currentTopic.enjinPolicy.moraleValue;
            int ethic = currentTopic.enjinPolicy.ethicValue;
            int profit = currentTopic.enjinPolicy.profitValue;
            LogPolicyOutcome("enjinPolicy", enj, morale, ethic, profit);
            ValueManager.instance.ChangeValue(enj, morale, ethic, profit);
        }


    }
    private void LogPolicyOutcome(string policyName, int enj, int morale, int ethic, int profit)
    {
        Debug.Log($"Applying {policyName}: Enjin {enj}, Morale {morale}, Ethics {ethic}, Profit {profit}");

        if (enj == 0 && morale == 0 && ethic == 0 && profit == 0)
        {
            Debug.LogWarning("Policy outcome values are all 0, so the bar will not visually move.");
        }
    }


    public void ContinueButtonPressed()
    {
        Debug.Log("Continue button pressed. Current screen: " + currentScreen);

        switch (currentScreen)
        {
            case GameScreens.CharacterIntroScreen:
                ContinueToScenario();
                break;
            case GameScreens.SituationExplanationScreen:
                StartVotingPhase();
                break;
            case GameScreens.EnjinUpdateScreen:
                StartVotingPhase();
                break;
            case GameScreens.SecondPolicyVotingScreen:
                DeterminePolicyOutcome();
                ContinueToNextUnityScreen();
                break;
            case GameScreens.FirstPolicyVotingScreen:
            case GameScreens.DiscussionScreen:
            case GameScreens.ResultsScreen:
                ContinueToNextUnityScreen();
                break;

            default:
                Debug.LogWarning("No Continue action assigned for screen: " + currentScreen);
                break;
        }
    }

    public void ContinueToNextUnityScreen()
    {
        if (GameUIManager.instance != null)
        {
            GameUIManager.instance.NextScreen();
        }
        else
        {
            Debug.LogWarning("GameUIManager instance is missing.");
        }
    }

    public void StartVotingPhase()
    {
        if (NetworkManager.instance != null)
        {
            NetworkManager.instance.SendStartVotingRequest();
        }
        else
        {
            Debug.LogWarning("NetworkManager instance is missing.");
        }
    }

    public void ContinueToScenario()
    {
        if (NetworkManager.instance != null)
        {
            NetworkManager.instance.SendShowScenarioRequest();
        }
        else
        {
            Debug.LogWarning("NetworkManager instance is missing.");
        }
    }
    public Topic GetCurrentTopic()
    {
        return currentTopic;
    }
}

