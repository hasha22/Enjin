using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Data")]
    [SerializeField] private string playerName;
    [SerializeField] private VoteTypes firstVote; //made serializable for visualization purposes
    [SerializeField] private bool secondVote;
    public Character selectedCharacter;

    public string GetPlayerName()
    {
        return playerName;
    }
    public (VoteTypes, bool) GetPlayerVotes()
    {
        return (firstVote, secondVote);
    }
    public void InitializePlayerData(string playerName)
    {
        selectedCharacter = CharacterDatabase.instance.GetRandomCharacter();
        this.playerName = playerName;
    }
}
