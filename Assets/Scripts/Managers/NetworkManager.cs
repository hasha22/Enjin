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
    [SerializeField] private string serverUrl = "wss://enjin--enjin--qpbmsj2bcc7n.code.run";
    [SerializeField] private string roomCode = "ABCD";
    [SerializeField] private string hostClientId = "unity-host-1";

    [Header("Players")]
    public List<GameObject> allPlayers = new List<GameObject>();
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerContainer;

    [Header("Scene Transition")]
    [SerializeField] private SceneTransitionManager sceneTransitionManager;

    [Header("Message Requests")]
    private const string START_GAME_REQUEST = "start_game_request";
    private const string HOST_REGISTER = "host_register";
    private const string SHOW_SCENARIO_REQUEST = "show_scenario_request";
    private const string START_VOTING_REQUEST = "start_voting_request";
    private const string FIRST_VOTE = "first_vote";
    private const string SECOND_VOTE = "second_vote";
    private const string CHARACTER_INFO = "character_info";
    private const string START_DISCUSSION_REQUEST = "start_discussion_request";
    private const string SHOW_ENJIN_UPDATE_SCREEN = "show_enjin_update_screen";
    private const string SHOW_OUTCOME_SCREEN = "show_outcome_screen";
    private const string SHOW_WAITING_SITUATION_SCREEN = "show_waiting_situation_screen";
    #region UNITY METHODS

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
        UIManager.instance.SetRoomCode(roomCode);
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
    #endregion
    #region WEBSOCKET CONNECTIVITY
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
    private async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }
    #endregion
    #region RECEIVING SERVER MESSAGES
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

        if (msg == null || string.IsNullOrEmpty(msg.type))
        {
            Debug.LogWarning("Message was null");
            return;
        }
        switch (msg.type)
        {
            case "player_joined":
                PlayerJoinEnvelope joinData = null;
                try
                {
                    joinData = JsonUtility.FromJson<PlayerJoinEnvelope>(msg.data);
                }
                catch
                {
                    Debug.LogWarning("Failed to parse player_joined envelope");
                    return;
                }
                Debug.Log($"Player joined: {joinData.playerName}");

                ConnectPlayer(joinData.playerName, joinData.playerID);
                break;
            case "player_vote_1":
                PlayerVote1Envelope voteData1 = null;
                try
                {
                    voteData1 = JsonUtility.FromJson<PlayerVote1Envelope>(msg.data);
                }
                catch
                {
                    Debug.LogWarning("Failed to parse player_vote_1 envelope");
                    return;
                }
                RegisterFirstPlayerVote(voteData1.playerID, voteData1.playerVote);
                break;
            case "player_vote_2":
                PlayerVote2Envelope voteData2 = null;
                try
                {
                    voteData2 = JsonUtility.FromJson<PlayerVote2Envelope>(msg.data);
                }
                catch
                {
                    Debug.LogWarning("Failed to parse player_vote_2 envelope");
                    return;
                }
                RegisterSecondPlayerVote(voteData2.playerID, voteData2.playerVote);
                break;
            case "start_game_success":
                if (sceneTransitionManager != null)
                {
                    sceneTransitionManager.LoadNextScene();
                    Debug.Log("Server confirmed. Starting game.");
                }
                else
                {
                    Debug.LogWarning("SceneTransitionManager is not assigned in NetworkManager.");
                }
                break;
            case "player_skip":
                PlayerSkipEnvelope playerSkip = null;
                try
                {
                    playerSkip = JsonUtility.FromJson<PlayerSkipEnvelope>(msg.data);
                }
                catch
                {
                    Debug.LogWarning("Failed to parse player_skip envelope");
                    return;
                }
                GameUIManager.instance.SkipDiscussionTurn(playerSkip.playerID);
                break;
            case "start_game_failed":
                Debug.LogWarning("Start game failed: " + msg.data);
                break;
            case "show_scenario_success":
                Debug.Log("Showing next screen.");
                break;
            case "show_scenario_failed":
                Debug.Log("Show scenario failed: " + msg.data);
                break;
            case "start_voting_success":
                Debug.Log("Server confirmed: voting started");
                break;
            case "start_voting_failed":
                Debug.LogWarning("Start voting failed: " + msg.data);
                break;
            case "player_disconnected":
                Debug.LogWarning("Player temporarily disconnected: " + msg.data);
                break;
            case "player_reconnected":
                Debug.Log("Player reconnected: " + msg.data);
                break;
            case "player_removed":
                Debug.Log("Player removed from room: " + msg.data);
                break;
        }
    }
    #endregion
    #region SEND SERVER REQUESTS
    public void SendHostRegister()
    {
        SendMessageToServer(
            HOST_REGISTER,
            new InformServerPayload
            {
                hostClientId = this.hostClientId
            }
        );
    }
    public void SendStartGameRequest()
    {
        SendMessageToServer(
            START_GAME_REQUEST,
            new InformServerPayload
            {
                hostClientId = this.hostClientId
            }
        );
    }
    public void SendShowScenarioRequest()
    {
        SendMessageToServer(
            SHOW_SCENARIO_REQUEST,
            new InformServerPayload
            {
                hostClientId = this.hostClientId
            }
        );
    }
    public void SendStartVotingRequest()
    {
        string voteType = GameManager.instance.currentScreen == GameScreens.FirstPolicyVotingScreen ? FIRST_VOTE : SECOND_VOTE;
        SendMessageToServer(
            START_VOTING_REQUEST,
            new StartVotingPayload
            {
                hostClientId = this.hostClientId,
                votingRound = voteType
            }
        );
    }
    public void SendCharacterInfo(string playerID, string characterName, string characterDescription, string keyword1, string keyword2)
    {
        SendMessageToServer(
            CHARACTER_INFO,
            new CharacterInfoPayload
            {
                playerID = playerID,
                characterName = characterName,
                characterDescription = characterDescription,
                keyword1 = keyword1,
                keyword2 = keyword2
            }
        );
    }
    public void SendStartDiscussionRequest()
    {
        SendMessageToServer(
            START_DISCUSSION_REQUEST,
            new InformServerPayload
            {
                hostClientId = this.hostClientId
            }
        );
    }
    public void SendShowEnjinUpdateScreen()
    {
        SendMessageToServer(
            SHOW_ENJIN_UPDATE_SCREEN,
            new InformServerPayload
            {
                hostClientId = this.hostClientId
            }
        );
    }
    public void SendShowOutcomeScreen()
    {
        SendMessageToServer(
            SHOW_OUTCOME_SCREEN,
            new InformServerPayload
            {
                hostClientId = this.hostClientId
            }
        );
    }
    public void SendShowWaitingSituationScreen()
    {
        SendMessageToServer(
            SHOW_WAITING_SITUATION_SCREEN,
            new InformServerPayload
            {
                hostClientId = this.hostClientId
            }
        );
    }
    private void SendMessageToServer<T>(string type, T payload)
    {
        OutgoingMessage<T> message =
            new OutgoingMessage<T>()
            {
                type = type,
                room = roomCode.Trim().ToUpper(),
                data = payload
            };

        string json = JsonUtility.ToJson(message);

        Debug.Log($"Sending {type}: {json}");

        Send(json);
    }
    private void Send(string json)
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
    #endregion

    #region GAME LOGIC
    public void ConnectPlayer(string playerName, string playerID)
    {
        if (allPlayers.Count >= 6)
        {
            Debug.Log("Error: Only 6 players allowed.");
            return;
        }

        GameObject newPlayer = Instantiate(playerPrefab, playerContainer);
        allPlayers.Add(newPlayer);
        Player player = newPlayer.GetComponent<Player>();
        player.InitializePlayerData(playerName, playerID);

        UIManager.instance.UpdatePlayerCard(allPlayers.Count - 1, player);

        SendCharacterInfo(player.GetPlayerID(),
            player.selectedCharacter.characterName,
            player.selectedCharacter.characterDescription,
            player.selectedCharacter.characterKeywords[0].ToString(),
            player.selectedCharacter.characterKeywords[1].ToString()
        );
    }
    public void RegisterFirstPlayerVote(string playerID, string playerVote)
    {
        foreach (GameObject player in allPlayers)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript.GetPlayerID() == playerID)
            {
                VoteTypes parsedVote = VoteTypes.Neutral;

                switch (playerVote)
                {
                    case "disagree":
                        parsedVote = VoteTypes.Disagree;
                        break;
                    case "mostly_disagree":
                        parsedVote = VoteTypes.MostlyDisagree;
                        break;
                    case "neutral":
                        parsedVote = VoteTypes.Neutral;
                        break;
                    case "mostly_agree":
                        parsedVote = VoteTypes.MostlyAgree;
                        break;
                    case "agree":
                        parsedVote = VoteTypes.Agree;
                        break;
                }
                playerScript.SetFirstVote(parsedVote);

                Debug.Log($"FIRST VOTE SAVED | player: {playerScript.GetPlayerName()}, ID: {playerScript.GetPlayerID()}, vote: {playerScript.GetFirstVote()}");
                break;
            }
        }
        if (FirstCheckIfAllVoted())
        {
            TimerScript.instance.StopTimer();
        }
    }

    public bool FirstCheckIfAllVoted()
    {
        foreach (GameObject p in allPlayers)
        {
            Player player = p.GetComponent<Player>();
            if (player.GetFirstVote() == 0 || player.GetFirstVote() == VoteTypes.NoVote)
            {
                return false;
            }
        }
        return true;
    }

    public bool SecondCheckIfAllVoted()
    {
        foreach (GameObject p in allPlayers)
        {
            Player player = p.GetComponent<Player>();
            if (player.GetSecondVote() == FinalVoteTypes.NoVote || player.GetSecondVote() == 0)
            {
                return false;
            }
        }
        return true;
    }

    public void RegisterSecondPlayerVote(string playerID, string playerVote)
    {
        foreach (GameObject player in allPlayers)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript.GetPlayerID() == playerID)
            {
                if (playerVote == "yes")
                {
                    playerScript.SetSecondVote(FinalVoteTypes.Yes);
                }
                else if (playerVote == "no")
                {
                    playerScript.SetSecondVote(FinalVoteTypes.No);
                }
                GameUIManager.instance.InstantiateVotePlayerIcon(playerScript);
                break;
            }
        }
        if (SecondCheckIfAllVoted())
        {
            TimerScript.instance.StopTimer();
        }
    }
    #endregion
    #region HELPERS
    public void StartGameFromButton()
    {
        if (allPlayers.Count == 0)
        {
            Debug.LogWarning("Cannot start game: no players connected.");
            return;
        }

        SendStartGameRequest();
    }
    public List<GameObject> GetPlayerList()
    {
        return allPlayers;
    }
    #endregion
}
