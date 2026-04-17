using NativeWebSocket;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance { get; private set; }

    [Header("Websockets")]
    private WebSocket websocket;
    [SerializeField] private string roomCode;
    [SerializeField] private List<GameObject> allPlayers = new List<GameObject>();
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerContainer;

    [Header("Testing Names")]
    private string name1 = "Jos";
    private string name2 = "Jeroen";
    private string name3 = "Justin";
    private string name4 = "Rose";
    private string name5 = "Senne";
    private string name6 = "Kim Kitsuragi";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif

        //temporary, made for testing. Connect Player & Disconnect player will be called by the server when it sends a message to unity that a player connected/disconnected
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Connecting player....");
            ConnectPlayer();
        }
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            DisconnectPlayer();
        }
    }
    async void Start()
    {
        //Generate Room code
        Application.runInBackground = true;

        websocket = new WebSocket("ws://localhost:5040");

        websocket.OnOpen += () =>
        {
            SendWebSocketMessage("{\"type\":\"host\"}");
            ConnectPlayer(); // Connects host as player
            Debug.Log("Connected as Host!");
        };
        websocket.OnError += (e) => Debug.Log("Error! " + e);
        websocket.OnClose += (code) => Debug.Log("Connection closed!");

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("Received: " + message);

            if (message.Contains("button_pressed"))
            {
                Debug.Log("Player pressed button!");

                SendWebSocketMessage("{\"type\":\"message\",\"text\":\"Hello from Unity!\"}");
            }

            /*if (msg.type == "slider_submit")
            {
                Debug.Log("Slider submitted: " + msg.value + " " + msg.label);

                // Example usage:
                // update UI, store vote, etc.
            }*/
        };

        await websocket.Connect();
    }


    async void SendWebSocketMessage(string json)
    {
        await websocket.SendText(json);
    }
    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }

    public void SendButtonClick()
    {
        Debug.Log("Sending click to server...");
        SendWebSocketMessage("{\"type\":\"button\"}");
    }
    public void ConnectPlayer()
    {
        if (allPlayers.Count >= 6)
        {
            Debug.Log("Error: Only 6 players allowed.");
            return;
        }

        GameObject newPlayer = Instantiate(playerPrefab, playerContainer);
        allPlayers.Add(newPlayer);
        Player player = newPlayer.GetComponent<Player>();

        //temporary, made for testing
        if (allPlayers.Count == 1) player.InitializePlayerData(name1);
        if (allPlayers.Count == 2) player.InitializePlayerData(name2);
        if (allPlayers.Count == 3) player.InitializePlayerData(name3);
        if (allPlayers.Count == 4) player.InitializePlayerData(name4);
        if (allPlayers.Count == 5) player.InitializePlayerData(name5);
        if (allPlayers.Count == 6) player.InitializePlayerData(name6);

        //Instantiate & Update UI elements
        //Register player in a list of active players
        UIManager.instance.IncreaseDisplayedPlayerCount();
        UIManager.instance.UpdatePlayerCard(allPlayers.Count - 1, player);
    }

    public void DisconnectPlayer()
    {

    }
    public List<GameObject> GetPlayerList()
    {
        return allPlayers;
    }
    public string GetRoomCode()
    {
        return roomCode;
    }
}
