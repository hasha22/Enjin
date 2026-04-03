using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Topic")]
public class Topic : ScriptableObject
{
    public string topicName;
    public string topicDescription;
    public List<Policy> policies;
}

[System.Serializable]
public class Policy
{
    public string policyName;
    public string policyDescription;
    public List<Keyword> keywords;
}


