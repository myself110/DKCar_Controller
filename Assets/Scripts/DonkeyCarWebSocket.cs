using UnityEngine;
using NativeWebSocket;
using Newtonsoft.Json;
using System.Collections;
using System.Threading.Tasks;

public class DonkeyCarWebSocket : MonoBehaviour
{
    public string donkeyCarIP = "192.168.1.100"; // Replace with your Donkey Car's IP
    public int donkeyCarPort = 9091; // Default WebSocket port
    WebSocket websocket;

    public float steering;
    public float throttle;

    async void Start()
    {
        websocket = new WebSocket($"ws://{donkeyCarIP}:{donkeyCarPort}");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            // Receiving messages (optional for a controller)
            Debug.Log("OnMessage! " + System.Text.Encoding.UTF8.GetString(bytes));
        };

        // Connect and await the connection
        await websocket.Connect();
    }

    async Task SendControlData()
    {
        // Create a JSON object
        var controlData = new {
            msg_type = "control", // Or whatever msg_type the Donkeycar expects
            steering = steering,
            throttle = throttle
        };
        string jsonData = JsonConvert.SerializeObject(controlData);

        // Send the message
        if (websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(jsonData);
        }
    }

    async void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
            if (websocket != null)
            {
                websocket.DispatchMessageQueue();
            }
        #endif

        // Get input from your controller (already mapped to -1.0 to 1.0)
        steering = Input.GetAxis("Horizontal"); // Example: A/D keys
        throttle = Input.GetAxis("Vertical");   // Example: W/S keys

        await SendControlData();
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }
}

    