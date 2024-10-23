using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class APIROController : MonoBehaviour
{
    // Sliders in UI
    public Slider J1_Slider;
    public Slider J2_Slider;
    public Slider J3_Slider;

    // Reference to Transform for each joint
    public Transform J1;
    public Transform J2;
    public Transform J3;

    // Variables to hold joint positions and rotation speeds
    private float J1ZPos = 0.0f;
    private float J2YRot = 0.0f;
    private float J3YRot = 0.0f;

    private MQTT_Client_Apiro mqttClient; // Reference to MQTT client
    private bool endEffectorEnabled = false; // Assuming false; adjust as needed
    private float lastDataSentTime = 0f;
    private float dataSendingInterval = 0.5f; // Send data every 0.5 seconds
    private bool jointValuesChanged = false; // Flag to track changes

    private void Start()
    {
        // Get the MQTT_Client_Apiro component
        mqttClient = GetComponent<MQTT_Client_Apiro>();

        // Set minimum and maximum values for sliders
        J1_Slider.minValue = -1;
        J1_Slider.maxValue = 1;
        J2_Slider.minValue = -1;
        J2_Slider.maxValue = 1;
        J3_Slider.minValue = -1;
        J3_Slider.maxValue = 1;
    }

    private void Update()
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

    // Check for input values from sliders
    private void CheckInput()
    {
        // Check if slider values have changed
        jointValuesChanged = J1_Slider.value != J1ZPos || J2_Slider.value != J2YRot || J3_Slider.value != J3YRot;

        // Update joint positions based on slider values
        J1ZPos = J1_Slider.value;
        J2YRot = J2_Slider.value;
        J3YRot = J3_Slider.value;
    }

    // Process movement for each joint
    private void ProcessMovement()
    {
        // Joint 1 movement
        J1.localPosition = new Vector3(J1.localPosition.x, J1.localPosition.y, J1ZPos);

        // Joint 2 rotation
        J2.localEulerAngles = new Vector3(J2.localEulerAngles.x, J2.localEulerAngles.y, J2YRot);

        // Joint 3 rotation
        J3.localEulerAngles = new Vector3(J3.localEulerAngles.x, J3.localEulerAngles.y, J3YRot);
    }

    // Send joint values via MQTT
    private void SendJointValues()
    {
        if (mqttClient != null && mqttClient.IsConnected())
        {
            var endEffector = new EndEffector
            {
                type = "suck",
                enable = endEffectorEnabled.ToString()
            };

            var data = new JointData
            {
                j1 = J1ZPos.ToString("F3"), // Formatting to 3 decimal places
                j2 = J2YRot.ToString("F3"),
                j3 = J3YRot.ToString("F3"),
                status = "True", // Example status
                endEffector = endEffector
            };

            var robotMessage = new JointRobotMessage
            {
                nodeID = "apiro-01", // Set nodeID as required
                moveType = "joint",   // Move type can be adjusted
                data = data,
                unixtime = GetUnixTimestamp() // Get the current Unix time
            };

            mqttClient.PublishJointValues(robotMessage);
            Debug.Log("Joint values sent via MQTT.");
        }
        else
        {
            Debug.LogError("MQTT client is not connected. Cannot send joint values.");
        }
    }

    // Method to get the current Unix timestamp
    private long GetUnixTimestamp()
    {
        return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
    }

    // Private classes to avoid ambiguity
    [System.Serializable]
    private class JointData
    {
        public string j1, j2, j3, status; // Joint positions and status
        public EndEffector endEffector;   // Include end effector information
    }

    [System.Serializable]
    private class JointRobotMessage
    {
        public string nodeID;   // Unique identifier for the robot node
        public string moveType; // Type of movement (e.g., joint or visual)
        public JointData data;  // Data object holding joint information and end effector state
        public long unixtime;   // Unix time as long
    }

    [System.Serializable]
    public class EndEffector
    {
        public string type;     // Type of end effector (e.g., "suck" or "grip")
        public string enable;   // Enable or disable the end effector
    }
}

// Placeholder MQTT Client class
public class MQTT_Client_Apiro : MonoBehaviour
{
    public bool IsConnected()
    {
        // Placeholder method to simulate connection status
        return true;
    }

    public void PublishJointValues(object message)
    {
        // Placeholder method for publishing joint values
        Debug.Log("Publishing message: " + message.ToString());
    }
}
