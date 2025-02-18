using UnityEngine;
using TMPro;

public class ClickInputTranslate : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject throttleCube;
    [SerializeField] private GameObject steerCube;
    [SerializeField] private TextMeshPro throttleValueText;
    [SerializeField] private TextMeshPro steerValueText;

    [Header("Settings")]
    [SerializeField] private float maxThrottleDistance = 4f;
    [SerializeField] private float maxSteerDistance = 4f;
    [SerializeField] private float sensitivity = 0.01f;

    private Vector3 throttleInitialPos;
    private Vector3 steerInitialPos;
    private bool isDraggingThrottle;
    private bool isDraggingSteer;
    private Vector3 lastMousePosition;

    public float throttleValue { get; private set; }
    public float steerValue { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (throttleCube) throttleInitialPos = throttleCube.transform.position;
        if (steerCube) steerInitialPos = steerCube.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseInput();
        UpdateVisuals();
    }

    private void HandleMouseInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Handle mouse down
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == throttleCube)
                {
                    isDraggingThrottle = true;
                }
                else if (hit.collider.gameObject == steerCube)
                {
                    isDraggingSteer = true;
                }
                lastMousePosition = Input.mousePosition;
            }
        }
        // Handle mouse up
        else if (Input.GetMouseButtonUp(0))
        {
            isDraggingThrottle = false;
            isDraggingSteer = false;
        }
        // Handle dragging
        else if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            
            if (isDraggingThrottle)
            {
                throttleValue = Mathf.Clamp(throttleValue + delta.y * sensitivity, -1f, 1f);
            }
            else if (isDraggingSteer)
            {
                steerValue = Mathf.Clamp(steerValue + delta.x * sensitivity, -1f, 1f);
            }
            
            lastMousePosition = Input.mousePosition;
        }
        // Auto-center when not dragging
        else
        {
            throttleValue = Mathf.MoveTowards(throttleValue, 0f, Time.deltaTime);
            steerValue = Mathf.MoveTowards(steerValue, 0f, Time.deltaTime);
        }
    }

    private void UpdateVisuals()
    {
        // Update throttle visuals
        if (throttleCube)
        {
            Vector3 newPos = throttleInitialPos;
            newPos.y += throttleValue * maxThrottleDistance;
            throttleCube.transform.position = newPos;
        }

        if (throttleValueText)
        {
            throttleValueText.text = $"Click Throttle: {throttleValue:F2}";
        }

        // Update steer visuals
        if (steerCube)
        {
            Vector3 newPos = steerInitialPos;
            newPos.y += steerValue * maxSteerDistance;
            steerCube.transform.position = newPos;
        }

        if (steerValueText)
        {
            steerValueText.text = $"Click Steer: {steerValue:F2}";
        }
    }
}
