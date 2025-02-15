using UnityEngine;
using UnityEngine.InputSystem;

public class NISTestMoza : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    private InputAction mozaSteer;
    private InputActionMap mozaMap;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Awake()
    {
        // Debug input asset
        if (inputActions == null)
        {
            Debug.LogError("Input Actions asset is not assigned in Inspector!");
            return;
        }

        // Debug action map
        mozaMap = inputActions.FindActionMap("Moza", true);
        if (mozaMap == null)
        {
            Debug.LogError("Could not find 'Moza' action map!");
            return;
        }

        // Debug steer action
        mozaSteer = mozaMap.FindAction("MozaSteer");
        if (mozaSteer == null)
        {
            Debug.LogError("Could not find 'MozaSteer' action!");
            return;
        }

        Debug.Log("Successfully initialized MOZA input actions");
    }

    void OnEnable()
    {
        if (mozaMap != null)
        {
            mozaMap.Enable();
        }
    }

    void OnDisable()
    {
        if (mozaMap != null)
        {
            mozaMap.Disable();
        }
    }

    void FixedUpdate()
    {
        if (mozaSteer == null)
        {
            Debug.LogWarning("MozaSteer action is null! Did you assign the Input Actions asset?");
            return;
        }

        float steerValue = mozaSteer.ReadValue<float>();
        if (Mathf.Abs(steerValue) > 0.01f)
        {
            Debug.Log($"MOZA Steer Input: {steerValue}");
            
            if (mozaSteer.activeControl != null)
            {
                Debug.Log($"Active Control: {mozaSteer.activeControl.name} | Path: {mozaSteer.activeControl.path}");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
