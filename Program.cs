namespace OdbcConnector
{
    class Program
    {
        static void Main(string[] args)
        {
            OdbcConn OdbcConnector = new OdbcConn();
            OdbcConnector.GetDataInJson("mysql64", "SELECT * FROM CITY");
        }
    }
}

    
