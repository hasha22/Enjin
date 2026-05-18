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

        UIManager.instance.SetRoomCode(roomCode);
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
                PlayerJoinPayload joinData = null;
                try
                {
                    joinData = JsonUtility.FromJson<PlayerJoinPayload>(msg.data);
                }
                catch
                {
                    Debug.LogWarning("Failed to parse player_joined payload");
                    return;
                }
                if (joinData == null)
                {
                    Debug.LogWarning("player_joined payload was null");
                    return;
                }
                Debug.Log($"Player joined: {joinData.playerName}");

                ConnectPlayer(joinData.playerName);
                break;
            case "player_vote_1":
                PlayerVote1Payload voteData1 = null;
                try
                {
                    voteData1 = JsonUtility.FromJson<PlayerVote1Payload>(msg.data);
                }
                catch
                {
                    Debug.LogWarning("Failed to parse player_vote_1 payload");
                    return;
                }
                if (voteData1 == null)
                {
                    Debug.LogWarning("player_vote_1 payload was null");
                    return;
                }
                RegisterFirstPlayerVote(voteData1.playerID, voteData1.playerVote);
                break;
            case "player_vote_2":
                PlayerVote2Payload voteData2 = null;
                try
                {
                    voteData2 = JsonUtility.FromJson<PlayerVote2Payload>(msg.data);
                }
                catch
                {
                    Debug.LogWarning("Failed to parse player_vote_2 payload");
                    return;
                }
                if (voteData2 == null)
                {
                    Debug.LogWarning("player_vote_2 payload was null");
                    return;
                }
                RegisterSecondPlayerVote(voteData2.playerID, voteData2.playerVote);
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
        player.SetFirstVote(VoteTypes.Agree);

        //Instantiate & Update UI elements
        //Register player in a list of active players
        UIManager.instance.IncreaseDisplayedPlayerCount();
        UIManager.instance.UpdatePlayerCard(allPlayers.Count - 1, player);
    }
    public void RegisterFirstPlayerVote(string playerID, string playerVote)
    {
        foreach (GameObject player in allPlayers)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript.GetPlayerID() == playerID)
            {
                switch (playerVote)
                {
                    case "disagree":
                        playerScript.SetFirstVote(VoteTypes.Disagree);
                        break;
                    case "mostly_disagree":
                        playerScript.SetFirstVote(VoteTypes.MostlyDisagree);
                        break;
                    case "neutral":
                        playerScript.SetFirstVote(VoteTypes.Neutral);
                        break;
                    case "mostly_agree":
                        playerScript.SetFirstVote(VoteTypes.MostlyAgree);
                        break;
                    case "agree":
                        playerScript.SetFirstVote(VoteTypes.Agree);
                        break;
                }
                break;
            }
        }
    }
    public void RegisterSecondPlayerVote(string playerID, string playerVote)
    {
        foreach (GameObject player in allPlayers)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript.GetPlayerID() == playerID)
            {
                if (playerVote == "yes") playerScript.SetSecondVote(true);
                else if (playerVote == "no") playerScript.SetSecondVote(false);
                if (GameUIManager.instance != null) GameUIManager.instance.InstantiateVotePlayerIcon(playerScript);
                break;
            }
        }
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