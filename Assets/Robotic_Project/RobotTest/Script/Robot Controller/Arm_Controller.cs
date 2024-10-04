using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[System.Serializable]
public class EndEffector
{
    public string type;
    public string enable;
}

[System.Serializable]
public class Data
{
    public string j1, j2, j3, j4, status;
    public EndEffector endEffector;
}

[System.Serializable]
public class RobotMessage
{
    public string nodeID, moveType;
    public Data data;
    public long unixtime;
}

public class Arm_Controller : MonoBehaviour
{
    [Header("Joint")]
    public Transform J1, J2, J3, J4;

    [Header("Turn Rate")]
    public int J1_TurnRate = 1, J2_TurnRate = 1, J3_TurnRate = 1, J4_TurnRate = 1;

    [Header("Rotation Limits")]
    public int J1YRotMin = -135, J1YRotMax = 135;
    public int J2YRotMin = -8, J2YRotMax = 80;
    public int J3YRotMin = -7, J3YRotMax = 61;
    public int J4YRotMin = -145, J4YRotMax = 145;

    [Header("Buttons")]
    public Button J1_PlusButton, J1_MinusButton;
    public Button J2_PlusButton, J2_MinusButton;
    public Button J3_PlusButton, J3_MinusButton;
    public Button J4_PlusButton, J4_MinusButton;
    public Button resetButton;

    [Header("Reset Duration")]
    public float resetDuration = 2.0f;

    private MQTT_Client mqttClient;
    private int lastJ1YRot, lastJ2YRot, lastJ3YRot, lastJ4YRot;
    public int J1YRot, J2YRot, J3YRot, J4YRot;
    private bool isButtonHeld, endEffectorEnabled;

    private void Start()
    {
        mqttClient = GetComponent<MQTT_Client>();

        if (resetButton != null) resetButton.onClick.AddListener(StartReset);
        // Assign button events for rotation adjustments
        AssignButtonEvents(J1_PlusButton, () => AdjustJointRotation(ref J1YRot, J1_TurnRate, J1YRotMin, J1YRotMax));
        AssignButtonEvents(J1_MinusButton, () => AdjustJointRotation(ref J1YRot, -J1_TurnRate, J1YRotMin, J1YRotMax));
        AssignButtonEvents(J2_PlusButton, () => AdjustJointRotation(ref J2YRot, J2_TurnRate, J2YRotMin, J2YRotMax));
        AssignButtonEvents(J2_MinusButton, () => AdjustJointRotation(ref J2YRot, -J2_TurnRate, J2YRotMin, J2YRotMax));
        AssignButtonEvents(J3_PlusButton, () => AdjustJointRotation(ref J3YRot, J3_TurnRate, J3YRotMin, J3YRotMax));
        AssignButtonEvents(J3_MinusButton, () => AdjustJointRotation(ref J3YRot, -J3_TurnRate, J3YRotMin, J3YRotMax));
        AssignButtonEvents(J4_PlusButton, () => AdjustJointRotation(ref J4YRot, J4_TurnRate, J4YRotMin, J4YRotMax));
        AssignButtonEvents(J4_MinusButton, () => AdjustJointRotation(ref J4YRot, -J4_TurnRate, J4YRotMin, J4YRotMax));
    }

    public void UpdateJointRotations()
    {
        J1.localEulerAngles = new Vector3(J1.localEulerAngles.x, -J1YRot, J1.localEulerAngles.z);
        J2.localEulerAngles = new Vector3(J2.localEulerAngles.x, J2YRot, J2.localEulerAngles.z);
        J3.localEulerAngles = new Vector3(J3YRot, J3.localEulerAngles.y, J3.localEulerAngles.z);
        J4.localEulerAngles = new Vector3(J4.localEulerAngles.x, J4.localEulerAngles.x, J4YRot);
    }

    private void AssignButtonEvents(Button button, System.Action action)
    {
        var trigger = button.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Add(CreateEventTriggerEntry(EventTriggerType.PointerDown, e => StartButtonHold(action)));
        trigger.triggers.Add(CreateEventTriggerEntry(EventTriggerType.PointerUp, e => StopButtonHold()));
    }

    private EventTrigger.Entry CreateEventTriggerEntry(EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        var entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(callback);
        return entry;
    }

    private void StartButtonHold(System.Action action)
    {
        isButtonHeld = true;
        StartCoroutine(ContinuousAction(action));
    }

    private void StopButtonHold()
    {
        isButtonHeld = false;
        // Send joint values only when the button is released
        SendJointValues();
    }

    private IEnumerator ContinuousAction(System.Action action)
    {
        while (isButtonHeld)
        {
            action();
            yield return new WaitForSeconds(0.01f);
        }
    }

    private void AdjustJointRotation(ref int jointRotation, int turnRate, int minRot, int maxRot)
    {
        jointRotation = Mathf.Clamp(jointRotation + turnRate, minRot, maxRot);
        UpdateJointRotations();
    }

    private void SendJointValues()
    {
        if (!mqttClient.IsConnected())
        {
            Debug.LogError("MQTT client is not connected. Cannot send joint values.");
            return;
        }

        if (lastJ1YRot == J1YRot && lastJ2YRot == J2YRot && lastJ3YRot == J3YRot && lastJ4YRot == J4YRot)
        {
            Debug.Log("Duplicate joint values detected. Skipping message.");
            return;
        }

        var endEffector = new EndEffector { type = "suck", enable = endEffectorEnabled.ToString() };
        var data = new Data { j1 = J1YRot.ToString(), j2 = J2YRot.ToString(), j3 = J3YRot.ToString(), j4 = J4YRot.ToString(), status = "True", endEffector = endEffector };
        var robotMessage = new RobotMessage { nodeID = "dobot-l-01", moveType = "joint", data = data, unixtime = GetUnixTimestamp() };

        mqttClient.PublishJointValues(robotMessage);
        UpdateLastJointValues();
    }

    private long GetUnixTimestamp() => (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;

    private void UpdateLastJointValues()
    {
        lastJ1YRot = J1YRot;
        lastJ2YRot = J2YRot;
        lastJ3YRot = J3YRot;
        lastJ4YRot = J4YRot;
    }

    // Reset function
    private void StartReset() => StartCoroutine(ResetJoints());

    private IEnumerator ResetJoints()
    {
        float elapsedTime = 0.0f;
        int[] initialRotations = { J1YRot, J2YRot, J3YRot, J4YRot };

        while (elapsedTime < resetDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / resetDuration;

            J1YRot = (int)Mathf.Lerp(initialRotations[0], 0, t);
            J2YRot = (int)Mathf.Lerp(initialRotations[1], 0, t);
            J3YRot = (int)Mathf.Lerp(initialRotations[2], 0, t);
            J4YRot = (int)Mathf.Lerp(initialRotations[3], 0, t);
            UpdateJointRotations();
            yield return null;
        }

        // Ensure joints are fully reset to zero
        J1YRot = J2YRot = J3YRot = J4YRot = 0;
        UpdateJointRotations();
    }

    public void SetEndEffectorState(bool isEnabled) => endEffectorEnabled = isEnabled;
}
