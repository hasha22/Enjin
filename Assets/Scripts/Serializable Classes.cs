public class SerializableClasses
{

}
// Envelope for receiving data
[System.Serializable]
public class Envelope
{
    public string type;
    public string data;
}

// Payload for sending data (generic)
[System.Serializable]
public class OutgoingMessage<T>
{
    public string type;
    public string room;
    public T data;
}
// Payloads for sending data to server
[System.Serializable]
public class InformServerPayload
{
    public string hostClientId;
}
[System.Serializable]
public class CharacterInfoPayload
{
    public string playerID;
    public string characterName;
    public string characterDescription;
    public string keyword1;
    public string keyword2;
}
[System.Serializable]
public class PlayerScreenCommandPayload
{
    public string screenId;
    public string playerState;
    public int roundNumber;
    public int totalRounds;
    public string voteType;
    public int votingDuration;
    public string currentSpeakerPlayerID;
    public string currentSpeakerName;
}

// Classes for converting incoming json to strings
[System.Serializable]
public class PlayerJoinEnvelope
{
    public string playerName;
    public string playerID;
}
[System.Serializable]
public class PlayerVote1Envelope
{
    public string playerName;
    public string playerID;
    public string playerVote;
    public int roundNumber;
    public string voteType;
    public string submitReason;
}
[System.Serializable]
public class PlayerVote2Envelope
{
    public string playerName;
    public string playerID;
    public string playerVote;
    public int roundNumber;
    public string voteType;
    public string submitReason;
}
[System.Serializable]
public class PlayerScreenCommandSuccessEnvelope
{
    public string screenId;
    public string playerState;
    public int roundNumber;
    public int totalRounds;
    public string voteType;
    public int votingDuration;
    public string currentSpeakerPlayerID;
    public string currentSpeakerName;
    public int playerCount;
}
[System.Serializable]
public class FailureEnvelope
{
    public string reason;
}
