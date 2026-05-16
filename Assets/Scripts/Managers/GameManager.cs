using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEngine;
using UnityEngine.Rendering.Universal;


public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [Header("Variables")]
    public List<Topic> allTopics = new List<Topic>();
    [SerializeField] private Topic currentTopic;
    [SerializeField] private Policy currentPolicy; 
    [SerializeField] public GameScreens currentScreen;
    [SerializeField] public int currentScreenNumber;
    [SerializeField] public int currentRound;
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
    public void UpdateData()
    {
        switch (currentScreen)
        {
            case GameScreens.SituationExplanationScreen:
                DetermineTopic();
                GameUIManager.instance.topText.text = "Current Situation";
                GameUIManager.instance.titleText.text = currentTopic.topicName;
                GameUIManager.instance.scenarioDescText.text = currentTopic.topicDescription;
                break;
            case GameScreens.FirstPolicyVotingScreen:
                GameUIManager.instance.topText.text = "Proposed Policy";
                GameUIManager.instance.titleText.text = currentTopic.formulatedPolicy.policyDescription;
                break;

        }
    }
    public void DetermineTopic()
    {
        switch (currentRound)
        {
            case 1:
                currentTopic = allTopics[0];
                currentPolicy = currentTopic.formulatedPolicy;
                break;
            case 2:
                currentTopic = allTopics[1];
                currentPolicy = currentTopic.formulatedPolicy;
                break;
            case 3:
                currentTopic = allTopics[2];
                currentPolicy = currentTopic.formulatedPolicy;
                break;
            case 4:
                currentTopic = allTopics[3];
                currentPolicy = currentTopic.formulatedPolicy;
                break;
            case 5:
                currentTopic = allTopics[4];
                currentPolicy = currentTopic.formulatedPolicy;
                break;
            case 6:
                currentTopic = allTopics[5];
                currentPolicy = currentTopic.formulatedPolicy;
                break;
        }
    }

    public void AssignVote(string playerId, VoteTypes vote = new VoteTypes(), bool voteTwo = false)
    {
        if (currentScreen == GameScreens.FirstPolicyVotingScreen)
        {
            foreach(GameObject g in NetworkManager.instance.allPlayers)
            {
                Player thisPlayer = g.GetComponent<Player>();
                if (playerId == thisPlayer.playerId)
                {
                    thisPlayer.SetFirstVote(vote);
                }
            }
        }
        else if (currentScreen == GameScreens.SecondPolicyVotingScreen)
        {
            foreach(GameObject g in NetworkManager.instance.allPlayers)
            {
                Player thisPlayer = g.GetComponent<Player>();
                if (playerId == thisPlayer.playerId)
                {
                    thisPlayer.SetSecondVote(voteTwo);
                    //CALL DISPLAY SECOND VOTE HERE!!!!!
                }
            }
        }
        else
        {
            Debug.Log($"You can't call this in screen {currentScreen}, you need to call it in screen 2 or 5");
        }
    }
    public Topic GetCurrentTopic()
    {
        return currentTopic;
    }
}
