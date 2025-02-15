using UnityEngine;
using TMPro;

public class MozaTest : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject throttleCube;

    [Header("Text Displays")]
    [SerializeField] private TextMeshPro throttleValueText;

    [Header("Input Settings")]
    [SerializeField] private float maxThrottleDistance = 4f;
    [SerializeField] private float inputDeadzone = 0.01f;
    [SerializeField] private bool invertThrottle = false;
    [SerializeField] private bool invertBrake = false;

    // Cached input values
    private float throttleValue;  // Combined value: brake = negative, gas = positive (-1 to 1)
    private float previousThrottleValue;  // Store previous value to detect changes

    // Cached initial positions
    private Vector3 throttleInitialPos;

    void Start()
    {
        if (throttleCube) throttleInitialPos = throttleCube.transform.position;
        previousThrottleValue = 0f;
    }

    void Update()
    {
        // Get raw input from both pedals using the correct axis names from input debugger
        float gasInput = Input.GetAxis("RX");      // Gas pedal (Rx axis)
        float brakeInput = Input.GetAxis("RY");    // Brake pedal (Ry axis)

        // Process pedal inputs
        ProcessPedalInputs(gasInput, brakeInput);

        // Only update visuals if the value has changed
        if (!Mathf.Approximately(previousThrottleValue, throttleValue))
        {
            UpdateThrottleVisuals();
            UpdateTextDisplay();
            previousThrottleValue = throttleValue;
        }
    }

    private void ProcessPedalInputs(float gasInput, float brakeInput)
    {
        // Apply deadzone and inversion for gas pedal
        float gas = Mathf.Abs(gasInput) > inputDeadzone ? gasInput : 0f;
        gas = invertThrottle ? -gas : gas;

        // Apply deadzone and inversion for brake pedal
        float brake = Mathf.Abs(brakeInput) > inputDeadzone ? brakeInput : 0f;
        brake = invertBrake ? -brake : brake;

        // Combine into single throttle value
        // Gas provides positive values, Brake provides negative values
        throttleValue = gas - brake;
        
        // Clamp the final value between -1 and 1
        throttleValue = Mathf.Clamp(throttleValue, -1f, 1f);
    }

    private void UpdateThrottleVisuals()
    {
        if (throttleCube)
        {
            Vector3 newPos = throttleInitialPos;
            newPos.y += throttleValue * maxThrottleDistance;
            throttleCube.transform.position = newPos;
        }
    }

    private void UpdateTextDisplay()
    {
        if (throttleValueText)
        {
            throttleValueText.text = $"Throttle: {throttleValue:F2}";
        }
    }

    // Public accessor for other scripts
    public float GetThrottleValue() => throttleValue;
}
