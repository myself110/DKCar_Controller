using UnityEngine;

public class TwoCubeController : MonoBehaviour
{
    [Header("Cube References")]
    [SerializeField] private GameObject leftCube;
    [SerializeField] private GameObject rightCube;

    [Header("Settings")]
    [SerializeField] private float maxDistance = 4f;  // Maximum distance cubes can move up/down
    [SerializeField] private float moveSpeed = 5f;    // Speed of cube movement

    private Vector3 leftInitialPos;
    private Vector3 rightInitialPos;
    private bool isDraggingLeft;
    private bool isDraggingRight;

    // Output values (-1 to +1)
    private float _leftValue;
    private float _rightValue;
    
    // Public accessors for the values
    public float leftValue => _leftValue;
    public float rightValue => _rightValue;

    void Start()
    {
        // Set initial positions at y=4 (representing +1)
        if (leftCube)
        {
            leftInitialPos = leftCube.transform.position;
            leftCube.transform.position = new Vector3(leftInitialPos.x, maxDistance, leftInitialPos.z);
            leftInitialPos = leftCube.transform.position; // Update initial position to the new height
            _leftValue = 1f; // Start at +1
        }
        
        if (rightCube)
        {
            rightInitialPos = rightCube.transform.position;
            rightCube.transform.position = new Vector3(rightInitialPos.x, maxDistance, rightInitialPos.z);
            rightInitialPos = rightCube.transform.position; // Update initial position to the new height
            _rightValue = 1f; // Start at +1
        }
    }

    void Update()
    {
        HandleInput();
        UpdateValues();
    }

    private void HandleInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Handle mouse down
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == leftCube)
                {
                    isDraggingLeft = true;
                }
                else if (hit.collider.gameObject == rightCube)
                {
                    isDraggingRight = true;
                }
            }
        }
        // Handle mouse up
        else if (Input.GetMouseButtonUp(0))
        {
            isDraggingLeft = false;
            isDraggingRight = false;
        }

        // Handle dragging
        if (Input.GetMouseButton(0))
        {
            float mouseY = Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;

            if (isDraggingLeft)
            {
                MoveCube(leftCube, leftInitialPos, ref _leftValue, mouseY);
            }
            else if (isDraggingRight)
            {
                MoveCube(rightCube, rightInitialPos, ref _rightValue, mouseY);
            }
        }
    }

    private void MoveCube(GameObject cube, Vector3 initialPos, ref float value, float deltaY)
    {
        if (!cube) return;

        // Calculate new position
        Vector3 newPos = cube.transform.position;
        newPos.y += deltaY * maxDistance;

        // Clamp position within bounds (maxDistance to -maxDistance)
        newPos.y = Mathf.Clamp(newPos.y, 0, maxDistance * 2);

        // Update cube position
        cube.transform.position = newPos;

        // Update value (-1 to +1), mapping from (0 to maxDistance*2) to (-1 to +1)
        value = Mathf.Lerp(-1f, 1f, newPos.y / (maxDistance * 2));
    }

    private void UpdateValues()
    {
        // Auto-return to center when not dragging
        if (!isDraggingLeft)
        {
            _leftValue = Mathf.MoveTowards(_leftValue, 0f, Time.deltaTime);
            if (leftCube)
            {
                Vector3 newPos = leftInitialPos;
                // Map from -1,1 to 0,maxDistance*2
                float mappedY = Mathf.Lerp(0, maxDistance * 2, (_leftValue + 1f) / 2f);
                newPos.y = mappedY;
                leftCube.transform.position = newPos;
            }
        }

        if (!isDraggingRight)
        {
            _rightValue = Mathf.MoveTowards(_rightValue, 0f, Time.deltaTime);
            if (rightCube)
            {
                Vector3 newPos = rightInitialPos;
                // Map from -1,1 to 0,maxDistance*2
                float mappedY = Mathf.Lerp(0, maxDistance * 2, (_rightValue + 1f) / 2f);
                newPos.y = mappedY;
                rightCube.transform.position = newPos;
            }
        }
    }
} 