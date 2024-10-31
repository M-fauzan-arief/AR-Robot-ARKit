using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;

public class Apiro_Visual : MonoBehaviour
{
    public Button pickPlaceButton;
    public Button stopButton;  // Added stop button
    public Transform J1;
    public Transform J2;
    public Transform J3;
    public GameObject gripper;

    private MqttClient client;
    private string brokerAddress = "103.106.72.182";
    private int brokerPort = 1887;
    private string topic = "/robot/Apiro/jointValues";

    [Header("Motor Speeds")]
    [Range(0.01f, 1.0f)]
    public float motorSpeedJ1 = 0.1f;
    [Range(0.01f, 1.0f)]
    public float motorSpeedJ2 = 0.1f;
    [Range(0.01f, 1.0f)]
    public float motorSpeedJ3 = 0.1f;

    private bool isGripperClosed = false;
    private bool stopRequested = false;  // Flag to control stop request

    [Header("J1 Position Limits")]
    public float J1MinZ = 0.0f;
    public float J1MaxZ = 0.15f;

    private void Start()
    {
        client = new MqttClient(brokerAddress, brokerPort, false, null, null, MqttSslProtocols.None);
        try
        {
            client.Connect(Guid.NewGuid().ToString());
            if (client.IsConnected)
                Debug.Log("Connected to MQTT Broker!");
            else
                Debug.LogError("Failed to connect to MQTT Broker!");
        }
        catch (Exception ex)
        {
            Debug.LogError("MQTT connection error: " + ex.Message);
        }

        pickPlaceButton.onClick.AddListener(() => SendMqttCommand("1"));  // Start Pick and Place
        stopButton.onClick.AddListener(() => SendMqttCommand("2"));  // Stop Pick and Place
    }

    private void SendMqttCommand(string message)
    {
        if (client != null && client.IsConnected)
        {
            client.Publish(topic, Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            Debug.Log($"Sent MQTT message: {message}");
            if (message == "1")
                StartCoroutine(PickAndPlaceRoutine());
            else if (message == "2")
                stopRequested = true;  // Request to stop
        }
        else
        {
            Debug.LogError("Not connected to MQTT Broker!");
        }
    }

    private void OnApplicationQuit()
    {
        if (client != null && client.IsConnected)
            client.Disconnect();
    }

    private IEnumerator PickAndPlaceRoutine()
    {
        int i = 0;
        stopRequested = false;  // Reset stop flag at start of routine
        while (i < 6 && !stopRequested)
        {
            Debug.Log($"Iteration {i + 1} of Pick and Place started.");

            yield return StartCoroutine(StepperMotionAxis2(0, motorSpeedJ2, 0.06f));
            yield return StartCoroutine(StepperMotionAxis1(0, motorSpeedJ1, 0.015f));
            ControlGripper(true);
            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(StepperMotionAxis1(1, motorSpeedJ1, 0.015f));
            yield return StartCoroutine(StepperMotionAxis2(1, motorSpeedJ2, 0.06f));
            yield return StartCoroutine(StepperMotionAxis1(1, motorSpeedJ1, 0.1f));
            yield return StartCoroutine(StepperMotionAxis2(1, motorSpeedJ2, 0.09f));
            yield return StartCoroutine(StepperMotionAxis1(0, motorSpeedJ1, 0.028f));

            ControlGripper(false);
            yield return new WaitForSeconds(1.0f);

            yield return StartCoroutine(StepperMotionAxis1(1, motorSpeedJ1, 0.028f));
            yield return StartCoroutine(StepperMotionAxis2(0, motorSpeedJ2, 0.09f));
            yield return StartCoroutine(StepperMotionAxis1(0, motorSpeedJ1, 0.1f));

            Debug.Log($"Iteration {i + 1} of Pick and Place completed.");
            i++;
        }

        if (stopRequested)
            Debug.Log("Pick and Place operation stopped.");
        else
            Debug.Log("Pick and Place operation finished.");
    }

    private IEnumerator StepperMotionAxis1(int direction, float speed, float distance)
    {
        float startZ = J1.localPosition.z;
        float endZ = direction == 1 ? Mathf.Min(startZ + distance, J1MaxZ) : Mathf.Max(startZ - distance, J1MinZ);

        while (Mathf.Abs(J1.localPosition.z - endZ) > 0.001f)
        {
            if (stopRequested) yield break;  // Stop coroutine if stop requested
            J1.localPosition = new Vector3(J1.localPosition.x, J1.localPosition.y, Mathf.MoveTowards(J1.localPosition.z, endZ, speed * Time.deltaTime));
            yield return null;
        }
        Debug.Log($"Axis 1 (Z) reached position: {J1.localPosition.z}");
    }

    private IEnumerator StepperMotionAxis2(int direction, float speed, float distance)
    {
        float startZ = J2.localPosition.z;
        float endZ = direction == 1 ? startZ + distance : startZ - distance;

        while (Mathf.Abs(J2.localPosition.z - endZ) > 0.001f)
        {
            if (stopRequested) yield break;
            J2.localPosition = new Vector3(J2.localPosition.x, J2.localPosition.y, Mathf.MoveTowards(J2.localPosition.z, endZ, speed * Time.deltaTime));
            yield return null;
        }
        Debug.Log($"Axis 2 (Z) reached position: {J2.localPosition.z}");
    }

    private void ControlGripper(bool close)
    {
        isGripperClosed = close;
        gripper.transform.localScale = close ? new Vector3(1, 1, 0.5f) : new Vector3(1, 1, 1);
        Debug.Log("Gripper " + (close ? "closed" : "opened"));
    }
}
