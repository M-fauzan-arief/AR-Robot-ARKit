using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using apirorobotmessage;

public class APIROController : MonoBehaviour
{

    [SerializeField] private Button J1_Plus, J1_Minus, J2_Plus, J2_Minus, J3_Plus, J3_Minus;
    [SerializeField] private Transform J1, J2, J3;

    [Header("Universal Settings")]
    [Tooltip("Universal rotation speed for all joints except Joint 1 (degrees per second for rotation).")]
    [SerializeField] [Range(0.1f, 100f)] private float universalRotationSpeed = 20f;

    [Header("Joint 1 - Positional Control (Z-axis)")]
    [Tooltip("Movement speed along Z-axis for Joint 1 (cm per second).")]
    [SerializeField] [Min(0.0f)] private float J1Speed = 1f;
    [Tooltip("Starting position along Z-axis for Joint 1.")]
    [SerializeField] private float J1StartPosition = 0f;
    [Tooltip("Minimum position along Z-axis for Joint 1.")]
    [SerializeField] private float J1MinPosition = -1f;
    [Tooltip("Maximum position along Z-axis for Joint 1.")]
    [SerializeField] private float J1MaxPosition = 1f;

    [Header("Joint 2 - Rotational Control (Z-axis)")]
    [Tooltip("Minimum rotation in degrees for Joint 2 around the Z-axis.")]
    [SerializeField] private float J2MinRotation = -90f;
    [Tooltip("Maximum rotation in degrees for Joint 2 around the Z-axis.")]
    [SerializeField] private float J2MaxRotation = 90f;

    [Header("Joint 3 - Rotational Control (Z-axis)")]
    [Tooltip("Minimum rotation in degrees for Joint 3 around the Z-axis.")]
    [SerializeField] private float J3MinRotation = -90f;
    [Tooltip("Maximum rotation in degrees for Joint 3 around the Z-axis.")]
    [SerializeField] private float J3MaxRotation = 90f;

    [Header("MQTT Configuration")]
    [Tooltip("Time interval in seconds between each MQTT message transmission.")]
    [SerializeField] private float dataSendingInterval = 0.5f;

    private MQTT_Client_Apiro mqttClient;
    private bool endEffectorEnabled = false;
    private float lastDataSentTime = 0f;
    private bool jointValuesChanged = false;

    // Joint positions and rotation values
    private float J1ZPos;
    private float J2ZRot = 0.0f;
    private float J3ZRot = 0.0f;

    // Flags to track button hold states
    private bool isJ1PlusHeld = false;
    private bool isJ1MinusHeld = false;
    private bool isJ2PlusHeld = false;
    private bool isJ2MinusHeld = false;
    private bool isJ3PlusHeld = false;
    private bool isJ3MinusHeld = false;

    private void Start()
    {
        mqttClient = GetComponent<MQTT_Client_Apiro>();

        // Initialize J1's position to the starting position
        J1ZPos = J1StartPosition;
        J1.localPosition = new Vector3(J1.localPosition.x, J1.localPosition.y, J1ZPos);

        // Add Event Triggers to each button for PointerDown and PointerUp
        AddEventTrigger(J1_Plus, () => isJ1PlusHeld = true, EventTriggerType.PointerDown);
        AddEventTrigger(J1_Plus, () => isJ1PlusHeld = false, EventTriggerType.PointerUp);

        AddEventTrigger(J1_Minus, () => isJ1MinusHeld = true, EventTriggerType.PointerDown);
        AddEventTrigger(J1_Minus, () => isJ1MinusHeld = false, EventTriggerType.PointerUp);

        AddEventTrigger(J2_Plus, () => isJ2PlusHeld = true, EventTriggerType.PointerDown);
        AddEventTrigger(J2_Plus, () => isJ2PlusHeld = false, EventTriggerType.PointerUp);

        AddEventTrigger(J2_Minus, () => isJ2MinusHeld = true, EventTriggerType.PointerDown);
        AddEventTrigger(J2_Minus, () => isJ2MinusHeld = false, EventTriggerType.PointerUp);

        AddEventTrigger(J3_Plus, () => isJ3PlusHeld = true, EventTriggerType.PointerDown);
        AddEventTrigger(J3_Plus, () => isJ3PlusHeld = false, EventTriggerType.PointerUp);

        AddEventTrigger(J3_Minus, () => isJ3MinusHeld = true, EventTriggerType.PointerDown);
        AddEventTrigger(J3_Minus, () => isJ3MinusHeld = false, EventTriggerType.PointerUp);
    }

