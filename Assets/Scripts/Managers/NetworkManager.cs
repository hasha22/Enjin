using NativeWebSocket;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance { get; private set; }

    [Header("Variables")]
    WebSocket websocket;
    public string roomCode;
    List<Player> allPlayers = new List<Player>();

    void Awake()
    {
        if (instance == null) { instance = this; }
    }
    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }
    async void Start()
    {
        Application.runInBackground = true;

        websocket = new WebSocket("ws://localhost:5085");

        websocket.OnOpen += () =>
        {
            SendWebSocketMessage("{\"type\":\"host\"}");
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
    public void ConnectPlayer()
    {

    }

    public void DisconnectPlayer()
    {

    }
}
