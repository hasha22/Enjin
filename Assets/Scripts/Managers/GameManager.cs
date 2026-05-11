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
}
