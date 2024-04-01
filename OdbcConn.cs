using System.Data.Odbc;
using Newtonsoft.Json.Linq;

namespace OdbcConnector
{

    public class OdbcConn
    {
        public string GetFullConnectionString(string Driver, string Database, string User, string PWD)
        {
            string ConnectionParams = "DRIVER={" + Driver + "};" +
                                      "SERVER=localhost;" +
                                      $"DATABASE={Database};";

            if (User != null)
            {
                ConnectionParams += $"UID={User};";
            }

            if (PWD != null)
            {
                ConnectionParams += $"PASSWORD={PWD};";
            }
            ConnectionParams += "OPTION=3";
            return ConnectionParams;
        }

        public string SendSqlQuery(string DSN, string QueryString)
        {
            OdbcConnection DbConnection = new OdbcConnection("DSN=" + DSN);

            try
            {
                DbConnection.Open();
            }
            catch (OdbcException ex)
            {
                Console.WriteLine("Подключение к базе данных не удалось:");
                Console.WriteLine(ex.Message);
            }

            OdbcCommand DbCommand = DbConnection.CreateCommand();
            DbCommand.CommandText = QueryString;

            try
            {
                OdbcDataReader DbReader = DbCommand.ExecuteReader();

                int fieldCount = DbReader.FieldCount;
                string[] colNames = new string[fieldCount];
                for (int i = 0; i < fieldCount; i++)
                {
                    colNames[i] = DbReader.GetName(i);
                }


                int j = 0;
                int rowCount = DbReader.RecordsAffected;
                string[,] colData = new string[fieldCount, rowCount];

                while (DbReader.Read())
                {
                    for (int i = 0; i < fieldCount; i++)
                    {
                        colData[i, j] += DbReader.GetString(i); 
                    }

                    if (j < rowCount)
                    {
                        j = j + 1;
                    }     
                }

                string jsonString = "{";
                for (int i = 0; i < fieldCount; i++)
                {
                    jsonString += '"' + colNames[i] + '"' + ": [";
                    for (j = 0; j < rowCount; j++)
                    {
                        jsonString += '"' + colData[i, j] + '"';

                        if ((j + 1) < rowCount)
                        {
                            jsonString += ", ";
                        }
                    }
                    jsonString += "]";

                    if ((i + 1) < fieldCount)
                    {
                        jsonString += ", ";
                    }
                }
                jsonString += "}";              
                
                DbReader.Close();
                DbCommand.Dispose();
                DbConnection.Close();

                return jsonString;
            }
            catch (OdbcException ex)
            {
                Console.WriteLine("Отправка команды " + QueryString + " не удалась:");
                return ex.Message;
            }
        }

        public JObject GetDataInJson(string DSN, string QueryString)
        {
            string jsonString = SendSqlQuery(DSN, QueryString);
            JObject jsonObject = JObject.Parse(jsonString);

            return jsonObject;
        }
    }
}
