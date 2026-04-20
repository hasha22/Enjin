using NativeWebSocket;
using System;
using System.Text;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance { get; private set; }

    [Header("Room Settings")]
    [SerializeField] private string serverUrl = "ws://localhost:5040";
    [SerializeField] private string roomCode = "ABCD";
    [SerializeField] private string hostClientId = "unity-host-1";

    [Serializable]
    public class Envelope
    {
        public string type;
        public string roomCode;
        public string clientId;
        public string eventType;
        public string state;
        public string message;
        public string payloadJson;
    }

    [Serializable]
    public class SliderPayload
    {
        public int value;
        public string label;
    }

    [Serializable]
    public class ButtonPayload
    {
        public bool pressed;
    }

    private WebSocket websocket;
    private bool isConnecting;

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
            // Optional: trigger reconnect routine here.
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
            case "host_registered":
                Debug.Log($"Host registered in room {msg.roomCode}");
                // Optional initial state broadcast:
                SendStateUpdate("lobby");
                break;

            case "player_joined":
                Debug.Log($"Player joined room {msg.roomCode}");
                break;

            case "player_input":
                HandlePlayerInput(msg);
                break;

            case "room_info":
                Debug.Log($"Room info update for {msg.roomCode}");
                break;

            case "error":
                Debug.LogWarning("Server error: " + (msg.message ?? "(no message)"));
                break;

            case "host_replaced":
                Debug.LogWarning("This host was replaced by another host connection");
                break;
        }
    }

    private void HandlePlayerInput(Envelope msg)
    {
        if (msg.eventType == "slider_submit")
        {
            var payload = SafeParse<SliderPayload>(msg.payloadJson);
            if (payload != null)
            {
                Debug.Log($"Slider from player: {payload.value} ({payload.label})");
                // STEP 4: one real event handled here
                // Update UI/game object if needed.
            }
            return;
        }

        if (msg.eventType == "button_pressed")
        {
            var payload = SafeParse<ButtonPayload>(msg.payloadJson);
            Debug.Log("Button pressed from player");
            return;
        }

        Debug.Log($"Unknown player eventType: {msg.eventType}");
    }

    public void SendHostRegister()
    {
        // Inspector can override the script default with an empty string; server requires a non-empty roomCode.
        string code = string.IsNullOrWhiteSpace(roomCode) ? "ABCD" : roomCode.Trim().ToUpper();
        string json = "{"
            + "\"type\":\"host_register\","
            + "\"roomCode\":\"" + Escape(code) + "\","
            + "\"clientId\":\"" + Escape(hostClientId) + "\""
            + "}";
        Debug.Log("Sending host_register: " + json);
        Send(json);
    }

    public void SendStateUpdate(string state)
    {
        string json = "{"
            + "\"type\":\"state_update\","
            + "\"state\":\"" + Escape(state) + "\""
            + "}";
        Send(json);
    }

    public void SendHostBroadcastTest(string text)
    {
        string payload = "{"
            + "\"text\":\"" + Escape(text) + "\""
            + "}";
        string json = "{"
            + "\"type\":\"host_broadcast\","
            + "\"eventType\":\"test_message\","
            + "\"payload\":" + payload
            + "}";
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

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }

    // Helper: server sends payload as an object, but JsonUtility can't parse dynamic objects directly.
    // For compatibility, if payloadJson is empty this returns null.
    private T SafeParse<T>(string payloadJson) where T : class
    {
        if (string.IsNullOrWhiteSpace(payloadJson)) return null;

        try
        {
            return JsonUtility.FromJson<T>(payloadJson);
        }
        catch
        {
            return null;
        }
    }

    private string Escape(string s)
    {
        return (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}