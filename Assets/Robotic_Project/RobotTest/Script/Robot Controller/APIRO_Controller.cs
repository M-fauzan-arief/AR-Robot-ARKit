using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EndEffector
{
    public string type;
    public string enable;
}

[System.Serializable]
public class Data
{
    public string j1, j2, j3, status;
    public EndEffector endEffector;
}

[System.Serializable]
public class RobotMessageApiro // Changed the class name to RobotMessageApiro
{
    public string nodeID, moveType;
    public Data data;
    public long unixtime;
}

public class APIRO_Controller : MonoBehaviour
{
    // Slider in UI
    public Slider J1_Sliders;
    public Slider J2_Sliders;
    public Slider J3_Sliders;

    // Default Slider values
    public float J1_SlidersValue = 0.0f;
    public float J2_SlidersValue = 0.0f;
    public float J3_SlidersValue = 0.0f;

    // Reference to Transform for each joint
    public Transform J1;
    public Transform J2;
    public Transform J3;

    // Rotation and movement speed for each joint
    public float J1_MoveRate = 0.1f; // Reduced the rate for slower movement
    public float J2_TurnRate = 1.0f;
    public float J3_TurnRate = 1.0f;

    // Limits for Joint 1's movement
    private float J1ZPos = 0.043f;
    private float J1ZPosMin = -0.1f; // Adjust according to your needs
    private float J1ZPosMax = 0.129f;  // Adjust according to your needs

    // Limits for Joint 2's rotation
    private float J2YRot = 20.0f;
    private float J2YRotMin = -74.0f;
    private float J2YRotMax = 107.0f;

    // Limits for Joint 3's rotation
    private float J3YRot = 0f;
    private float J3YRotMin = -90.0f;
    private float J3YRotMax = 90.0f;

    private MQTT_Client_Apiro mqttClient; // Updated reference to MQTT_Client_Apiro
    private bool endEffectorEnabled = false; // Assuming false; adjust as needed
    private float lastDataSentTime = 0f;
    private float dataSendingInterval = 0.5f; // Send data every 0.5 seconds
    private bool jointValuesChanged = false; // Flag to track changes

    // Start is called before the first frame update
    void Start()
    {
        // Get the MQTT_Client_Apiro component
        mqttClient = GetComponent<MQTT_Client_Apiro>();

        // Set minimum values for sliders
        J1_Sliders.minValue = -1;
        J2_Sliders.minValue = -1;
        J3_Sliders.minValue = -1;

        // Set maximum values for sliders
        J1_Sliders.maxValue = 1;
        J2_Sliders.maxValue = 1;
        J3_Sliders.maxValue = 1;
    }

    // Check for input values from sliders
    void CheckInput()
    {
        float prevJ1Value = J1_SlidersValue;
        float prevJ2Value = J2_SlidersValue;
        float prevJ3Value = J3_SlidersValue;

        J1_SlidersValue = J1_Sliders.value;
        J2_SlidersValue = J2_Sliders.value;
        J3_SlidersValue = J3_Sliders.value;

        // Check if any slider value has changed
        if (J1_SlidersValue != prevJ1Value || J2_SlidersValue != prevJ2Value || J3_SlidersValue != prevJ3Value)
        {
            jointValuesChanged = true;
        }
    }

    // Process movement for each joint
    void ProcessMovement()
    {
        // Joint 1 movement
        float targetZPos = Mathf.Clamp(J1.localPosition.z + J1_SlidersValue * J1_MoveRate, J1ZPosMin, J1ZPosMax);
        J1ZPos = Mathf.Lerp(J1.localPosition.z, targetZPos, Time.deltaTime * 0.8f); // Adjust interpolation speed as needed
        J1.localPosition = new Vector3(J1.localPosition.x, J1.localPosition.y, J1ZPos);

        // Joint 2 rotation
        J2YRot += J2_SlidersValue * J2_TurnRate;
        J2YRot = Mathf.Clamp(J2YRot, J2YRotMin, J2YRotMax);
        J2.localEulerAngles = new Vector3(J2.localEulerAngles.x, J2.localEulerAngles.y, J2YRot);

        // Joint 3 rotation
        J3YRot += J3_SlidersValue * J3_TurnRate;
        J3YRot = Mathf.Clamp(J3YRot, J3YRotMin, J3YRotMax);
        J3.localEulerAngles = new Vector3(J3.localEulerAngles.x, -J3.localEulerAngles.x, J3YRot);
    }

    // Reset slider values
    public void ResetSlider()
    {
        J1_SlidersValue = 0.0f;
        J2_SlidersValue = 0.0f;
        J3_SlidersValue = 0.0f;

        J1_Sliders.value = 0.0f;
        J2_Sliders.value = 0.0f;
        J3_Sliders.value = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        ProcessMovement();

        // Send MQTT data at intervals or when joint values change
        if (jointValuesChanged && Time.time - lastDataSentTime >= dataSendingInterval)
        {
            SendJointValues();
            lastDataSentTime = Time.time;
            jointValuesChanged = false;
        }
    }

    // Send joint values via MQTT
    void SendJointValues()
    {
        if (mqttClient != null && mqttClient.IsConnected())
        {
            var endEffector = new EndEffector
            {
                type = "suck",
                enable = endEffectorEnabled.ToString()
            };

            var data = new Data
            {
                j1 = J1ZPos.ToString("F3"), // Formatting to 3 decimal places
                j2 = J2YRot.ToString("F3"),
                j3 = J3YRot.ToString("F3"),
                status = "True",
                endEffector = endEffector
            };

            var robotMessage = new RobotMessageApiro // Use RobotMessageApiro here
            {
                nodeID = "apiro-01",
                moveType = "joint",
                data = data,
                unixtime = GetUnixTimestamp()
            };

            mqttClient.PublishJointValues(robotMessage);
            Debug.Log("Joint values sent via MQTT.");
        }
        else
        {
            Debug.LogError("MQTT client is not connected. Cannot send joint values.");
        }
    }

    private long GetUnixTimestamp()
    {
        System.DateTime unixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        return (long)(System.DateTime.UtcNow - unixEpoch).TotalSeconds;
    }
}
