using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterDatabase : MonoBehaviour
{
    public static CharacterDatabase instance { get; private set; }
    [Header("Characters")]
    [SerializeField] private List<Character> allCharacters = new List<Character>();
    private HashSet<Character> assignedCharacters = new HashSet<Character>();

    private void Awake()
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
    public Character GetRandomCharacter()
    {
        Character characterToAssign;

        //Filters out already assigned characters
        List<Character> available = allCharacters.Where(c => !assignedCharacters.Contains(c)).ToList();

        if (available.Count == 0)
        {
            Debug.Log("Error: All characters have been assigned");
            return null;
        }

        int randIndex = Random.Range(0, available.Count);
        Debug.Log(randIndex);
        characterToAssign = available[randIndex];
        assignedCharacters.Add(characterToAssign);

        return characterToAssign;
    }
}
