public class SerializableClasses
{

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
public class StartVotingPayload
{
    public string hostClientId;
    public string votingRound;
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

// Envelope for receiving data (generic)
[System.Serializable]
public class Envelope
{
    public string type;
    public string data;
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
    public string playerID;
    public string playerVote;
}
[System.Serializable]
public class PlayerVote2Envelope
{
    public string playerID;
    public string playerVote;
}
