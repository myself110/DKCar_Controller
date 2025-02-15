using UnityEngine;
using UnityEngine.InputSystem;  // Required for new Input System (Gamepad inputs)
using TMPro;                    // Required for TextMeshPro UI elements

/// <summary>
/// Handles PS5 controller input mapping and visualization
/// Maps trigger and stick inputs to visual representations and text displays
/// </summary>
public class InputTranslate : MonoBehaviour
{
    [Header("Input References")]
    [SerializeField] private GameObject throttleCube;    // Cube that moves based on L2/R2 trigger input
    [SerializeField] private GameObject steerCube;       // Cube that moves based on left stick input

    [Header("Text Displays")]
    [SerializeField] private TextMeshPro throttleValueText;  // Displays the current throttle value (-1 to 1)
    [SerializeField] private TextMeshPro steerValueText;     // Displays the current steering value (-1 to 1)

    [Header("Input Settings")]
    [SerializeField] private float maxThrottleDistance = 4f;  // Maximum vertical distance the throttle cube can move
    [SerializeField] private float maxSteerDistance = 4f;     // Maximum vertical distance the steering cube can move

    // Cached input values
    private float throttleValue;  // Combined L2/R2 value: L2 = negative, R2 = positive (-1 to 1)
    private float steerValue;     // Left stick horizontal value: left = negative, right = positive (-1 to 1)
    private float previousThrottleValue;
    private float previousSteerValue;

    // Cached initial positions of the visualization cubes
    private Vector3 throttleInitialPos;
    private Vector3 steerInitialPos;

    /// <summary>
    /// Initializes the starting positions of the visualization cubes
    /// Called once when the script instance is being loaded
    /// </summary>
    void Start()
    {
        if (throttleCube) throttleInitialPos = throttleCube.transform.position;
        if (steerCube) steerInitialPos = steerCube.transform.position;
        
        previousThrottleValue = 0f;
        previousSteerValue = 0f;
    }

    /// <summary>
    /// Handles input processing and updates visualizations every frame
    /// </summary>
    void Update()
    {
        // Read PS5 trigger values (0 to 1 range for each trigger)
        float l2 = Gamepad.current?.leftTrigger.ReadValue() ?? 0f;
        float r2 = Gamepad.current?.rightTrigger.ReadValue() ?? 0f;
        
        // Combine triggers into single throttle value (-1 to 1)
        // L2 provides negative values, R2 provides positive values
        throttleValue = r2 - l2;

        // Read left stick horizontal axis (-1 to 1)
        // Negative = left, Positive = right
        steerValue = Gamepad.current?.leftStick.ReadValue().x ?? 0f;

        // Only update throttle if value changed
        if (!Mathf.Approximately(previousThrottleValue, throttleValue))
        {
            UpdateThrottleVisuals();
            if (throttleValueText)
            {
                throttleValueText.text = $"Throttle: {throttleValue:F2}";
            }
            previousThrottleValue = throttleValue;
        }

        // Only update steering if value changed
        if (!Mathf.Approximately(previousSteerValue, steerValue))
        {
            UpdateSteerVisuals();
            if (steerValueText)
            {
                steerValueText.text = $"Steer: {steerValue:F2}";
            }
            previousSteerValue = steerValue;
        }
    }

    /// <summary>
    /// Updates the throttle cube position based on trigger input
    /// Moves the cube vertically within the maxThrottleDistance range
    /// </summary>
    private void UpdateThrottleVisuals()
    {
        if (throttleCube)
        {
            Vector3 newPos = throttleInitialPos;
            newPos.y += throttleValue * maxThrottleDistance;  // Scale movement by maxThrottleDistance
            throttleCube.transform.position = newPos;
        }
    }

    /// <summary>
    /// Updates the steering cube position based on left stick input
    /// Moves the cube vertically within the maxSteerDistance range
    /// </summary>
    private void UpdateSteerVisuals()
    {
        if (steerCube)
        {
            Vector3 newPos = steerInitialPos;
            newPos.y += steerValue * maxSteerDistance;  // Scale movement by maxSteerDistance
            steerCube.transform.position = newPos;
        }
    }
}
