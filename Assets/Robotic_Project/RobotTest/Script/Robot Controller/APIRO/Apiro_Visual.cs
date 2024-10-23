using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;  // Add reference to M2Mqtt
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;

public class Apiro_Visual : MonoBehaviour
{
    // UI elements for triggering the pick and place operation
    public Button pickPlaceButton;
    public Button enableMotorButton;
    public Button disableMotorButton;
    public Button playPatternButton;
    public Button gripperOnButton;
    public Button gripperOffButton;
    public Button stopMotorButton;

    // Reference to Transforms simulating stepper motors or objects
    public Transform J1;
    public Transform J2;
    public Transform J3;
    public GameObject gripper; // For gripper control (open/close)

    // MQTT connection details
    private MqttClient client;
    private string brokerAddress = "103.106.72.182"; // ESP32 IP
    private int brokerPort = 1887; // Port for MQTT
    private string topic = "/robot/Apiro/jointValues";

    // Grouping the motor speed settings in the editor
    [Header("Motor Speeds")]
    [Range(0.01f, 1.0f)]
    public float motorSpeedJ1 = 0.1f;  // Speed for J1, adjustable in editor
    [Range(0.01f, 1.0f)]
    public float motorSpeedJ2 = 0.1f;  // Speed for J2, adjustable in editor
    [Range(0.01f, 1.0f)]
    public float motorSpeedJ3 = 0.1f;  // Speed for J3, adjustable in editor

    private bool isGripperClosed = false;

    // Grouping the J1 position limits in the editor
    [Header("J1 Position Limits")]
    public float J1MinZ = 0.0f;
    public float J1MaxZ = 0.15f;

