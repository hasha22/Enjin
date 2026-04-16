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
    }
    async void Start()
    {
        Application.runInBackground = true;

        websocket = new WebSocket("ws://localhost:5040");

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

    }

    public void DisconnectPlayer()
    {

    }
}
