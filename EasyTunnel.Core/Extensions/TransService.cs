using System.IO.Pipelines;
using System.Net.Sockets;

namespace EasyTunnel.Shared.Extensions;

public class TransService
{
    private readonly TcpClient _tcp1;
    private readonly TcpClient _tcp2;
    private readonly CancellationToken _token;

    public TransService(TcpClient tcp1, TcpClient tcp2, CancellationToken? token = null)
    {
        _tcp1 = tcp1;
        _tcp2 = tcp2;
        _token = token ?? new();
    }

    public async Task BridgeConnectAsync()
    {
        var pipe1 = new Pipe();
        var pipe2 = new Pipe();

        using var stream1 = _tcp1.GetStream();
        using var stream2 = _tcp2.GetStream();

        var tasks = new[]
        {
            WriteDataAsync(stream1, pipe1.Writer, _token),
            WriteDataAsync(stream2, pipe2.Writer, _token),

            ReadDataAsync(stream2, pipe1.Reader, _token),
            ReadDataAsync(stream1, pipe2.Reader, _token)
        };

        try
        {
            await Task.WhenAny(tasks);
        }
        catch(TaskCanceledException)
        {
        }
        finally
        {
            _tcp1.Close();
            _tcp2.Close();
            await pipe1.Writer.CompleteAsync();
            await pipe2.Writer.CompleteAsync();
            await pipe1.Reader.CompleteAsync();
            await pipe2.Reader.CompleteAsync();
        }
    }

    private static async Task WriteDataAsync(NetworkStream source, PipeWriter writer, CancellationToken token)
    {
        while(!token.IsCancellationRequested)
        {
            var memory = writer.GetMemory(1024);
            var bytesRead = await source.ReadAsync(memory, token);
            if(bytesRead == 0)
            {
                break;
            }

            writer.Advance(bytesRead);

            var result = await writer.FlushAsync(token);
            if(result.IsCompleted || token.IsCancellationRequested)
            {
                break;
            }
        }

        await writer.CompleteAsync();
    }

    private static async Task ReadDataAsync(NetworkStream target, PipeReader reader, CancellationToken token)
    {
        while(!token.IsCancellationRequested)
        {
            var result = await reader.ReadAsync(token);
            if(result.IsCompleted || token.IsCancellationRequested)
            {
                break;
            }

            var buffer = result.Buffer;
            foreach(var segment in buffer)
            {
                await target.WriteAsync(segment, token);
            }

            reader.AdvanceTo(buffer.End);
        }

        await reader.CompleteAsync();
    }
}
