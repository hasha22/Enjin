[System.Serializable]
public class SerializableClasses
{

}
[System.Serializable]
public class Envelope
{
    public string type;
    public string data;
}

[System.Serializable]
public class PlayerJoinPayload
{
    public string playerName;
    public string playerID;
}
public class PlayerVote1Payload
{
    public string playerID;
    public string playerVote;
}
public class PlayerVote2Payload
{
    public string playerID;
    public string playerVote;
}
