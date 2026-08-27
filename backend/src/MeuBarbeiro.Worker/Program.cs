using MeuBarbeiro.Infrastructure.Messaging;
using MeuBarbeiro.Infrastructure.Persistence;
using MeuBarbeiro.Worker;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
builder.Services.AddSingleton<RabbitMqTopologyInitializer>();
builder.Services.AddSingleton<DatabaseSchemaInitializer>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();