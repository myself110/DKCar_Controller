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

    private Vector3 throttleInitialPos;
    private Vector3 steerInitialPos;
    public float throttleValue { get; private set; }
    public float steerValue { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (throttleCube) throttleInitialPos = throttleCube.transform.position;
        if (steerCube) steerInitialPos = steerCube.transform.position;
        if (controllerTypeText) controllerTypeText.text = "Active Controller: G27";
    }

    // Update is called once per frame
    void Update()
    {
        ProcessThrottleInputs();
        ProcessSteerInputs();
        UpdateVisuals();
    }

    private void ProcessThrottleInputs()
    {
        // G27 pedals are typically on axes 3 (accelerator) and 2 (brake)
        // Accelerator: 1 to -1 (released to pressed)
        // Brake: -1 to 1 (released to pressed)
        float accelerator = Input.GetAxis("G27 Accelerator");
        float brake = Input.GetAxis("G27 Brake");

        // Invert and remap accelerator from (-1 to 1) to (0 to 1)
        float acceleratorMapped = Mathf.InverseLerp(-1f, 1f, -accelerator);

        // Remap brake from (-1 to 1) to (0 to -1)
        float brakeMapped = -Mathf.InverseLerp(-1f, 1f, brake);

        // Apply deadzone
        if (Mathf.Abs(acceleratorMapped) < inputDeadzone) acceleratorMapped = 0f;
        if (Mathf.Abs(brakeMapped) < inputDeadzone) brakeMapped = 0f;

        // Use whichever input has larger absolute value
        throttleValue = Mathf.Abs(acceleratorMapped) > Mathf.Abs(brakeMapped) 
            ? acceleratorMapped 
            : brakeMapped;
    }

    private void ProcessSteerInputs()
    {
        // G27 steering is typically on axis 1
        // Raw value is -1 (left) to 1 (right)
        float steerRaw = Input.GetAxis("G27 Steering");

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
