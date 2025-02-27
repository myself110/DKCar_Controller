using UnityEngine;

public class TwoCubeReader : MonoBehaviour
{
    [SerializeField] private TwoCubeController cubeController;
    [SerializeField] private float logThreshold = 0.01f; // Only log when values change by this amount

    private float lastLeftValue;
    private float lastRightValue;
    private float mappedLeftValue;
    private float mappedRightValue;

    void Start()
    {
        if (cubeController == null)
        {
            Debug.LogError("TwoCubeController reference is missing!");
            enabled = false;
            return;
        }

        // Initialize last values
        lastLeftValue = cubeController.leftValue;
        lastRightValue = cubeController.rightValue;
    }

    void Update()
    {
        float rawLeft = cubeController.leftValue;   // Raw value from -1 to +1
        float rawRight = cubeController.rightValue; // Raw value from -1 to +1

        // Map both values from -1,+1 to 0,+1
        mappedLeftValue = Mathf.InverseLerp(-1f, 1f, rawLeft);
        mappedRightValue = Mathf.InverseLerp(-1f, 1f, rawRight);

        // Check if mapped values have changed significantly
        if (Mathf.Abs(mappedLeftValue - lastLeftValue) > logThreshold || 
            Mathf.Abs(mappedRightValue - lastRightValue) > logThreshold)
        {
            Debug.Log($"TwoCubeReader - Left[0 to +1]: {mappedLeftValue:F2}, Right[0 to +1]: {mappedRightValue:F2}");
            
            // Update last values
            lastLeftValue = mappedLeftValue;
            lastRightValue = mappedRightValue;
        }
    }

    // Public accessors for the mapped values
    public float GetMappedLeftValue() => mappedLeftValue;
    public float GetMappedRightValue() => mappedRightValue;
} 