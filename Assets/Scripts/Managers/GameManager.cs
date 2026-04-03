using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance {get; private set;}

    [Header("Variables")]
    public List<Topic> allTopics = new List<Topic>();
    [SerializeField] private Topic currentTopic;
    [SerializeField] private Policy currentPolicy;

    void Awake()
    {
        if (instance == null){instance = this;}
    }

}
