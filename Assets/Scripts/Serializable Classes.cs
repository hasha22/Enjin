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
