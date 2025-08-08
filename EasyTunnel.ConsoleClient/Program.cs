using EasyTunnel.ConsoleClient.Models;
using EasyTunnel.Shared.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Sockets;

CancellationTokenSource token = new();
var config = MyConfiguration.GetObject<ClientConfig>("ClientConfig");

var connection = new HubConnectionBuilder()
    .WithUrl($"http://{config.ServerIP}:{config.ServerPort}/server")
    .WithAutomaticReconnect()
    .WithKeepAliveInterval(TimeSpan.FromSeconds(60))
    .WithServerTimeout(TimeSpan.FromSeconds(60))
    .Build();

connection.On<string>("Message", (message) =>
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine($"From Server:{message}");
});

connection.On<string>("Error", (message) =>
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"From Server:{message}");
    connection.StopAsync().Wait();
});

connection.On<string>("StartLogin", (message) =>
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"From Server:{message}");
    _ = connection.InvokeAsync("LoginAsync", config.Token, config.UserName, config.PortMaps);
});

connection.On<int, int, string>("StartTransaction", (localPort, serverPort, message) =>
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"StartTransaction:{localPort},{serverPort},{message}");
    _ = TcpConnectionAsync(localPort, serverPort);
});

await connection.StartAsync();

async Task TcpConnectionAsync(int localPort, int serverPort)
{
    using var local = new TcpClient();
    await local.ConnectAsync("127.0.0.1", localPort);

    using var server = new TcpClient();
    await server.ConnectAsync(config.ServerIP, serverPort);

    var trans = new TransService(local, server, token.Token);
    await trans.BridgeConnectAsync();
}

while(true)
{
    ConsoleKeyInfo keyInfo = Console.ReadKey(true);

    if((keyInfo.Modifiers & ConsoleModifiers.Control) != 0 && keyInfo.Key == ConsoleKey.C)
    {
        break;
    }
}

AppDomain.CurrentDomain.ProcessExit += (s, e) =>
{
    connection.StopAsync().Wait();
    token.Cancel();
};