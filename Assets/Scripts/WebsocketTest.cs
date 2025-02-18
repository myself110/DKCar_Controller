using UnityEngine;
using WebSocketSharp;
using UnityEngine.InputSystem;

[System.Serializable]
public class DonkeyCarData
{
    public float angle;
    public float throttle;
    public string drive_mode;
    public bool recording;
}

public class WebsocketTest : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private float stepSize = 0.1f;
    [SerializeField] private float inputDeadzone = 0.01f;
    [SerializeField] private InputActionAsset inputActions;
    
    private WebSocket ws;
    private float angle = 0f;
    private float throttle = 0f;
    private string mode = "user";
    private bool recording = false;

    // MOZA Input Actions
    private InputActionMap mozaMap;
    private InputAction mozaRX;
    private InputAction mozaRY;
    private InputAction mozaSteerLeft;
    private InputAction mozaSteerRight;

    void Start()
    {
        InitializeMozaInputs();
        InitializeWebSocket();
    }

    private void InitializeMozaInputs()
    {
        if (inputActions == null)
        {
            Debug.LogError("Input Actions asset is not assigned!");
            return;
        }

        mozaMap = inputActions.FindActionMap("Moza", true);
        if (mozaMap == null)
        {
            Debug.LogError("Could not find 'Moza' action map!");
            return;
        }

        mozaRX = mozaMap.FindAction("MozaRX");
        mozaRY = mozaMap.FindAction("MozaRY");
        mozaSteerLeft = mozaMap.FindAction("MozaSteerLeft");
        mozaSteerRight = mozaMap.FindAction("MozaSteerRight");
        
        mozaMap.Enable();
    }

    private void InitializeWebSocket()
    {
        ws = new WebSocket("ws://192.168.50.142:8887/wsDrive");
        ws.OnOpen += (sender, e) => Debug.Log("WebSocket connected!");
        ws.OnMessage += (sender, e) => Debug.Log("Message received: " + e.Data);
        ws.OnError += (sender, e) => Debug.LogError("WebSocket error: " + e.Message);
        ws.OnClose += (sender, e) => Debug.Log("WebSocket closed: " + e.Reason);
        ws.Connect();
    }

    void Update()
    {
        HandleKeyboardInput();
        HandleMozaInput();
    }

    void HandleMozaInput()
    {
        if (mozaRX == null || mozaRY == null || mozaSteerLeft == null || mozaSteerRight == null) return;

        // Handle throttle
        float rxRaw = mozaRX.ReadValue<float>();
        float ryRaw = mozaRY.ReadValue<float>();
        
        float rxMapped = Mathf.InverseLerp(-1f, 1f, rxRaw);
        float ryMapped = Mathf.InverseLerp(1f, -1f, ryRaw) * -1f;

        if (Mathf.Abs(rxMapped) < inputDeadzone) rxMapped = 0f;
        if (Mathf.Abs(ryMapped) < inputDeadzone) ryMapped = 0f;

        throttle = Mathf.Abs(rxMapped) > Mathf.Abs(ryMapped) ? rxMapped : ryMapped;

        // Handle steering
        float leftStickRaw = mozaSteerLeft.ReadValue<float>();
        float rightStickRaw = mozaSteerRight.ReadValue<float>();

        if (leftStickRaw > 0f + inputDeadzone)
        {
            angle = (1f - leftStickRaw) * -1f;
        }
        else if (rightStickRaw < 0f - inputDeadzone)
        {
            angle = Mathf.InverseLerp(-1f, 0f, rightStickRaw);
        }
        else
        {
            angle = 0f;
        }

        if (Mathf.Abs(throttle) > inputDeadzone || Mathf.Abs(angle) > inputDeadzone)
        {
            SendControlSignal();
        }
    }

    void HandleKeyboardInput()
    {
        bool inputChanged = false;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            throttle = Mathf.Clamp(throttle + stepSize, -1f, 1f);
            inputChanged = true;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            throttle = Mathf.Clamp(throttle - stepSize, -1f, 1f);
            inputChanged = true;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            angle = Mathf.Clamp(angle - stepSize, -1f, 1f);
            inputChanged = true;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            angle = Mathf.Clamp(angle + stepSize, -1f, 1f);
            inputChanged = true;
        }

        if (inputChanged)
        {
            SendControlSignal();
        }
    }

    void SendControlSignal()
    {
        var data = new DonkeyCarData
        {
            angle = angle,
            throttle = throttle,
            drive_mode = mode,
            recording = recording
        };

        string json = JsonUtility.ToJson(data);
        Debug.Log($"Sending JSON - Throttle: {throttle:F2}, Angle: {angle:F2}");
        Debug.Log($"JSON: {json}");
        ws.Send(json);
    }

    void OnDestroy()
    {
        mozaMap?.Disable();
        ws?.Close();
    }
}