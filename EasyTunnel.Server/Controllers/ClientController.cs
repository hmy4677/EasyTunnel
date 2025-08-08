using EasyTunnel.Server.Hubs;
using Microsoft.AspNetCore.Mvc;

namespace EasyTunnel.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientController : ControllerBase
{
    [HttpGet]
    public IActionResult GetClientList()
    {
        var clients = EasyTunnelHub.ClientCollection
            .Select(p => new
            {
                p.Value.ConnectionId,
                p.Value.UserName,
                LoginTime = p.Value.LoginTime.ToString("yyyy-MM-dd HH:mm:ss"),
                PortMaps = string.Join(", ", p.Value.PortMaps.Select(x =>
                {
                    var array = x.Split(',');
                    return $"{array[0]}→{array[1]}";
                }))
            }).ToList();

        var html = $@"
    <html>
    <head>
        <meta charset=""UTF-8"">
        <style>
            table {{
                width: 100%;
                border-collapse: collapse;
                font-family: Arial, sans-serif;
                box-shadow: 0 0 20px rgba(0,0,0,0.15);
            }}
            th, td {{
                padding: 12px 15px;
                text-align: left;
                border-bottom: 1px solid #dddddd;
            }}
            th {{
                background-color: #009879;
                color: white;
                position: sticky;
                top: 0;
            }}
            tr:nth-child(even) {{
                background-color: #f3f3f3;
            }}
            tr:hover {{
                background-color: #f1f1f1;
            }}
            .status {{
                display: inline-block;
                padding: 5px 10px;
                border-radius: 20px;
                font-size: 12px;
                font-weight: bold;
            }}
            .online {{
                background-color: #d4edda;
                color: #155724;
            }}
        </style>
    </head>
    <body>
        <table>
            <thead>
                <tr>
                    <th>Connection ID</th>
                    <th>User Name</th>
                    <th>Port Map</th>
                    <th>Login Time</th>
                </tr>
            </thead>
            <tbody>
                {string.Join("", clients.Select(c => $@"
                <tr>
                    <td>{c.ConnectionId}</td>
                    <td>{c.UserName}</td>
                    <td>{c.PortMaps}</td>
                    <td>{c.LoginTime}</td>
                </tr>"))}
            </tbody>
        </table>
    </body>
    </html>";

        return Content(html, "text/html");
    }
}