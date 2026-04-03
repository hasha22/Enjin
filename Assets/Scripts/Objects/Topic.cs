using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objects/Topic")]
public class Topic : ScriptableObject
{
    public string topicName;
    public string topicDescription;
    public List<Policy> policies;

    public Policy GetRandomPolicy()
    {
        if (policies.Count > 0)
        {
            return policies[Random.Range(0, policies.Count)];
        }
        return null;
    }
}

[System.Serializable]
public class Policy
{
    public string policyName;
    public string policyDescription;
    public List<PolicyKeywords> keywords;
}


