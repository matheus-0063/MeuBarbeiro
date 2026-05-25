namespace MeuBarbeiro.Api.Models.Responses;

public sealed class EventProcessingAuditResponseModel
{
    public Guid Id { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
