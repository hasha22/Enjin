using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [Header("Variables")]
    public List<Topic> allTopics = new List<Topic>();
    [SerializeField] public Topic currentTopic;
    [SerializeField] private Policy currentPolicy; 
    [SerializeField] public GameScreens currentScreen;
    [SerializeField] public int currentScreenNumber;
    [SerializeField] public int currentRound;
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
    /*
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
    */

    public void DeterminePolicyOutcome()
    {
        List<Player> votedYes = new List<Player>();
        List<Player> votedNo = new List<Player>();
        foreach (GameObject player in NetworkManager.instance.allPlayers)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript.secondVote){votedYes.Add(playerScript);}
            else {votedNo.Add(playerScript);}
        }
        if (votedYes.Count > votedNo.Count)
        {
            int enj = currentTopic.formulatedPolicy.enjinValue;
            int morale = currentTopic.formulatedPolicy.moraleValue;
            int ethic = currentTopic.formulatedPolicy.ethicValue;
            int profit = currentTopic.formulatedPolicy.profitValue;
            ValueManager.instance.ChangeValue(enj, morale, ethic, profit);
        }
        else
        {
            int enj = currentTopic.enjinPolicy.enjinValue;
            int morale = currentTopic.enjinPolicy.moraleValue;
            int ethic = currentTopic.enjinPolicy.ethicValue;
            int profit = currentTopic.enjinPolicy.profitValue;
            ValueManager.instance.ChangeValue(enj, morale, ethic, profit);
        }
    }
    

    public Topic GetCurrentTopic()
    {
        return currentTopic;
    }
}
