using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [Header("Variables")]
    public List<Topic> allTopics = new List<Topic>();
    [SerializeField] private Topic currentTopic;
    [SerializeField] private Policy currentPolicy; //Discuss with daniel if this is necessary, since you can just access it from currenttopic anyway
    [SerializeField] public int currentScreen;
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
            case 1:
                DetermineTopic();
                GameUIManager.instance.titleText.text = currentTopic.topicDescription;
                //change text
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
}
