using NativeWebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance { get; private set; }

    [Header("WebSocket")]
    private WebSocket websocket;
    private bool isConnecting;

    [Header("Room Settings")]
    [SerializeField] private string serverUrl = "ws://localhost:5040";
    [SerializeField] private string roomCode = "ABCD";
    [SerializeField] private string hostClientId = "unity-host-1";

    [Header("Players")]
    public List<GameObject> allPlayers = new List<GameObject>();
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerContainer;

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

    async void Start()
    {
        Application.runInBackground = true;
        await Connect();
    }
    
    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }
    public async System.Threading.Tasks.Task Connect()
    {
        if (isConnecting) return;
        isConnecting = true;

        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("Connected to server");
            SendHostRegister();
        };

        websocket.OnMessage += (bytes) =>
        {
            string raw = Encoding.UTF8.GetString(bytes);
            Debug.Log("Received: " + raw);
            HandleIncoming(raw);
        };

        websocket.OnError += (e) => Debug.LogError("WebSocket Error: " + e);
        websocket.OnClose += (e) =>
        {
            Debug.LogWarning("WebSocket closed");
        };

        try
        {
            await websocket.Connect();
        }
        catch (Exception ex)
        {
            Debug.LogError("Connect failed: " + ex.Message);
        }
        finally
        {
            isConnecting = false;
        }
    }
    private void HandleIncoming(string raw)
    {
        Envelope msg;
        try
        {
            msg = JsonUtility.FromJson<Envelope>(raw);
        }
        catch
        {
            Debug.LogWarning("Invalid JSON from server");
            return;
        }

        if (msg == null || string.IsNullOrEmpty(msg.type)) return;

        switch (msg.type)
        {
            case "player_joined":
                PlayerJoinPayload data = null;
                try
                {
                    data = JsonUtility.FromJson<PlayerJoinPayload>(msg.data);
                }
                catch
                {
                    Debug.LogWarning("Failed to parse player_joined payload");
                    return;
                }

                if (data == null)
                {
                    Debug.LogWarning("player_joined payload was null");
                    return;
                }

                Debug.Log($"Player joined: {data.playerName}");

                ConnectPlayer(data.playerName);
                break;
            case "error":
                Debug.LogWarning("Server error");
                break;
        }
    }
    public void SendHostRegister()
    {
        string code = string.IsNullOrWhiteSpace(roomCode) ? "ABCD" : roomCode.Trim().ToUpper();
        string json = "{"
            + "\"type\":\"host_register\","
            + "\"room\":\"" + Escape(code) + "\","
            + "\"clientId\":\"" + Escape(hostClientId) + "\""
            + "}";
        Debug.Log("Sending host_register: " + json);
        Send(json);
    }
    public void ConnectPlayer(string playerName)
    {
        if (allPlayers.Count >= 6)
        {
            Debug.Log("Error: Only 6 players allowed.");
            return;
        }

        GameObject newPlayer = Instantiate(playerPrefab, playerContainer);
        allPlayers.Add(newPlayer);
        Player player = newPlayer.GetComponent<Player>();
        player.InitializePlayerData(playerName);

        //Instantiate & Update UI elements
        //Register player in a list of active players
        UIManager.instance.IncreaseDisplayedPlayerCount();
        UIManager.instance.UpdatePlayerCard(allPlayers.Count - 1, player);
    }
    public void Send(string json)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            websocket.SendText(json);
        }
        else
        {
            Debug.LogWarning("WebSocket not open. Message not sent.");
        }
    }
    private async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }
    private string Escape(string s)
    {
        return (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
    public List<GameObject> GetPlayerList()
    {
        return allPlayers;
    }
}