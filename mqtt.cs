using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Server;
using System.Net.NetworkInformation;
using System.Text;


namespace KNetReceiver
{
    class Helper
    {
        private static string MQTTIP = "192.168.1.22";

        private static IMqttClient MqttClientSub;


        public static async void MqttConnectClient()
        {
            String Payload;

            MqttFactory mqttFactorySub = new MqttFactory();
            MqttClientSubscribeOptions mqttSubscribeOptions = mqttFactorySub.CreateSubscribeOptionsBuilder().WithTopicFilter(f => { f.WithTopic("KNet/#"); }).Build();
            MqttClientSub = mqttFactorySub.CreateMqttClient();

            MqttClientSub.ApplicationMessageReceivedAsync += e =>
            {
                if (e.ApplicationMessage.PayloadSegment.Count != 0)
                    Payload = Encoding.Default.GetString(e.ApplicationMessage.PayloadSegment.Array);
                else
                    Payload = "";

                Program.MessageReceived(e.ApplicationMessage.Topic, Payload);
                return Task.CompletedTask;
            };

            MqttClientOptions mqttClientOptionsSub = new MqttClientOptionsBuilder().WithTcpServer(MQTTIP).Build();
            ManagedMqttClientOptions managedMqttClientOptions = new ManagedMqttClientOptionsBuilder().WithClientOptions(mqttClientOptionsSub).Build();

            await MqttClientSub.ConnectAsync(mqttClientOptionsSub, CancellationToken.None);

            SubscriptMQTTMessage(mqttSubscribeOptions);
        }


        public static Task SubscriptMQTTMessage(MqttClientSubscribeOptions mqttSubscribeOptions)
        {
            MqttClientSub.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            Console.WriteLine("MQTT client subscribed to topic.");
            return Task.CompletedTask;
        }

    }
}