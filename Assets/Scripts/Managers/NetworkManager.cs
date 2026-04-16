using NativeWebSocket;
using System.Text;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance { get; private set; }
    
    [System.Serializable]
    public class WSMessage
    {
        public string type;
        public int value;
        public string label;
    }

    WebSocket websocket;

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

        websocket = new WebSocket("ws://localhost:5040");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connected to server");
            Send("{\"type\":\"host\"}");
        };

        websocket.OnMessage += (bytes) =>
        {
            string msg = Encoding.UTF8.GetString(bytes);
            Debug.Log("Received: " + msg);

            WSMessage data = JsonUtility.FromJson<WSMessage>(msg);

            if (data == null) return;

            if (data.type == "button_pressed")
            {
                Debug.Log("Button event received in Unity");
            }

            if (data.type == "slider_submit")
            {
                Debug.Log("Slider event received in Unity: " + data.value);
            }
        };

        websocket.OnError += (e) => Debug.Log("Error: " + e);
        websocket.OnClose += (e) => Debug.Log("Closed");

        await websocket.Connect();
    }

    public void SendButtonClick()
    {
        Send("{\"type\":\"button_pressed\"}");
    }

    public void Send(string json)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            websocket.SendText(json);
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
        await websocket.Close();
    }
}