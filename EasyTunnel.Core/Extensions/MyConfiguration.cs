using Microsoft.Extensions.Configuration;

namespace EasyTunnel.Shared.Extensions;

public class MyConfiguration
{
    private const string file = "./appsettings.json";

    public static T GetObject<T>(string key)
        where T : new()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile(file)
            .Build();

        var obj = new T();
        builder.Bind(key, obj);

        return obj;
    }

    public static string GetValue(string key)
    {
        return new ConfigurationBuilder()
            .AddJsonFile(file)
            .Build()
            .GetSection(key).Value!;
    }

    public static string GetConnectionString(string key)
    {
        return new ConfigurationBuilder()
            .AddJsonFile(file)
            .Build()
            .GetConnectionString(key)!;
    }
}