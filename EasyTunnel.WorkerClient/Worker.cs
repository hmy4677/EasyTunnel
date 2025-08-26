using EasyTunnel.Shared.Extensions;
using EasyTunnel.WorkerClient.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Sockets;

namespace EasyTunnel.WorkerClient;

public class Worker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ClientConfig _clientConfig = new();
    private HubConnection connection;

    public Worker(IConfiguration configuration)
    {
        _configuration = configuration;
        _configuration.GetSection("ClientConfig").Bind(_clientConfig);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await connection.StopAsync();
    }

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        connection = new HubConnectionBuilder()
            .WithUrl($"http://{_clientConfig.ServerIP}:{_clientConfig.ServerPort}/server")
            .WithAutomaticReconnect(new MyRetryPolicy())
            .WithKeepAliveInterval(TimeSpan.FromSeconds(60))
            .WithServerTimeout(TimeSpan.FromSeconds(60))
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Information);
                logging.AddConsole();
            }).Build();

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
            _ = connection.InvokeAsync("LoginAsync", _clientConfig.Token, _clientConfig.UserName, _clientConfig.PortMaps);
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
            await server.ConnectAsync(_clientConfig.ServerIP, serverPort);

            var trans = new TransService(local, server, stoppingToken);
            await trans.BridgeConnectAsync();
        }
    }
}