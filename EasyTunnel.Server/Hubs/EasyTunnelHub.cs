using EasyTunnel.Shared.Extensions;
using EasyTunnel.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace EasyTunnel.Server.Hubs;

public class EasyTunnelHub : Hub
{
    private readonly IConfiguration _configuration;
    private readonly List<string> _tokens = [];

    private static ConcurrentDictionary<string, TcpListener> ListenerCollection = new();
    public static ConcurrentDictionary<string, ClientInfo> ClientCollection = new();

    public EasyTunnelHub(IConfiguration configuration)
    {
        _configuration = configuration;
        _configuration.GetSection("Tokens").Bind(_tokens);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Client(Context.ConnectionId).SendAsync("StartLogin", "Connect Success, Start Login");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine("Client Close" + exception?.Message);

        var client = ClientCollection[Context.ConnectionId];
        client.CancellationTokenSource.Cancel();
        client.CancellationTokenSource.Dispose();
        ClientCollection.TryRemove(Context.ConnectionId, out _);

        foreach(var item in client.PortMaps)
        {
            ListenerCollection[item].Stop();
            ListenerCollection.TryRemove(item, out _);
        }
    }

    public async Task LoginAsync(string token, string userName, string[] portMaps)
    {
        var proxy = Clients.Client(Context.ConnectionId);
        if(_tokens is { Count: > 0 } && !_tokens.Contains(token))
        {
            _ = proxy.SendAsync("Error", "Login Error");
            return;
        }

        var client = new ClientInfo
        {
            ConnectionId = Context.ConnectionId,
            PortMaps = portMaps,
            UserName = userName
        };

        ClientCollection.TryAdd(Context.ConnectionId, client);

        _ = proxy.SendAsync("Message", "Login Success");

        _ = ListenAsync(Context.ConnectionId);
    }

    public async Task ListenAsync(string connectionId)
    {
        var client = ClientCollection[Context.ConnectionId];
        foreach(var item in client.PortMaps)
        {
            _ = TcpClientServerAsync(connectionId, item);
        }
    }

    private async Task TcpClientServerAsync(string connectionId, string portMap)
    {
        var array = portMap.Split(',');
        var clientPort = int.Parse(array[0]);
        var serverPort = int.Parse(array[1]);

        var client = ClientCollection[Context.ConnectionId];
        var token = client.CancellationTokenSource.Token;

        var proxy = Clients.Client(connectionId);
        var listener = new TcpListener(IPAddress.Any, serverPort);
        listener.Start();
        ListenerCollection.TryAdd(portMap, listener);

        while(!token.IsCancellationRequested)
        {
            using var remote = await listener.AcceptTcpClientAsync(token);
            await proxy.SendAsync("StartTransaction", clientPort, serverPort, "Start Transaction");

            using var server = await listener.AcceptTcpClientAsync(token);

            var trans = new TransService(remote, server, token);
            await trans.BridgeConnectAsync();
            _ = proxy.SendAsync("Message", "Stop Tansaction");
        }

        ListenerCollection[portMap].Stop();
        ListenerCollection[portMap].Dispose();
        ListenerCollection.TryRemove(portMap, out _);
    }
}