    private void Start()
    {
        // Initialize MQTT connection using brokerAddress and brokerPort
        client = new MqttClient(brokerAddress, brokerPort, false, null, null, MqttSslProtocols.None);
        try
        {
            client.Connect(Guid.NewGuid().ToString());  // Client ID can be any unique string

            // Check if client is connected
            if (client.IsConnected)
            {
                Debug.Log("Connected to MQTT Broker!");
            }
            else
            {
                Debug.LogError("Failed to connect to MQTT Broker!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("MQTT connection error: " + ex.Message);
        }

        // Set button listeners for the commands
        pickPlaceButton.onClick.AddListener(() => StartCoroutine(PickAndPlaceRoutine()));  // Trigger Pick and Place routine
        enableMotorButton.onClick.AddListener(() => SendMqttCommand("1"));  // Enable motor
        disableMotorButton.onClick.AddListener(() => SendMqttCommand("2"));  // Disable motor
        playPatternButton.onClick.AddListener(() => SendMqttCommand("3"));  // Play pattern 1
        gripperOnButton.onClick.AddListener(() => SendMqttCommand("4"));  // Gripper ON
        gripperOffButton.onClick.AddListener(() => SendMqttCommand("5"));  // Gripper OFF
        stopMotorButton.onClick.AddListener(() => SendMqttCommand("6"));  // Stop motor
    }

    // Method to send MQTT messages
    private void SendMqttCommand(string message)
    {
        if (client != null && client.IsConnected)
        {
            // Send the command to the ESP32
            client.Publish(topic, Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            Debug.Log($"Sent MQTT message: {message}");
        }
        else
        {
            Debug.LogError("Not connected to MQTT Broker!");
        }
    }

    private void OnApplicationQuit()
    {
        // Disconnect from MQTT broker when the application quits
        if (client != null && client.IsConnected)
        {
            client.Disconnect();
        }
    }

    // This method simulates the pick and place object operation
    private IEnumerator PickAndPlaceRoutine()
    {
        int i = 0;
        while (i < 6)
        {
            Debug.Log($"Iteration {i + 1} of Pick and Place started.");

            // Simulate movements with delays based on the Arduino code provided
            yield return StartCoroutine(StepperMotionAxis2(0, motorSpeedJ2, 0.06f));  // Move J2 by 60 units
            yield return StartCoroutine(StepperMotionAxis1(0, motorSpeedJ1, 0.015f));  // Move J1 down by 15 units
            ControlGripper(true);  // Close gripper
            yield return new WaitForSeconds(0.5f);  // Delay 500 ms

            // Continue with more movements
            yield return StartCoroutine(StepperMotionAxis1(1, motorSpeedJ1, 0.015f));  // Move J1 up by 15 units
            yield return StartCoroutine(StepperMotionAxis2(1, motorSpeedJ2, 0.06f));  // Move J2 by 60 units up
            yield return StartCoroutine(StepperMotionAxis1(1, motorSpeedJ1, 0.1f));  // Move J1 up by 100 units
            yield return StartCoroutine(StepperMotionAxis2(1, motorSpeedJ2, 0.09f));  // Move J2 by 90 units up
            yield return StartCoroutine(StepperMotionAxis1(0, motorSpeedJ1, 0.028f));  // Move J1 down by 28 units

            ControlGripper(false);  // Open gripper
            yield return new WaitForSeconds(1.0f);  // Delay 1000 ms

            // Final movements in the iteration
            yield return StartCoroutine(StepperMotionAxis1(1, motorSpeedJ1, 0.028f));  // Move J1 up by 28 units
            yield return StartCoroutine(StepperMotionAxis2(0, motorSpeedJ2, 0.09f));  // Move J2 by 90 units down
            yield return StartCoroutine(StepperMotionAxis1(0, motorSpeedJ1, 0.1f));  // Move J1 down by 100 units

            Debug.Log($"Iteration {i + 1} of Pick and Place completed.");
            i++;
        }

        Debug.Log("Pick and Place operation finished.");
    }

    // Simulate motion of Axis 1 (J1) with min/max limits
    private IEnumerator StepperMotionAxis1(int direction, float speed, float distance)
    {
        float startZ = J1.localPosition.z;
        float endZ = direction == 1 ? Mathf.Min(startZ + distance, J1MaxZ) : Mathf.Max(startZ - distance, J1MinZ);

        while (Mathf.Abs(J1.localPosition.z - endZ) > 0.001f)
        {
            J1.localPosition = new Vector3(J1.localPosition.x, J1.localPosition.y, Mathf.MoveTowards(J1.localPosition.z, endZ, speed * Time.deltaTime));
            yield return null;  // Wait for next frame
        }
        Debug.Log($"Axis 1 (Z) reached position: {J1.localPosition.z}");
    }

    // Simulate motion of Axis 2 (J2) along Z axis
    private IEnumerator StepperMotionAxis2(int direction, float speed, float distance)
    {
        float startZ = J2.localPosition.z;
        float endZ = direction == 1 ? startZ + distance : startZ - distance;

        while (Mathf.Abs(J2.localPosition.z - endZ) > 0.001f)
        {
            J2.localPosition = new Vector3(J2.localPosition.x, J2.localPosition.y, Mathf.MoveTowards(J2.localPosition.z, endZ, speed * Time.deltaTime));
            yield return null;  // Wait for next frame
        }
        Debug.Log($"Axis 2 (Z) reached position: {J2.localPosition.z}");
    }

    // Simulate motion of Axis 3 (J3) along Z axis (for completeness)
    private IEnumerator StepperMotionAxis3(int direction, float speed, float distance)
    {
        float startZ = J3.localPosition.z;
        float endZ = direction == 1 ? startZ + distance : startZ - distance;

        while (Mathf.Abs(J3.localPosition.z - endZ) > 0.001f)
        {
            J3.localPosition = new Vector3(J3.localPosition.x, J3.localPosition.y, Mathf.MoveTowards(J3.localPosition.z, endZ, speed * Time.deltaTime));
            yield return null;  // Wait for next frame
        }
        Debug.Log($"Axis 3 (Z) reached position: {J3.localPosition.z}");
    }

    // Control Gripper to open or close
    private void ControlGripper(bool close)
    {
        isGripperClosed = close;
        if (close)
        {
            // Simulate gripper closing action
            gripper.transform.localScale = new Vector3(1, 1, 0.5f); // Example gripper close simulation
        }
        else
        {
            // Simulate gripper opening action
            gripper.transform.localScale = new Vector3(1, 1, 1); // Example gripper open simulation
        }

        Debug.Log("Gripper " + (close ? "closed" : "opened"));
    }
}
