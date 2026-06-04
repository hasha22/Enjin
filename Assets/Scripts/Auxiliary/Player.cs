using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerRoundVote
{
    public int roundNumber;
    public VoteTypes firstVote;
    public bool hasFirstVote;
    public bool secondVote;
    public bool hasSecondVote;
}

public class Player : MonoBehaviour
{
    [Header("Player Data")]
    [SerializeField] private string playerId;
    [SerializeField] private string playerName;
    [SerializeField] private VoteTypes firstVote;
    [SerializeField] private bool secondVote;
    [SerializeField] private bool hasSecondVote;
    [SerializeField] private List<PlayerRoundVote> roundVotes = new List<PlayerRoundVote>();
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
    public bool HasSecondVote()
    {
        return hasSecondVote;
    }
    public List<PlayerRoundVote> GetRoundVotes()
    {
        return roundVotes;
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
    public void SetSecondVote(bool vote)
    {
        secondVote = vote;
        hasSecondVote = true;
    }
    public void SaveFirstVoteForRound(int roundNumber, VoteTypes vote)
    {
        PlayerRoundVote roundVote = GetOrCreateRoundVote(roundNumber);
        roundVote.firstVote = vote;
        roundVote.hasFirstVote = true;
    }
    public void SaveSecondVoteForRound(int roundNumber, bool vote)
    {
        PlayerRoundVote roundVote = GetOrCreateRoundVote(roundNumber);
        roundVote.secondVote = vote;
        roundVote.hasSecondVote = true;
    }
    public void ResetRoundVotes()
    {
        firstVote = VoteTypes.NoVote;
        secondVote = false;
        hasSecondVote = false;
    }
    private PlayerRoundVote GetOrCreateRoundVote(int roundNumber)
    {
        foreach (PlayerRoundVote roundVote in roundVotes)
        {
            if (roundVote.roundNumber == roundNumber)
            {
                return roundVote;
            }
        }

        PlayerRoundVote newRoundVote = new PlayerRoundVote
        {
            roundNumber = roundNumber,
            firstVote = VoteTypes.NoVote,
            hasFirstVote = false,
            secondVote = false,
            hasSecondVote = false
        };

        roundVotes.Add(newRoundVote);
        return newRoundVote;
    }
}
