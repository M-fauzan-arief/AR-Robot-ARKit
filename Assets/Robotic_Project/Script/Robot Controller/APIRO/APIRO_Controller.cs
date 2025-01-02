using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using apirorobotmessage;
using System.Text;

public class APIROController : MonoBehaviour
{
    [SerializeField] private Button J1_Plus, J1_Minus, J2_Plus, J2_Minus, J3_Plus, J3_Minus;
    [SerializeField] private Transform J1, J2, J3;

    [Header("Universal Settings")]
    [SerializeField] [Range(0.1f, 100f)] private float universalRotationSpeed = 20f;

    [Header("Joint 1 - Positional Control (Z-axis)")]
    [SerializeField] [Min(0.0f)] private float J1Speed = 1f;
    [SerializeField] private float J1StartPosition = 0f;
    [SerializeField] private float J1MinPosition = -1f;
    [SerializeField] private float J1MaxPosition = 1f;

    [Header("Joint 2 - Rotational Control (Z-axis)")]
    [SerializeField] private float J2MinRotation = -90f;
    [SerializeField] private float J2MaxRotation = 90f;

    [Header("Joint 3 - Rotational Control (Z-axis)")]
    [SerializeField] private float J3MinRotation = -90f;
    [SerializeField] private float J3MaxRotation = 90f;

    private MQTT_Client_Apiro mqttClient;
    private float J1ZPos, J2ZRot = 0.0f, J3ZRot = 0.0f;

    private bool isJ1PlusHeld = false, isJ1MinusHeld = false;
    private bool isJ2PlusHeld = false, isJ2MinusHeld = false;
    private bool isJ3PlusHeld = false, isJ3MinusHeld = false;

    private void Start()
    {
        mqttClient = GetComponent<MQTT_Client_Apiro>();
        J1ZPos = J1StartPosition;
        J1.localPosition = new Vector3(J1.localPosition.x, J1.localPosition.y, J1ZPos);

        AddEventTrigger(J1_Plus, () => OnButtonStateChange(ref isJ1PlusHeld, true), EventTriggerType.PointerDown);
        AddEventTrigger(J1_Plus, () => OnButtonStateChange(ref isJ1PlusHeld, false), EventTriggerType.PointerUp);
        AddEventTrigger(J1_Minus, () => OnButtonStateChange(ref isJ1MinusHeld, true), EventTriggerType.PointerDown);
        AddEventTrigger(J1_Minus, () => OnButtonStateChange(ref isJ1MinusHeld, false), EventTriggerType.PointerUp);

        AddEventTrigger(J2_Plus, () => OnButtonStateChange(ref isJ2PlusHeld, true), EventTriggerType.PointerDown);
        AddEventTrigger(J2_Plus, () => OnButtonStateChange(ref isJ2PlusHeld, false), EventTriggerType.PointerUp);
        AddEventTrigger(J2_Minus, () => OnButtonStateChange(ref isJ2MinusHeld, true), EventTriggerType.PointerDown);
        AddEventTrigger(J2_Minus, () => OnButtonStateChange(ref isJ2MinusHeld, false), EventTriggerType.PointerUp);

        AddEventTrigger(J3_Plus, () => OnButtonStateChange(ref isJ3PlusHeld, true), EventTriggerType.PointerDown);
        AddEventTrigger(J3_Plus, () => OnButtonStateChange(ref isJ3PlusHeld, false), EventTriggerType.PointerUp);
        AddEventTrigger(J3_Minus, () => OnButtonStateChange(ref isJ3MinusHeld, true), EventTriggerType.PointerDown);
        AddEventTrigger(J3_Minus, () => OnButtonStateChange(ref isJ3MinusHeld, false), EventTriggerType.PointerUp);
    }

    private void Update()
    {
        AdjustJointAngles();
        ProcessMovement();
    }

    private void AdjustJointAngles()
    {
        float delta = Time.deltaTime;

        if (isJ1PlusHeld)
            J1ZPos = Mathf.Clamp(J1ZPos + J1Speed * delta, J1MinPosition, J1MaxPosition);
        if (isJ1MinusHeld)
            J1ZPos = Mathf.Clamp(J1ZPos - J1Speed * delta, J1MinPosition, J1MaxPosition);

        if (isJ2PlusHeld)
            J2ZRot = Mathf.Clamp(J2ZRot + universalRotationSpeed * delta, J2MinRotation, J2MaxRotation);
        if (isJ2MinusHeld)
            J2ZRot = Mathf.Clamp(J2ZRot - universalRotationSpeed * delta, J2MinRotation, J2MaxRotation);

        if (isJ3PlusHeld)
            J3ZRot = Mathf.Clamp(J3ZRot + universalRotationSpeed * delta, J3MinRotation, J3MaxRotation);
        if (isJ3MinusHeld)
            J3ZRot = Mathf.Clamp(J3ZRot - universalRotationSpeed * delta, J3MinRotation, J3MaxRotation);
    }

    private void ProcessMovement()
    {
        J1.localPosition = new Vector3(J1.localPosition.x, J1.localPosition.y, J1ZPos);
        J2.localEulerAngles = new Vector3(J2.localEulerAngles.x, J2.localEulerAngles.y, J2ZRot);
        J3.localEulerAngles = new Vector3(J3.localEulerAngles.x, J3.localEulerAngles.y, J3ZRot);
    }

    private void OnButtonStateChange(ref bool buttonState, bool newState)
    {
        if (buttonState != newState) // Detect only when the state changes
        {
            buttonState = newState;
            SendJointMovementStatus();
        }
    }

    private void SendJointMovementStatus()
    {
        if (mqttClient != null && mqttClient.IsConnected())
        {
            string jsonMessage = $@"
            {{
                ""nodeID"": ""apiro-01"",
                ""moveType"": ""joint"",
                ""data"": {{
                    ""j1"": ""{DetermineMovementStatus(isJ1PlusHeld, isJ1MinusHeld)}"",
                    ""j2"": ""{DetermineMovementStatus(isJ2PlusHeld, isJ2MinusHeld)}"",
                    ""j3"": ""{DetermineMovementStatus(isJ3PlusHeld, isJ3MinusHeld)}""
                }},
                ""unixtime"": {GetUnixTimestamp()}
            }}";

            mqttClient.PublishJointValues(jsonMessage);
        }
    }

    private int DetermineMovementStatus(bool isPlusHeld, bool isMinusHeld)
    {
        if (isPlusHeld) return 1;
        if (isMinusHeld) return -1;
        return 0;
    }

    private long GetUnixTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private void AddEventTrigger(Button button, Action action, EventTriggerType triggerType)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = triggerType };
        entry.callback.AddListener((eventData) => action());
        trigger.triggers.Add(entry);
    }
}
