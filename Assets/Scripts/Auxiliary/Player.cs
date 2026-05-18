using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Data")]
    [SerializeField] public string playerId;
    [SerializeField] private string playerName;
    [SerializeField] private VoteTypes firstVote; //made serializable for visualization purposes
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
    /*
    public (VoteTypes, bool) GetPlayerVotes()
    {
        return (firstVote, secondVote);
    }*/
    public VoteTypes GetFirstVote()
    {
        return firstVote;
    }
    public bool GetSecondVote()
    {
        return secondVote;
    }
    public void InitializePlayerData(string playerName)
    {
        selectedCharacter = CharacterDatabase.instance.GetRandomCharacter();
        this.playerName = playerName;
    }

    public void SetFirstVote(VoteTypes vote)
    {
        firstVote = vote;
    }
    public void SetSecondVote(bool vote)
    {
        secondVote = vote;
    }

}
