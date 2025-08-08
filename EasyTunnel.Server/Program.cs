using EasyTunnel.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR(p =>
{
    p.ClientTimeoutInterval = TimeSpan.FromSeconds(120);
});

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapHub<EasyTunnelHub>("/server");

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
