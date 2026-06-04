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

    private const string PlayerStateVoting = "voting";
    private const string PlayerStateViewingCharacter = "viewing_character";
    private const string PlayerStateViewingScenario = "viewing_scenario";
    private const string PlayerStateWaitingForSituation = "waiting_for_situation";
    private const string PlayerStateWaitingForDiscussion = "waiting_for_discussion";
    private const string PlayerStateWaitingAfterDiscussion = "waiting_after_discussion";
    private const string PlayerStateWaitingForEnjinUpdate = "waiting_for_enjin_update";
    private const string PlayerStateWaitingForResults = "waiting_for_results";
    private const string PlayerStateDiscussionTurn = "discussion_turn";
    private const string PlayerStateGameOver = "game_over";

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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RegisterSecondPlayerVote("1", "yes");
        }
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
                PlayerJoinEnvelope joinData = null;
                try
                {
                    joinData = JsonUtility.FromJson<PlayerJoinEnvelope>(msg.data);
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
                PlayerVote2Envelope voteData2 = null;
                try
                {
                    voteData2 = JsonUtility.FromJson<PlayerVote2Envelope>(msg.data);
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

            case "start_game_success":
                Debug.Log("Server confirmed: game started");

                if (sceneTransitionManager != null)
                {
                    sceneTransitionManager.LoadNextScene();
                }
                else
                {
                    Debug.LogWarning("SceneTransitionManager is not assigned in NetworkManager.");
                }

                break;

            case "start_game_failed":
                Debug.LogWarning("Start game failed: " + msg.data);
                break;

            case "player_disconnected":
                Debug.Log("Player temporarily disconnected: " + msg.data);
                break;

            case "player_reconnected":
                Debug.Log("Player reconnected: " + msg.data);
                break;

            case "player_removed":
                Debug.Log("Player removed from room: " + msg.data);
                break;

            case "show_scenario_success":
                Debug.Log("Server confirmed: scenario shown");

                if (GameUIManager.instance != null)
                {
                    GameUIManager.instance.NextScreen();
                }
                else
                {
                    Debug.LogWarning("GameUIManager instance is missing.");
                }

                break;

            case "show_scenario_failed":
                Debug.LogWarning("Show scenario failed: " + msg.data);
                break;

            case "start_voting_success":
                Debug.Log("Server confirmed: voting started");

                if (GameUIManager.instance != null)
                {
                    GameUIManager.instance.NextScreen();
                }
                else
                {
                    Debug.LogWarning("GameUIManager instance is missing.");
                }

                break;

            case "start_voting_failed":
                Debug.LogWarning("Start voting failed: " + msg.data);
                break;

            case "player_screen_command_success":
                PlayerScreenCommandSuccessEnvelope screenData = null;
                try
                {
                    screenData = JsonUtility.FromJson<PlayerScreenCommandSuccessEnvelope>(msg.data);
                }
                catch
                {
                    Debug.LogWarning("Failed to parse player_screen_command_success payload");
                    return;
                }

                HandlePlayerScreenCommandSuccess(screenData);
                break;

            case "player_screen_command_failed":
                FailureEnvelope failureData = null;
                try
                {
                    failureData = JsonUtility.FromJson<FailureEnvelope>(msg.data);
                }
                catch
                {
                    Debug.LogWarning("Player screen command failed: " + msg.data);
                    return;
                }

                Debug.LogWarning("Player screen command failed: " + failureData.reason);
                break;
        }
    }
    private void HandlePlayerScreenCommandSuccess(PlayerScreenCommandSuccessEnvelope screenData)
    {
        if (screenData == null)
        {
            Debug.LogWarning("Player screen command success payload was null.");
            return;
        }

        Debug.Log($"Server confirmed player screen: {screenData.screenId}");

        switch (screenData.screenId)
        {
            case "characterScreen":
                if (sceneTransitionManager != null)
                {
                    sceneTransitionManager.LoadNextScene();
                }
                else
                {
                    Debug.LogWarning("SceneTransitionManager is not assigned in NetworkManager.");
                }
                break;

            case "situationScreen":
            case "votingScreen":
                if (GameUIManager.instance != null)
                {
                    GameUIManager.instance.NextScreen();
                }
                else
                {
                    Debug.LogWarning("GameUIManager instance is missing.");
                }
                break;
        }
    }
    public void SendHostRegister()
    {
        SendMessageToServer(
        "host_register",
        new InformServerPayload
        {
            hostClientId = this.hostClientId
        });
    }

    public void StartGameFromButton()
    {
        if (allPlayers.Count == 0)
        {
            Debug.LogWarning("Cannot start game: no players connected.");
            return;
        }

        SendStartGameRequest();
    }

    public void SendStartGameRequest()
    {
        SendPlayerScreenCommand(
            "characterScreen",
            PlayerStateViewingCharacter,
            GameManager.instance != null ? GameManager.instance.currentRound : 0,
            "",
            0
        );
    }

    public void SendShowScenarioRequest()
    {
        SendPlayerScreenCommand(
            "situationScreen",
            PlayerStateViewingScenario,
            GameManager.instance.currentRound,
            "",
            0
        );
    }

    public void SendStartVotingRequest()
    {
        string voteType = GameManager.instance.currentScreen == GameScreens.EnjinUpdateScreen
            ? "second_vote"
            : "first_vote";

        SendPlayerScreenCommand(
            "votingScreen",
            PlayerStateVoting,
            GameManager.instance.currentRound,
            voteType,
            GameManager.instance.votingTime
        );
    }
    public void SendWaitingForSituationScreenCommand()
    {
        SendWaitingScreenCommand(PlayerStateWaitingForSituation);
    }
    public void SendWaitingForDiscussionScreenCommand()
    {
        SendWaitingScreenCommand(PlayerStateWaitingForDiscussion);
    }
    public void SendWaitingAfterDiscussionScreenCommand()
    {
        SendWaitingScreenCommand(PlayerStateWaitingAfterDiscussion);
    }
    public void SendWaitingForEnjinUpdateScreenCommand()
    {
        SendWaitingScreenCommand(PlayerStateWaitingForEnjinUpdate);
    }
    public void SendWaitingForResultsScreenCommand()
    {
        SendWaitingScreenCommand(PlayerStateWaitingForResults);
    }
    private void SendWaitingScreenCommand(string playerState)
    {
        SendPlayerScreenCommand(
            "waitingScreen",
            playerState,
            GameManager.instance != null ? GameManager.instance.currentRound : 0,
            "",
            0
        );
    }
    public void SendDiscussionTurnScreenCommand(string currentSpeakerPlayerID, string currentSpeakerName, int discussionDuration)
    {
        SendPlayerScreenCommand(
            "discussionScreen",
            PlayerStateDiscussionTurn,
            GameManager.instance != null ? GameManager.instance.currentRound : 0,
            "",
            discussionDuration,
            currentSpeakerPlayerID,
            currentSpeakerName
        );
    }
    public void SendGameOverScreenCommand()
    {
        SendPlayerScreenCommand(
            "gameOverScreen",
            PlayerStateGameOver,
            GameManager.instance != null ? GameManager.instance.currentRound : 0,
            "",
            0
        );
    }
    private void SendPlayerScreenCommand(string screenId, string playerState, int roundNumber, string voteType, int votingDuration)
    {
        SendPlayerScreenCommand(screenId, playerState, roundNumber, voteType, votingDuration, "", "");
    }
    private void SendPlayerScreenCommand(string screenId, string playerState, int roundNumber, string voteType, int votingDuration, string currentSpeakerPlayerID, string currentSpeakerName)
    {
        SendMessageToServer(
            "player_screen_command",
            new PlayerScreenCommandPayload
            {
                screenId = screenId,
                playerState = playerState,
                roundNumber = roundNumber,
                totalRounds = GameManager.instance != null ? GameManager.instance.totalRounds : 0,
                voteType = voteType,
                votingDuration = votingDuration,
                currentSpeakerPlayerID = currentSpeakerPlayerID,
                currentSpeakerName = currentSpeakerName
            });
    }
    public void SendCharacterInfo(string playerID, string characterName, string characterDescription, string keyword1, string keyword2)
    {
        SendMessageToServer(
        "character_info",
        new CharacterInfoPayload
        {
            playerID = playerID,
            characterName = characterName,
            characterDescription = characterDescription,
            keyword1 = keyword1,
            keyword2 = keyword2
        });
    }
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

        //Instantiate & Update UI elements
        //Register player in a list of active players
        UIManager.instance.IncreaseDisplayedPlayerCount();
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
        Debug.Log($"TRY REGISTER FIRST VOTE | playerID: {playerID}, vote: {playerVote}");
        foreach (GameObject player in allPlayers)
        {
            Player playerScript = player.GetComponent<Player>();

            Debug.Log($"Checking player: {playerScript.GetPlayerName()} with ID: {playerScript.GetPlayerID()}");

            if (playerScript.GetPlayerID() == playerID)
            {
                VoteTypes parsedVote = VoteTypes.NoVote;

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
                playerScript.SaveFirstVoteForRound(GetCurrentRoundNumber(), parsedVote);

                Debug.Log($"FIRST VOTE SAVED | player: {playerScript.GetPlayerName()}, ID: {playerScript.GetPlayerID()}, vote: {playerScript.GetFirstVote()}");
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
                if (playerVote == "yes")
                {
                    playerScript.SetSecondVote(true);
                    playerScript.SaveSecondVoteForRound(GetCurrentRoundNumber(), true);
                }
                else if (playerVote == "no")
                {
                    playerScript.SetSecondVote(false);
                    playerScript.SaveSecondVoteForRound(GetCurrentRoundNumber(), false);
                }
                if (GameUIManager.instance != null) GameUIManager.instance.InstantiateVotePlayerIcon(playerScript);
                break;
            }
        }
    }
    private int GetCurrentRoundNumber()
    {
        return GameManager.instance != null ? GameManager.instance.currentRound : 0;
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
    public List<GameObject> GetPlayerList()
    {
        return allPlayers;
    }
    public void ResetPlayerRoundVotes()
    {
        foreach (GameObject playerObject in allPlayers)
        {
            Player player = playerObject.GetComponent<Player>();
            player.ResetRoundVotes();
        }
    }
}
