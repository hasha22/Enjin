using System.Collections.Generic;
using UnityEngine;

public class Character : ScriptableObject
{
    public string characterName;
    public string characterDescription;
    public Sprite characterImage;
    public List<Policy> policyKeywords;
    public List<CharacterKeywords> characterKeywords;
}
