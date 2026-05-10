namespace MeuBarbeiro.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "MeuBarbeiro worker ativo. Aguardando consumo de eventos em {time}.",
                DateTimeOffset.Now);

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
