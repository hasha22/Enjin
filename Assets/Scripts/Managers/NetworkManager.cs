using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance {get; private set;}

    [Header("Variables")]
    List<Player> allPlayers = new List<Player>();

    void Awake()
    {
        if (instance == null){instance = this;}
    }

    public void ConnectPlayer()
    {
        
    }

    public void DisconnectPlayer()
    {
        
    }
}
