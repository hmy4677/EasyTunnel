namespace EasyTunnel.Shared.Models;

public class ClientInfo
{
    public string ConnectionId { get; set; }
    public string UserName { get; set; }
    public string[] PortMaps { get; set; }
    public DateTime LoginTime { get; set; } = DateTime.Now;
    public CancellationTokenSource CancellationTokenSource { get; init; } = new CancellationTokenSource();
}
