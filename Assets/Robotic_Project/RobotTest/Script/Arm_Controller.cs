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
    public string j1;
    public string j2;
    public string j3;
    public string j4;
    public string status;
    public EndEffector endEffector;
}

[System.Serializable]
public class RobotMessage
{
    public string nodeID;
    public string moveType;
    public Data data;
    public long unixtime;
}

public class Arm_Controller : MonoBehaviour
{
    private int lastJ1YRot = 0;
    private int lastJ2YRot = 0;
    private int lastJ3YRot = 0;
    private int lastJ4YRot = 0;

    [Header("Joint")]
    public Transform J1;
    public Transform J2;
    public Transform J3;
    public Transform J4;

    [Header("Turn Rate")]
    public int J1_TurnRate = 1;
    public int J2_TurnRate = 1;
    public int J3_TurnRate = 1;
    public int J4_TurnRate = 1;

    private int J1YRot = 0;
    private int J1YRotMin = -135;
    private int J1YRotMax = 135;

    private int J2YRot = 0;
    private int J2YRotMin = -8;
    private int J2YRotMax = 80;

    private int J3YRot = 0;
    private int J3YRotMin = -7;
    private int J3YRotMax = 61;

    private int J4YRot = 0;
    private int J4YRotMin = -145;
    private int J4YRotMax = 145;

    [Header("Buttons")]
    public Button J1_PlusButton;
    public Button J1_MinusButton;
    public Button J2_PlusButton;
    public Button J2_MinusButton;
    public Button J3_PlusButton;
    public Button J3_MinusButton;
    public Button J4_PlusButton;
    public Button J4_MinusButton;
    public Button resetButton;

    [Header("Reset Duration")]
    public float resetDuration = 2.0f;

    [Header("Data Sending Interval")]
    public float dataSendingInterval = 0.5f; // Interval time between data sending
    private float lastDataSentTime; // Last time data was sent

    private MQTT_Client mqttClient;
    private bool endEffectorEnabled;

    private void Start()
    {
        mqttClient = GetComponent<MQTT_Client>();

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(StartReset);
        }

        // Assign button hold events
        AssignButtonEvents(J1_PlusButton, () => AdjustJointRotation(ref J1YRot, J1_TurnRate, J1YRotMin, J1YRotMax));
        AssignButtonEvents(J1_MinusButton, () => AdjustJointRotation(ref J1YRot, -J1_TurnRate, J1YRotMin, J1YRotMax));
        AssignButtonEvents(J2_PlusButton, () => AdjustJointRotation(ref J2YRot, J2_TurnRate, J2YRotMin, J2YRotMax));
        AssignButtonEvents(J2_MinusButton, () => AdjustJointRotation(ref J2YRot, -J2_TurnRate, J2YRotMin, J2YRotMax));
        AssignButtonEvents(J3_PlusButton, () => AdjustJointRotation(ref J3YRot, J3_TurnRate, J3YRotMin, J3YRotMax));
        AssignButtonEvents(J3_MinusButton, () => AdjustJointRotation(ref J3YRot, -J3_TurnRate, J3YRotMin, J3YRotMax));
        AssignButtonEvents(J4_PlusButton, () => AdjustJointRotation(ref J4YRot, J4_TurnRate, J4YRotMin, J4YRotMax));
        AssignButtonEvents(J4_MinusButton, () => AdjustJointRotation(ref J4YRot, -J4_TurnRate, J4YRotMin, J4YRotMax));

        lastDataSentTime = Time.time; // Initialize lastDataSentTime
    }

    private void Update()
    {
        // Check if it's time to send data based on dataSendingInterval
        if (Time.time - lastDataSentTime >= dataSendingInterval)
        {
            SendJointValues(); // Send joint values
            lastDataSentTime = Time.time; // Update lastDataSentTime
        }
    }

    void AssignButtonEvents(Button button, System.Action action)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((e) => StartCoroutine(ContinuousAction(action)));
        trigger.triggers.Add(pointerDown);

        var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((e) => StopAllCoroutines());
        trigger.triggers.Add(pointerUp);
    }

    IEnumerator ContinuousAction(System.Action action)
    {
        while (true)
        {
            action();
            yield return new WaitForSeconds(0.01f); // Adjust the delay as needed
        }
    }

    void AdjustJointRotation(ref int jointRotation, int turnRate, int minRot, int maxRot)
    {
        jointRotation += turnRate;
        jointRotation = Mathf.Clamp(jointRotation, minRot, maxRot);
        UpdateJointRotations();
    }

    void UpdateJointRotations()
    {
        J1.localEulerAngles = new Vector3(J1.localEulerAngles.x, -J1YRot, J1.localEulerAngles.z);
        J2.localEulerAngles = new Vector3(J2.localEulerAngles.x, J2YRot, J2.localEulerAngles.z);
        J3.localEulerAngles = new Vector3(J3YRot, J3.localEulerAngles.y, J3.localEulerAngles.z);
        J4.localEulerAngles = new Vector3(J4.localEulerAngles.x, J4.localEulerAngles.x, J4YRot);
    }

    void SendJointValues()
    {
        if (!mqttClient.IsConnected())
        {
            Debug.LogError("MQTT client is not connected. Cannot send joint values.");
            return;
        }

        if (J1YRot == lastJ1YRot && J2YRot == lastJ2YRot && J3YRot == lastJ3YRot && J4YRot == lastJ4YRot)
        {
            Debug.Log("Duplicate joint values detected. Skipping message.");
            return;
        }

        var endEffector = new EndEffector
        {
            type = "suck",
            enable = endEffectorEnabled.ToString()
        };

        var data = new Data
        {
            j1 = J1YRot.ToString(),
            j2 = J2YRot.ToString(),
            j3 = J3YRot.ToString(),
            j4 = J4YRot.ToString(),
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

        // Update last joint values
        lastJ1YRot = J1YRot;
        lastJ2YRot = J2YRot;
        lastJ3YRot = J3YRot;
        lastJ4YRot = J4YRot;
    }

    private long GetUnixTimestamp()
    {
        System.DateTime unixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        return (long)(System.DateTime.UtcNow - unixEpoch).TotalSeconds;
    }

    public void ResetSlider()
    {
        J1YRot = 0;
        J2YRot = 0;
        J3YRot = 0;
        J4YRot = 0;
        UpdateJointRotations();
    }

    private IEnumerator ResetJoints()
    {
        float elapsedTime = 0.0f;

        float initialJ1Rotation = J1YRot;
        float initialJ2Rotation = J2YRot;
        float initialJ3Rotation = J3YRot;
        float initialJ4Rotation = J4YRot;

        while (elapsedTime < resetDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / resetDuration;

            J1YRot = (int)Mathf.Lerp(initialJ1Rotation, 0.0f, t);
            J2YRot = (int)Mathf.Lerp(initialJ2Rotation, 0.0f, t);
            J3YRot = (int)Mathf.Lerp(initialJ3Rotation, 0.0f, t);
            J4YRot = (int)Mathf.Lerp(initialJ4Rotation, 0.0f, t);

            UpdateJointRotations();

            yield return null;
        }

        J1YRot = 0;
        J2YRot = 0;
        J3YRot = 0;
        J4YRot = 0;

        UpdateJointRotations();
    }

    private void StartReset()
    {
        StartCoroutine(ResetJoints());
        ResetSlider();
    }

    public void SetEndEffectorState(bool isEnabled)
    {
        endEffectorEnabled = isEnabled;
    }
}

