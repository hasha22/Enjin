using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objects/Topic")]
public class Topic : ScriptableObject
{
    [TextArea(2, 10)]
    public string topicName;
    [TextArea(3, 10)]
    public string topicDescription;
    public Policy formulatedPolicy;
    public Policy enjinPolicy;
}

[System.Serializable]
public class Policy
{
    public string policyName;
    [TextArea(3, 10)]
    public string policyDescription;
    public List<PolicyKeywords> keywords;

    [Header("Value changes")]
    public int enjinValue;
    public int moraleValue;
    public int ethicValue;
    public int profitValue;
}


