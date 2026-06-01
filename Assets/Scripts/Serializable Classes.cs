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
    public string playerName;
    public string playerID;
    public string playerVote;
}
public class PlayerVote2Payload
{
    public string playerName;
    public string playerID;
    public string playerVote;
}
public class PlayerCharacterPayload
{
    public string characterName;
    public string characterDescription;
    public string keyword1;
    public string keyword2;
}