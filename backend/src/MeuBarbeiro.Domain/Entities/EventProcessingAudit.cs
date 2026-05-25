namespace MeuBarbeiro.Domain.Entities;

public sealed class EventProcessingAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventName { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
