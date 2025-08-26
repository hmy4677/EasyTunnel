using Microsoft.AspNetCore.SignalR.Client;

namespace EasyTunnel.WorkerClient;

public class MyRetryPolicy : IRetryPolicy
{
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        return TimeSpan.FromMinutes(5);
    }
}
