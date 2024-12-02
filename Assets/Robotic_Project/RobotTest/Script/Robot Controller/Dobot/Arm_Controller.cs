using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[System.Serializable]
public class EndEffectorDobot
{
    public string type;
    public string enable;
}

[System.Serializable]
public class DataDobot
{
    public string j1, j2, j3, j4, status;
    public EndEffectorDobot endEffector;
}

[System.Serializable]
public class RobotMessage
{
    public string nodeID, moveType;
    public DataDobot data;
    public long unixtime;
}

public class Arm_Controller : MonoBehaviour
{
    [Header("Joint")]
    public Transform J1, J2, J3, J4;

    [Header("Turn Rate")]
    public int J1_TurnRate = 1, J2_TurnRate = 1, J3_TurnRate = 1, J4_TurnRate = 1;

    [Header("Rotation Limits")]
    public int J1ZRotMin = -135, J1ZRotMax = 135;  // J1 rotates on Z-axis
    public int J2XRotMin = -8, J2XRotMax = 80;     // J2 rotates on X-axis
    public int J3XRotMin = -7, J3XRotMax = 61;     // J3 rotates on X-axis
    public int J4ZRotMin = -145, J4ZRotMax = 145;  // J4 rotates on Z-axis

    [Header("Buttons")]
    public Button J1_PlusButton, J1_MinusButton;
    public Button J2_PlusButton, J2_MinusButton;
    public Button J3_PlusButton, J3_MinusButton;
    public Button J4_PlusButton, J4_MinusButton;
    public Button resetButton;

    [Header("Reset Duration")]
    public float resetDuration = 2.0f;

    private MQTT_Client mqttClient;
    private int lastJ1ZRot, lastJ2XRot, lastJ3XRot, lastJ4ZRot;  // Adjusted rotations
    public int J1ZRot, J2XRot, J3XRot, J4ZRot;
    private bool isButtonHeld, endEffectorEnabled;

    private void Start()
    {
        mqttClient = GetComponent<MQTT_Client>();

        if (resetButton != null) resetButton.onClick.AddListener(StartReset);

        // Assign button events for rotation adjustments
        AssignButtonEvents(J1_PlusButton, () => AdjustJointRotation(ref J1ZRot, J1_TurnRate, J1ZRotMin, J1ZRotMax));
        AssignButtonEvents(J1_MinusButton, () => AdjustJointRotation(ref J1ZRot, -J1_TurnRate, J1ZRotMin, J1ZRotMax));
        AssignButtonEvents(J2_PlusButton, () => AdjustJointRotation(ref J2XRot, J2_TurnRate, J2XRotMin, J2XRotMax));
        AssignButtonEvents(J2_MinusButton, () => AdjustJointRotation(ref J2XRot, -J2_TurnRate, J2XRotMin, J2XRotMax));
        AssignButtonEvents(J3_PlusButton, () => AdjustJointRotation(ref J3XRot, J3_TurnRate, J3XRotMin, J3XRotMax));
        AssignButtonEvents(J3_MinusButton, () => AdjustJointRotation(ref J3XRot, -J3_TurnRate, J3XRotMin, J3XRotMax));
        AssignButtonEvents(J4_PlusButton, () => AdjustJointRotation(ref J4ZRot, J4_TurnRate, J4ZRotMin, J4ZRotMax));
        AssignButtonEvents(J4_MinusButton, () => AdjustJointRotation(ref J4ZRot, -J4_TurnRate, J4ZRotMin, J4ZRotMax));
    }

    public void UpdateJointRotations()
    {
        // Rotate J1 and J4 on the Z-axis
        J1.localEulerAngles = new Vector3(J1.localEulerAngles.x, J1.localEulerAngles.y, J1ZRot);
        J4.localEulerAngles = new Vector3(J4.localEulerAngles.x, J4.localEulerAngles.y, J4ZRot);

        // Rotate J2 and J3 on the X-axis
        J2.localEulerAngles = new Vector3(J2XRot, J2.localEulerAngles.y, J2.localEulerAngles.z);
        J3.localEulerAngles = new Vector3(J3XRot, J3.localEulerAngles.y, J3.localEulerAngles.z);
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

        if (lastJ1ZRot == J1ZRot && lastJ2XRot == J2XRot && lastJ3XRot == J3XRot && lastJ4ZRot == J4ZRot)
        {
            Debug.Log("Duplicate joint values detected. Skipping message.");
            return;
        }

        var endEffector = new EndEffectorDobot
        {
            type = "suck",
            enable = (!endEffectorEnabled).ToString()
        };

        var data = new DataDobot
        {
            j1 = (-J1ZRot).ToString(),
            j2 = J2XRot.ToString(),
            j3 = J3XRot.ToString(),
            j4 = J4ZRot.ToString(),
            status = "True",
            endEffector = endEffector
        };

        var robotMessage = new RobotMessage
        {
            nodeID = "dobot-l-01",
            moveType = "joint",
            data = data,
            unixtime = GetUnixTimestamp()
        };

        mqttClient.PublishJointValues(robotMessage);
        UpdateLastJointValues();
    }

    private long GetUnixTimestamp() => (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;

    private void UpdateLastJointValues()
    {
        lastJ1ZRot = J1ZRot;
        lastJ2XRot = J2XRot;
        lastJ3XRot = J3XRot;
        lastJ4ZRot = J4ZRot;
    }

    private void StartReset() => StartCoroutine(ResetJoints());

    private IEnumerator ResetJoints()
    {
        float elapsedTime = 0.0f;
        int[] initialRotations = { J1ZRot, J2XRot, J3XRot, J4ZRot };

        while (elapsedTime < resetDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / resetDuration;

            J1ZRot = (int)Mathf.Lerp(initialRotations[0], 0, t);
            J2XRot = (int)Mathf.Lerp(initialRotations[1], 0, t);
            J3XRot = (int)Mathf.Lerp(initialRotations[2], 0, t);
            J4ZRot = (int)Mathf.Lerp(initialRotations[3], 0, t);
            UpdateJointRotations();
            yield return null;
        }

        J1ZRot = J2XRot = J3XRot = J4ZRot = 0;
        UpdateJointRotations();
    }

    public void SetEndEffectorState(bool isEnabled) => endEffectorEnabled = isEnabled;
}
