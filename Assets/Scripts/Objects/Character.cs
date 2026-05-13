using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objects/Character")]
public class Character : ScriptableObject
{
    public string characterName;
    [TextArea(3, 10)]
    public string characterDescription;
    public Sprite characterImage;
    public List<CharacterKeywords> characterKeywords;
}