    private void Update()
    {
        AdjustJointAngles();
        ProcessMovement();

        if (jointValuesChanged && Time.time - lastDataSentTime >= dataSendingInterval)
        {
            SendJointValues();
            lastDataSentTime = Time.time;
            jointValuesChanged = false;
        }
    }

    private void AdjustJointAngles()
    {
        float delta = Time.deltaTime;

        if (isJ1PlusHeld)
        {
            J1ZPos = Mathf.Clamp(J1ZPos + J1Speed * delta, J1MinPosition, J1MaxPosition);
            jointValuesChanged = true;
        }
        if (isJ1MinusHeld)
        {
            J1ZPos = Mathf.Clamp(J1ZPos - J1Speed * delta, J1MinPosition, J1MaxPosition);
            jointValuesChanged = true;
        }

        if (isJ2PlusHeld)
        {
            J2ZRot = Mathf.Clamp(J2ZRot + universalRotationSpeed * delta, J2MinRotation, J2MaxRotation);
            jointValuesChanged = true;
        }
        if (isJ2MinusHeld)
        {
            J2ZRot = Mathf.Clamp(J2ZRot - universalRotationSpeed * delta, J2MinRotation, J2MaxRotation);
            jointValuesChanged = true;
        }

        if (isJ3PlusHeld)
        {
            J3ZRot = Mathf.Clamp(J3ZRot + universalRotationSpeed * delta, J3MinRotation, J3MaxRotation);
            jointValuesChanged = true;
        }
        if (isJ3MinusHeld)
        {
            J3ZRot = Mathf.Clamp(J3ZRot - universalRotationSpeed * delta, J3MinRotation, J3MaxRotation);
            jointValuesChanged = true;
        }
    }

    private void ProcessMovement()
    {
        // Set the Z-axis position of J1
        J1.localPosition = new Vector3(J1.localPosition.x, J1.localPosition.y, J1ZPos);

        // Set the Z-axis rotations for J2 and J3
        J2.localEulerAngles = new Vector3(J2.localEulerAngles.x, J2.localEulerAngles.y, J2ZRot);
        J3.localEulerAngles = new Vector3(J3.localEulerAngles.x, J3.localEulerAngles.y, J3ZRot);
    }

    private void SendJointValues()
    {
        if (mqttClient != null && mqttClient.IsConnected())
        {
            var data = new JointData
            {
                j1 = J1ZPos.ToString("F3"),
                j2 = J2ZRot.ToString("F3"),
                j3 = J3ZRot.ToString("F3"),
                status = "True",
                endEffector = new EndEffector { type = "suck", enable = endEffectorEnabled.ToString() }
            };

            var robotMessage = new JointRobotMessage
            {
                nodeID = "apiro-01",
                moveType = "joint",
                data = data,
                unixtime = GetUnixTimestamp()
            };

            mqttClient.PublishJointValues(new apirorobotmessage.RobotMessageApiro
            {
                nodeID = robotMessage.nodeID,
                moveType = robotMessage.moveType,
                state = 1
            });

            Debug.Log("Joint values sent via MQTT.");
        }
        else
        {
            Debug.LogError("MQTT client is not connected. Cannot send joint values.");
        }
    }

    private long GetUnixTimestamp() => new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

    private void AddEventTrigger(Button button, Action action, EventTriggerType triggerType)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = triggerType };
        entry.callback.AddListener((eventData) => action());
        trigger.triggers.Add(entry);
    }

    [System.Serializable]
    private class JointData { public string j1, j2, j3, status; public EndEffector endEffector; }
    [System.Serializable]
    private class JointRobotMessage { public string nodeID; public string moveType; public JointData data; public long unixtime; }
    [System.Serializable]
    public class EndEffector { public string type; public string enable; }
}