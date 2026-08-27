using System.Text;
using System.Text.Json;
using MeuBarbeiro.Contracts.Events;
using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.Infrastructure.Messaging;
using MeuBarbeiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MeuBarbeiro.Worker;

public class Worker(ILogger<Worker> logger, IRabbitMqConnectionProvider connectionProvider, RabbitMqTopologyInitializer topologyInitializer, DatabaseSchemaInitializer databaseSchemaInitializer,
    IOptions<RabbitMqOptions> options, IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private IModel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        topologyInitializer.Initialize();

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await databaseSchemaInitializer.EnsureSchemaAsync(dbContext, stoppingToken);

        _channel = connectionProvider.CreateChannel();
        _channel.BasicQos(0, 1, false);

        ConfigureConsumer(_options.RequestedQueue, HandleRequestedAsync);
        ConfigureConsumer(_options.StatusUpdatedQueue, HandleStatusUpdatedAsync);

        logger.LogInformation("MeuBarbeiro worker ativo. Consumindo filas {RequestedQueue} e {StatusUpdatedQueue}.",
            _options.RequestedQueue,
            _options.StatusUpdatedQueue);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }

    private void ConfigureConsumer(string queueName, Func<string, string, Task> handler)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, args) =>
        {
            var payload = Encoding.UTF8.GetString(args.Body.ToArray());

            try
            {
                await handler(queueName, payload);
                _channel!.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar mensagem da fila {QueueName}.", queueName);
                await PersistAuditAsync(typeof(object).Name, queueName, payload, "Failed", ex.Message);
                _channel!.BasicNack(args.DeliveryTag, false, false);
            }
        };

        _channel!.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
    }

    private async Task HandleRequestedAsync(string queueName, string payload)
    {
        var message = JsonSerializer.Deserialize<AppointmentRequestedIntegrationEvent>(payload, JsonOptions)
                      ?? throw new InvalidOperationException("Payload AppointmentRequested invalido.");

        await MarkAppointmentAsInProgressAsync(message.AppointmentId);
        logger.LogInformation("Consumed event AppointmentRequested from queue {QueueName} for appointment {AppointmentId}.", queueName, message.AppointmentId);
        await PersistAuditAsync(nameof(AppointmentRequestedIntegrationEvent), queueName, payload, "Processed");
    }

    private async Task HandleStatusUpdatedAsync(string queueName, string payload)
    {
        var message = JsonSerializer.Deserialize<AppointmentStatusUpdatedIntegrationEvent>(payload, JsonOptions)
                      ?? throw new InvalidOperationException("Payload AppointmentStatusUpdated invalido.");

        logger.LogInformation("Consumed event AppointmentStatusUpdated from queue {QueueName} for appointment {AppointmentId}.", queueName, message.AppointmentId);
        await PersistAuditAsync(nameof(AppointmentStatusUpdatedIntegrationEvent), queueName, payload, "Processed");
    }

    private async Task PersistAuditAsync(string eventName, string queueName, string payload, string status, string? errorMessage = null)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            dbContext.EventProcessingAudits.Add(new EventProcessingAudit
            {
                EventName = eventName,
                QueueName = queueName,
                Payload = payload,
                ProcessedAtUtc = DateTime.UtcNow,
                Status = status,
                ErrorMessage = errorMessage
            });

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao persistir auditoria do evento {EventName} na fila {QueueName}.", eventName, queueName);
        }
    }

    private async Task MarkAppointmentAsInProgressAsync(Guid appointmentId)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var appointment = await dbContext.Appointments.FirstOrDefaultAsync(item => item.Id == appointmentId);
        if (appointment is null)
        {
            logger.LogWarning("Appointment {AppointmentId} nao encontrado para marcar como InProgress.", appointmentId);
            return;
        }

        if (appointment.Status == Domain.Enums.AppointmentStatus.InProgress)
        {
            return;
        }

        //appointment.SetStatus(Domain.Enums.AppointmentStatus.InProgress);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Appointment {AppointmentId} atualizado para InProgress apos processamento do evento.", appointmentId);
    }
}
