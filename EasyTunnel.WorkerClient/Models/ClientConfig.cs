namespace EasyTunnel.WorkerClient.Models;

public class ClientConfig
{
    public string ServerIP { get; set; }
    public int ServerPort { get; set; }
    public string Token { get; set; }
    public string UserName { get; set; }
    public string[] PortMaps { get; set; }
}