using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class LogiG27Translate : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject throttleCube;
    [SerializeField] private GameObject steerCube;
    [SerializeField] private TextMeshPro throttleValueText;
    [SerializeField] private TextMeshPro steerValueText;
    [SerializeField] private TextMeshPro controllerTypeText;

    [Header("Settings")]
    [SerializeField] private float maxThrottleDistance = 4f;
    [SerializeField] private float maxSteerDistance = 4f;
    [SerializeField] private float inputDeadzone = 0.01f;
    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap g27Map;
    private InputAction g27Throttle;
    private InputAction g27Steer;
    private Vector3 throttleInitialPos;
    private Vector3 steerInitialPos;
    public float throttleValue { get; private set; }
    public float steerValue { get; private set; }

    void Awake()
    {
        if (inputActions == null)
        {
            Debug.LogError("Input Actions asset is not assigned!");
            return;
        }

        g27Map = inputActions.FindActionMap("G27", true);
        if (g27Map == null)
        {
            Debug.LogError("Could not find 'G27' action map!");
            return;
        }

        g27Throttle = g27Map.FindAction("G27Throttle");
        g27Steer = g27Map.FindAction("G27Steer");
        
        if (g27Throttle == null) Debug.LogError("Could not find 'G27Throttle' action!");
        if (g27Steer == null) Debug.LogError("Could not find 'G27Steer' action!");
    }

    void OnEnable()
    {
        g27Map?.Enable();
    }

    void OnDisable()
    {
        g27Map?.Disable();
    }

    void Start()
    {
        if (throttleCube) throttleInitialPos = throttleCube.transform.position;
        if (steerCube) steerInitialPos = steerCube.transform.position;
        if (controllerTypeText) controllerTypeText.text = "Active Controller: G27";
    }

    void Update()
    {
        if (g27Throttle == null || g27Steer == null) return;

        ProcessThrottleInputs();
        ProcessSteerInputs();
        UpdateVisuals();
    }

    private void ProcessThrottleInputs()
    {
        float throttleRaw = g27Throttle.ReadValue<float>();
        
        // Apply deadzone
        if (Mathf.Abs(throttleRaw) < inputDeadzone)
        {
            throttleValue = 0f;
        }
        else
        {
            throttleValue = throttleRaw;
        }
    }

    private void ProcessSteerInputs()
    {
        float steerRaw = g27Steer.ReadValue<float>();

        // Apply deadzone
        if (Mathf.Abs(steerRaw) < inputDeadzone)
        {
            steerValue = 0f;
        }
        else
        {
            steerValue = steerRaw;
        }
    }

    private void UpdateVisuals()
    {
        // Update throttle visuals
        if (throttleCube && !Mathf.Approximately(throttleValue, 0f))
        {
            Vector3 newPos = throttleInitialPos;
            newPos.y += throttleValue * maxThrottleDistance;
            throttleCube.transform.position = newPos;
        }

        if (throttleValueText)
        {
            throttleValueText.text = $"G27 Throttle: {throttleValue:F2}";
        }

        // Update steer visuals
        if (steerCube && !Mathf.Approximately(steerValue, 0f))
        {
            Vector3 newPos = steerInitialPos;
            newPos.y += steerValue * maxSteerDistance;
            steerCube.transform.position = newPos;
        }

        if (steerValueText)
        {
            steerValueText.text = $"G27 Steer: {steerValue:F2}";
        }
    }
}
