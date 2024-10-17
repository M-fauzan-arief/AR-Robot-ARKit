using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System;
using System.Text;

public class MQTT_Client_Apiro : MonoBehaviour
{
    private MqttClient client;
    [SerializeField] private string brokerAddress = "103.106.72.182";
    [SerializeField] private int brokerPort = 8885;
    [SerializeField] private string robotID = "Apiro";

    private void Start()
    {
        client = new MqttClient(brokerAddress, brokerPort, false, null, null, MqttSslProtocols.None);
        string clientId = Guid.NewGuid().ToString();

        try
        {
            Debug.Log($"Attempting to connect to MQTT broker at {brokerAddress}:{brokerPort}...");
            client.Connect(clientId);

            if (client.IsConnected)
            {
                Debug.Log($"Connected to MQTT broker at {brokerAddress}:{brokerPort}.");
                client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

                string topic = $"/robot/{robotID}/commands";
                client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                Debug.Log($"Subscribed to topic: {topic}");
            }
            else
            {
                Debug.LogError($"Failed to connect to MQTT broker at {brokerAddress}:{brokerPort}.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception occurred while connecting to MQTT broker at {brokerAddress}:{brokerPort}: {ex.Message}");
        }
    }

    private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string receivedMessage = Encoding.UTF8.GetString(e.Message);
        Debug.Log("Received: " + receivedMessage);
    }

    public bool IsConnected()
    {
        return client.IsConnected;
    }

    public void PublishJointValues(RobotMessageApiro robotMessageApiro)
    {
        if (client.IsConnected)
        {
            string topic = "/robot/Apiro/jointValues";
            string message = JsonUtility.ToJson(robotMessageApiro);
            client.Publish(topic, Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }
        else
        {
            Debug.LogError("MQTT client is not connected. Cannot publish message.");
        }
    }

    private void OnDestroy()
    {
        if (client != null && client.IsConnected)
        {
            client.Disconnect();
            Debug.Log("Disconnected from MQTT broker.");
        }
    }
}
