using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [Header("Variables")]
    public List<Topic> allTopics = new List<Topic>();
    [SerializeField] private Topic currentTopic;
    [SerializeField] private Policy currentPolicy;

    [Header("Main references")]
    public List<GameObject> allScreens = new List<GameObject>();
    public GameObject timer;
    public TextMeshProUGUI timerText;


    [Header("Situation Explanation Screen References")]
    public TextMeshProUGUI scenarioNameText;
    public TextMeshProUGUI scenarioDescriptionText;

    [Header("Policy Voting 1 Screen References")]
    public TextMeshProUGUI policyText1;
    public GameObject keywordCardPrefab;
    public GameObject posKeywordContainer;
    public GameObject negKeywordContainer;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
