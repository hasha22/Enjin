using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objects/Ending")]
public class Ending : ScriptableObject
{
    public string endingName;
    [TextArea(3, 10)]
    public List<string> endingText = new List<string>();
}
