using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MozaInputTranslate : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject throttleCube;
    [SerializeField] private GameObject steerCube;
    [SerializeField] private TextMeshPro throttleValueText;
    [SerializeField] private TextMeshPro steerValueText;

    [Header("Settings")]
    [SerializeField] private float maxThrottleDistance = 4f;
    [SerializeField] private float maxSteerDistance = 4f;
    [SerializeField] private float inputDeadzone = 0.01f;
    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap mozaMap;
    private InputAction mozaRX;
    private InputAction mozaRY;
    private InputAction mozaSteerLeft;
    private InputAction mozaSteerRight;
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
        
        if (mozaRX == null) Debug.LogError("Could not find 'MozaRX' action!");
        if (mozaRY == null) Debug.LogError("Could not find 'MozaRY' action!");
        if (mozaSteerLeft == null) Debug.LogError("Could not find 'MozaSteerLeft' action!");
        if (mozaSteerRight == null) Debug.LogError("Could not find 'MozaSteerRight' action!");
    }

    void Start()
    {
        if (throttleCube) throttleInitialPos = throttleCube.transform.position;
        if (steerCube) steerInitialPos = steerCube.transform.position;
    }

    void OnEnable()
    {
        mozaMap?.Enable();
    }

    void OnDisable()
    {
        mozaMap?.Disable();
    }

    void Update()
    {
        if (mozaRX == null || mozaRY == null || mozaSteerLeft == null || mozaSteerRight == null) return;

        ProcessThrottleInputs();
        ProcessSteerInputs();
    }

    private void ProcessThrottleInputs()
    {
        float rxRaw = mozaRX.ReadValue<float>();  // -1 to +1
        float ryRaw = mozaRY.ReadValue<float>();  // +1 to -1

        // Map RX from (-1 to +1) to (0 to +1)
        float rxMapped = Mathf.InverseLerp(-1f, 1f, rxRaw);

        // Map RY from (+1 to -1) to (0 to -1)
        float ryMapped = Mathf.InverseLerp(1f, -1f, ryRaw) * -1f;

        // Apply deadzone
        if (Mathf.Abs(rxMapped) < inputDeadzone) rxMapped = 0f;
        if (Mathf.Abs(ryMapped) < inputDeadzone) ryMapped = 0f;

        // Use whichever input has larger absolute value
        throttleValue = Mathf.Abs(rxMapped) > Mathf.Abs(ryMapped) ? rxMapped : ryMapped;
        
        UpdateThrottleVisuals();
    }

    private void ProcessSteerInputs()
    {
        float leftStickRaw = mozaSteerLeft.ReadValue<float>();   // +1 to 0
        float rightStickRaw = mozaSteerRight.ReadValue<float>(); // -1 to 0

        float mappedValue = 0f;

        // Process left stick if it's active
        if (leftStickRaw > 0f + inputDeadzone)
        {
            // Map from (+1 to 0) to (0 to -1)
            mappedValue = (1f - leftStickRaw) * -1f;
        }
        // Process right stick if it's active
        else if (rightStickRaw < 0f - inputDeadzone)
        {
            // Map from (-1 to 0) to (0 to +1)
            mappedValue = Mathf.InverseLerp(-1f, 0f, rightStickRaw);
        }

        steerValue = mappedValue;

        // Debug steer values
        if (Mathf.Abs(steerValue) > inputDeadzone)
        {
            if (leftStickRaw > 0f + inputDeadzone)
            {
                Debug.Log($"Left Raw: {leftStickRaw:F3} → Mapped: {steerValue:F3}");
            }
            else if (rightStickRaw < 0f - inputDeadzone)
            {
                Debug.Log($"Right Raw: {rightStickRaw:F3} → Mapped: {steerValue:F3}");
            }
        }

        UpdateSteerVisuals();
    }

    private void UpdateThrottleVisuals()
    {
        if (throttleCube && !Mathf.Approximately(throttleValue, 0f))
        {
            Vector3 newPos = throttleInitialPos;
            newPos.y += throttleValue * maxThrottleDistance;
            throttleCube.transform.position = newPos;
        }

        if (throttleValueText)
        {
            throttleValueText.text = $"Moza Throttle: {throttleValue:F2}";
        }
    }

    private void UpdateSteerVisuals()
    {
        if (steerCube && !Mathf.Approximately(steerValue, 0f))
        {
            Vector3 newPos = steerInitialPos;
            newPos.y += steerValue * maxSteerDistance;
            steerCube.transform.position = newPos;
        }

        if (steerValueText)
        {
            steerValueText.text = $"Moza Steer: {steerValue:F2}";
        }
    }
}
