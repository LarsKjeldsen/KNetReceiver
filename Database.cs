using Microsoft.Data.SqlClient;
using MySqlConnector;
using System.Runtime.CompilerServices;
using System.Text;


namespace KNetReceiver
{
    internal class Database
    {
        private string connectionString = "Persist Security Info=False;User ID=KNetReceiver;Password=KNetReceiver1234;Initial Catalog=KNet;Server=192.168.1.4;Encrypt=False;";
        private int prev_day = 0;


        public void WriteData(string Table_name, string value, DateTime time)
        {
            if (time.Day != prev_day) 
            { 
                prev_day = time.Day;
                Console.WriteLine(time);
            }


            SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();
            connection.ChangeDatabase("KNet");

            SqlCommand command = new SqlCommand("", connection);

            StringBuilder SQL = new StringBuilder();

            if (time == DateTime.MinValue)
            {
                SQL.AppendFormat("DECLARE @DateNow datetime = Convert(datetime, FORMAT(GETDATE(), 'yyyy-MM-dd HH:mm'))\n" +
                    "MERGE data as t\n" +
                    "USING(VALUES(@DateNow)) as d(Time)\n" +
                    "ON t.Time = @DateNow\n" +
                    "WHEN MATCHED THEN\n" +
                        "UPDATE\n" +
                        "SET [{0}] = {1}\n" +
                    "WHEN NOT MATCHED THEN\n" +
                        "INSERT(Time, [{0}])\n" +
                        "VALUES(@DateNow, {1}) ;", Table_name, value);

            }
            else
            { // CAST('2023-10-19 15:08:00' as datetime)
                SQL.AppendFormat("DECLARE @DateNow DATETIME = CAST('" + time.ToString("yyyy-MM-dd HH:mm:00") + "' AS DATETIME)\n" +
                    "MERGE data as t\n" +
                    "USING(VALUES(@DateNow)) as d(Time)\n" +
                    "ON t.Time = @DateNow\n" +
                    "WHEN MATCHED THEN\n" +
                        "UPDATE\n" +
                        "SET [{0}] = '{1}'\n" +
                    "WHEN NOT MATCHED THEN\n" +
                        "INSERT(Time, [{0}])\n" +
                        "VALUES(@DateNow, '{1}') ;", Table_name, value);
            }

            try
            {
                command.CommandText = SQL.ToString();
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                if (e.Message.StartsWith("Invalid column name")) // Missing 
                {
                    StringBuilder SQL2 = new StringBuilder();

                    SQL2.AppendFormat("ALTER TABLE Data\r\nADD {0}  VARCHAR(45) NULL", Table_name);

                    using (SqlConnection connection_E = new SqlConnection(connectionString))
                    {
                        using (SqlCommand command_E = new SqlCommand(SQL2.ToString(), connection_E))
                        {
                            command_E.ExecuteNonQuery();
                        }
                        using (SqlCommand command2 = new SqlCommand(SQL.ToString(), connection_E))
                        {
                            command2.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    if (command.Connection.State == System.Data.ConnectionState.Closed)
                        connection.Open();

                    Console.WriteLine(e.Message);
                }

            }
            finally
            {
                connection.Close();
                connection.Dispose();
                command.Dispose();
            }
        }

        class mysqldata
        {
            public DateTime Time;
            public string Value;
            public string TabelName;
        };



        public void GetMysqlData()
        {
            string query = "SELECT * FROM KNet.Data WHERE TIME > \"2023-11-17 00:30:00.000\"";
            string myConnectionString;
            DateTime t;
            bool first = true;

            List<mysqldata> data = new List<mysqldata>();

            myConnectionString = "server=192.168.1.22;uid=kjeldsen;pwd=Minmore9876;database=KNet;Pooling=false";

            using (MySqlConnection MySQLconnection = new MySqlConnection(myConnectionString))
            {
                if (MySQLconnection.State != System.Data.ConnectionState.Open)
                    MySQLconnection.Open();

                using (MySqlCommand MySQLcommand = new MySqlCommand(query, MySQLconnection))
                {
                    MySqlDataReader reader = MySQLcommand.ExecuteReader();


                    while (reader.Read())
                    {
                        t = (DateTime)reader.GetMySqlDateTime(0);

                        for (int i = 1; i < reader.FieldCount; i++)
                        {
                            if (!reader.IsDBNull(i))
                            {
                                string v = reader[i].ToString().Replace(',', '.');

                                mysqldata d = new mysqldata();
                                d.Time = t;
                                d.Value = v;
                                d.TabelName = reader.GetName(i);

                                data.Add(d);
                            }
                        }

                    }
                }
                Console.WriteLine("Records : " + data.Count);
                bool flag = true;
                List<Task> tasks = new List<Task>();    

                foreach (mysqldata da in data)
                {
                 // WriteData(da.TabelName, da.Value, da.Time);
                    tasks.Add(Task.Run(() => { WriteData(da.TabelName, da.Value, da.Time); }));
                }
                do
                {
                    long c = (tasks.Count(t => t.Status == TaskStatus.Running));
                    long w = (tasks.Count(t => t.Status == TaskStatus.WaitingToRun));
                    long r = (tasks.Count(t => t.Status == TaskStatus.RanToCompletion));

                    Console.Write("\r" + c + ", " + w + ", " + r + "     ");
                    Thread.Sleep(1000);
                } while (true);

            }

        }
    }
}

