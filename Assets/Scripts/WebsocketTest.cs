using UnityEngine;
using WebSocketSharp;
using UnityEngine.InputSystem;
using TMPro;

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
    [Header("Connection Settings")]
    [SerializeField] private string ipAddress = "192.168.1.223";
    [SerializeField] private string port = "8887";
    [SerializeField] private float reconnectDelay = 5f;
    [SerializeField] private int maxReconnectAttempts = 3;
    private int reconnectAttempts = 0;
    private bool isReconnecting = false;

    [Header("Controller Enable/Disable")]
    [SerializeField] private bool enableKeyboardInput = true;
    [SerializeField] private bool enableMozaInput = false;
    [SerializeField] private bool enableG27Input = true;

    [Header("Input Settings")]
    [SerializeField] private float stepSize = 0.1f;
    [SerializeField] private float inputDeadzone = 0.01f;
    [SerializeField] private InputActionAsset inputActions;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI connectionStatusText;
    
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

    // G27 Input Actions
    private InputActionMap g27Map;
    private InputAction g27Throttle;
    private InputAction g27Steer;

    void Start()
    {
        InitializeInputs();
        InitializeWebSocket();
    }

    private void InitializeInputs()
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
        
        // Initialize G27 inputs
        g27Map = inputActions.FindActionMap("G27", true);
        if (g27Map == null)
        {
            Debug.LogError("Could not find 'G27' action map!");
            return;
        }

        g27Throttle = g27Map.FindAction("G27Throttle");
        g27Steer = g27Map.FindAction("G27Steer");
        
        mozaMap.Enable();
        g27Map.Enable();
    }

    private void InitializeWebSocket()
    {
        string wsUrl = $"ws://{ipAddress}:{port}/wsDrive";
        ws = new WebSocket(wsUrl);
        
        UpdateConnectionStatus(false); // Initial state
        
        ws.OnOpen += (sender, e) => {
            Debug.Log("WebSocket connected!");
            isReconnecting = false;
            reconnectAttempts = 0;
            ResetControlValues();
            SendControlSignal();
            UpdateConnectionStatus(true);
        };
        
        ws.OnMessage += (sender, e) => Debug.Log("Message received: " + e.Data);
        
        ws.OnError += (sender, e) => {
            Debug.LogWarning($"WebSocket error: {e.Message}");
            if (connectionStatusText != null)
            {
                connectionStatusText.text = "Status: Disconnected";
                connectionStatusText.color = Color.red;
            }
            ws?.Close();
            ws = null;
        };
        
        ws.OnClose += (sender, e) => {
            Debug.Log("WebSocket closed: " + e.Reason);
            if (connectionStatusText != null)
            {
                connectionStatusText.text = "Status: Disconnected";
                connectionStatusText.color = Color.red;
            }
            ws = null;
        };
        
        TryConnect();
    }

    private void TryConnect()
    {
        try
        {
            ws.Connect();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to connect: {e.Message}");
            AttemptReconnect();
        }
    }

    private async void AttemptReconnect()
    {
        if (isReconnecting || reconnectAttempts >= maxReconnectAttempts) return;

        isReconnecting = true;
        reconnectAttempts++;
        
        Debug.Log($"Attempting to reconnect... Attempt {reconnectAttempts}/{maxReconnectAttempts}");
        
        await System.Threading.Tasks.Task.Delay((int)(reconnectDelay * 1000));
        
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }
        
        InitializeWebSocket();
    }

    private void ResetControlValues()
    {
        angle = 0f;
        throttle = 0f;
        mode = "user";
        recording = false;
        
        // Also disable input maps initially and re-enable only the ones we want
        mozaMap?.Disable();
        g27Map?.Disable();
        
        if (enableMozaInput) mozaMap?.Enable();
        if (enableG27Input) g27Map?.Enable();
    }

    void Update()
    {
        // Reset values at the start of each frame
        angle = 0f;
        throttle = 0f;

        if (enableKeyboardInput) HandleKeyboardInput();
        if (enableMozaInput) HandleMozaInput();
        if (enableG27Input) HandleG27Input();
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

    void HandleG27Input()
    {
        if (g27Throttle == null || g27Steer == null) return;

        float throttleRaw = g27Throttle.ReadValue<float>();
        float steerRaw = g27Steer.ReadValue<float>();

        if (Mathf.Abs(throttleRaw) < inputDeadzone) throttleRaw = 0f;
        if (Mathf.Abs(steerRaw) < inputDeadzone) steerRaw = 0f;

        if (Mathf.Abs(throttleRaw) > inputDeadzone || Mathf.Abs(steerRaw) > inputDeadzone)
        {
            throttle = throttleRaw;
            angle = steerRaw;
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
        Debug.Log($"Sending Control Signal - Throttle: {throttle:F2}, Angle: {angle:F2}, Valid Controllers: G27={g27Map != null}, MOZA={mozaMap != null}");
        ws.Send(json);
    }

    void OnDestroy()
    {
        if (ws != null)
        {
            isReconnecting = false;
            ws.Close();
        }
        mozaMap?.Disable();
        g27Map?.Disable();
    }

    public void RetryConnection()
    {
        // Reset connection state
        isReconnecting = false;
        reconnectAttempts = 0;
        
        // Update status to show reconnecting
        if (connectionStatusText != null)
        {
            connectionStatusText.text = "Status: Reconnecting...";
            connectionStatusText.color = Color.yellow;
        }
        
        // Close existing connection if any
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }
        
        Debug.Log("Manually retrying connection...");
        InitializeWebSocket();
    }

    private void UpdateConnectionStatus(bool isConnected)
    {
        if (connectionStatusText != null)
        {
            connectionStatusText.text = $"Status: {(isConnected ? "Connected" : "Disconnected")}";
            connectionStatusText.color = isConnected ? Color.green : Color.red;
        }
    }
}