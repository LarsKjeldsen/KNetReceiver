


namespace KNetReceiver
{
    class Program
    {
        static string SQLSERVER = "solen";
        static Database d;

        static void Main(string[] args)
        {
            d = new Database();

            Console.WriteLine("KNetReceiver ver 2.0");


//            Helper.MqttConnectClient();

            d.GetMysqlData();

            Console.WriteLine("Press enter to exit.");

            //while (true)
            //{
            //    if (!Helper.MqttClientSub.IsConnected)
            //        Helper.MqttConnectClient();
            //    Thread.Sleep(1000);
            //}
        }




        public static void MessageReceived(string Topic, string Payload)
        {
              // Skip KNet, is always KNet/.
            Console.WriteLine(DateTime.Now.ToString() + " ::: " + Topic + " ::: " + Payload);
            string Tabel_name = Topic.Remove(0, 5).Replace('/', '_');  // Remove KNet and replace '/' with'_'
            d.WriteData(Tabel_name, Payload, DateTime.MinValue);
        }
    }
}
