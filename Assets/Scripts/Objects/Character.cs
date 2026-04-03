using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objects/Character")]
public class Character : ScriptableObject
{
    public string characterName;
    public string characterDescription;
    public Sprite characterImage;
    public List<PolicyKeywords> policyKeywords;
    public List<CharacterKeywords> characterKeywords;
}
