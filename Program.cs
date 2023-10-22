


namespace KNetReceiver
{
    class Program
    {
        static string SQLSERVER = "192.168.1.24";
        static Database d;

        static void Main(string[] args)
        {
            d = new Database(SQLSERVER);

            Console.WriteLine("KNetReceiver ver 2.0");

//            Helper.MqttConnectClient();

            d.GetMysqlData();

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }


        public static void MessageReceived(string Topic, string Payload)
        {
              // Skip KNet, is always KNet/.
            Console.WriteLine(Topic + " ::: " + Payload);
            string Tabel_name = Topic.Remove(0, 5).Replace('/', '_');  // Remove KNet and replace '/' with'_'
            d.WriteData(Tabel_name, Payload, DateTime.MinValue);
        }
    }
}
