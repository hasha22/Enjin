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
public class InformServerPayload
{
    public string hostClientId;
}
public class CharacterInfoPayload
{
    public string playerID;
    public string characterName;
    public string characterDescription;
    public string keyword1;
    public string keyword2;
}

// Classes for converting incoming json to strings
[System.Serializable]
public class PlayerJoinEnvelope
{
    public string playerName;
    public string playerID;
}
public class PlayerVote1Envelope
{
    public string playerName;
    public string playerID;
    public string playerVote;
}
public class PlayerVote2Envelope
{
    public string playerName;
    public string playerID;
    public string playerVote;
}