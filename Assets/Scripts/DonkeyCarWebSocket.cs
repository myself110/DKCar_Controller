using UnityEngine;
using NativeWebSocket;
using System.Threading.Tasks;

public class DonkeyCarWebSocket : MonoBehaviour
{
    [Header("WebSocket Settings")]
    public string donkeyCarIP = "192.168.1.100";
    public int donkeyCarPort = 8887;
    
    [Header("Control Values")]
    [SerializeField] private float throttle = 0f;
    [SerializeField] private float angle = 0f;
    [SerializeField] private float stepSize = 0.1f;
    private string mode = "user";
    private bool recording = false;

    private WebSocket websocket;
    private bool isConnected;

    async void Start()
    {
        websocket = new WebSocket($"ws://{donkeyCarIP}:{donkeyCarPort}/wsDrive");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            isConnected = true;
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError($"Error! {e}");
            isConnected = false;
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
            isConnected = false;
        };

        await websocket.Connect();
    }

    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
            websocket?.DispatchMessageQueue();
        #endif

        if (!isConnected) return;

        // Handle keyboard input
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            throttle += stepSize;
            SendControlData();
            Debug.Log($"Throttle: {throttle}");
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            throttle -= stepSize;
            SendControlData();
            Debug.Log($"Throttle: {throttle}");
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            angle -= stepSize;
            SendControlData();
            Debug.Log($"Angle: {angle}");
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            angle += stepSize;
            SendControlData();
            Debug.Log($"Angle: {angle}");
        }
    }

    async void SendControlData()
    {
        if (websocket.State != WebSocketState.Open) return;

        var controlData = new
        {
            angle = angle,
            throttle = throttle,
            drive_mode = mode,
            recording = recording
        };

        string jsonData = JsonUtility.ToJson(controlData);
        Debug.Log($"Sending JSON: {jsonData}");
        await websocket.SendText(jsonData);
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }
}

    