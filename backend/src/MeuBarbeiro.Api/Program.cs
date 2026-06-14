using System.Text;
using Asp.Versioning;
using MeuBarbeiro.Application.Abstractions.Messaging;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.Services;
using MeuBarbeiro.Infrastructure.Messaging;
using MeuBarbeiro.Infrastructure.Persistence;
using MeuBarbeiro.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]!;

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecretKey)
            )
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<DatabaseSchemaInitializer>();
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
builder.Services.AddSingleton<RabbitMqTopologyInitializer>();
builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
builder.Services.AddScoped<IAppointmentRepository, SqliteAppointmentRepository>();
builder.Services.AddScoped<IBarberRepository, SqliteBarberRepository>();
builder.Services.AddScoped<IBarbershopRepository, SqliteBarbershopRepository>();
builder.Services.AddScoped<IClientRepository, SqliteClientRepository>();
builder.Services.AddScoped<IServiceOfferingRepository, SqliteServiceOfferingRepository>();
builder.Services.AddScoped<IUserRepository, SqliteUserRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IBarbershopService, BarbershopService>();
builder.Services.AddScoped<IServicesService, ServicesService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var databaseSchemaInitializer = scope.ServiceProvider.GetRequiredService<DatabaseSchemaInitializer>();
    var topologyInitializer = scope.ServiceProvider.GetRequiredService<RabbitMqTopologyInitializer>();
    await databaseSchemaInitializer.EnsureSchemaAsync(dbContext);
    topologyInitializer.Initialize();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    name = "MeuBarbeiro API",
    status = "running",
    architecture = "clean-architecture + rabbitmq + sqlite",
    nextStep = "Implement appointment, barbershop and review endpoints"
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestampUtc = DateTime.UtcNow
}));

app.MapGet("/api/roadmap", () => Results.Ok(new[]
{
    "Sprint 1: backend REST + SQLite + documentacao",
    "Sprint 2: RabbitMQ + eventos",
    "Sprint 3: app cliente",
    "Sprint 4: app prestador + integracao fim a fim"
}));

app.Run();
