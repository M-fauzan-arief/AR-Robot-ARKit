using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System;
using System.Text;

namespace apirorobotmessage  // Update to your project namespace
{
    public class MQTT_Client_Apiro : MonoBehaviour
    {
        private MqttClient client;
        [SerializeField] private string brokerAddress = "103.106.72.182";
        [SerializeField] private int brokerPort = 1887;
        [SerializeField] private string robotID = "Apiro";

        private float lastPublishTime = 0f;
        [SerializeField] private float publishCooldown = 1.0f; // Cooldown time in seconds

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
            return client != null && client.IsConnected;
        }

        public void PublishJointValues(RobotMessageApiro robotMessageApiro)
        {
            if (client.IsConnected)
            {
                if (Time.time - lastPublishTime >= publishCooldown)
                {
                    string topic = "/robot/Apiro/jointValues";
                    string message = JsonUtility.ToJson(robotMessageApiro);
                    client.Publish(topic, Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                    lastPublishTime = Time.time; // Update last published time
                    Debug.Log("Message published: " + message);
                }
                else
                {
                    Debug.LogWarning("Publish skipped due to cooldown.");
                }
            }
            else
            {
                Debug.LogError("MQTT client is not connected. Cannot publish message.");
            }
        }

        // Fixed PublishJointValues method with jsonMessage as a string
        internal void PublishJointValues(string jsonMessage)
        {
            if (client.IsConnected)
            {
                if (Time.time - lastPublishTime >= publishCooldown)
                {
                    string topic = "/robot/Apiro/jointValues";
                    client.Publish(topic, Encoding.UTF8.GetBytes(jsonMessage), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                    lastPublishTime = Time.time; // Update last published time
                    Debug.Log("Message published: " + jsonMessage);
                }
                else
                {
                    Debug.LogWarning("Publish skipped due to cooldown.");
                }
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

    [Serializable]
    public class RobotMessageApiro
    {
        public string nodeID;
        public string moveType;
        public int state;  // Define state field correctly
    }
}
