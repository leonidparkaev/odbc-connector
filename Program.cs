using OdbcConnector;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Configuration;
using Microsoft.Extensions.Logging;
using WinLogger;

string? DSN;
int Port;
DSN = ConfigurationManager.AppSettings.Get("DSN");
Port = Int32.Parse(ConfigurationManager.AppSettings.Get("Port"));

IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, Port);
using Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
OdbcConn OdbcConnector = new OdbcConn();

try
{
    tcpSocket.Bind(ipPoint);
    tcpSocket.Listen();
    Console.WriteLine("Сервер запущен\n");
    WinLogger.cWinLogger.Logger.LogInformation("Сервер запущен\n");

    while (true)
    {
        using var tcpClient = await tcpSocket.AcceptAsync();

        List<byte> response = [];
        byte[] buffer = new byte[1024];
        int bytes = 0;

        bytes = await tcpClient.ReceiveAsync(buffer);
        response.AddRange(buffer.Take(bytes));

        var responseString = Encoding.UTF8.GetString(response.ToArray());
        Console.WriteLine($"От клиента {tcpClient.RemoteEndPoint} получены данные:\n{responseString}\n");
        WinLogger.cWinLogger.Logger.LogInformation($"От клиента {tcpClient.RemoteEndPoint} получены данные:\n{responseString}");

        string Json = OdbcConnector.GetDataInJsonString(DSN, responseString);
        byte[] data = Encoding.UTF8.GetBytes(Json);
        await tcpClient.SendAsync(data);
        Console.WriteLine($"Клиенту {tcpClient.RemoteEndPoint} отправлены данные.\n");
        WinLogger.cWinLogger.Logger.LogInformation($"Клиенту {tcpClient.RemoteEndPoint} отправлены данные");
    }
}
catch (Exception ex)
{
    byte[] error = Encoding.UTF8.GetBytes(ex.Message);
    using var tcpClient = await tcpSocket.AcceptAsync();
    await tcpClient.SendAsync(error);
    Console.WriteLine($"Ошибка отправки данных клиенту {tcpClient.RemoteEndPoint}:\n{ex.Message}\n");
    WinLogger.cWinLogger.Logger.LogError($"Ошибка отправки данных клиенту {tcpClient.RemoteEndPoint}:\n{ex.Message}");
}


