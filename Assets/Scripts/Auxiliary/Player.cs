using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Data")]
    [SerializeField] private string playerId;
    [SerializeField] private string playerName;
    [SerializeField] private VoteTypes firstVote;
    [SerializeField] private bool secondVote;
    public Character selectedCharacter;

    public string GetPlayerName()
    {
        return playerName;
    }
    public string GetPlayerID()
    {
        return playerId;
    }
    public VoteTypes GetFirstVote()
    {
        return firstVote;
    }
    public bool GetSecondVote()
    {
        return secondVote;
    }
    public void InitializePlayerData(string playerName, string playerID)
    {
        selectedCharacter = CharacterDatabase.instance.GetRandomCharacter();
        this.playerName = playerName;
        playerId = playerID;
    }
    public void SetFirstVote(VoteTypes vote)
    {
        firstVote = vote;
    }
}
