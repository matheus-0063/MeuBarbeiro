using Asp.Versioning;
using MeuBarbeiro.Api.Models.Responses;
using MeuBarbeiro.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MeuBarbeiro.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/event-processing-audits")]
public class EventProcessingAuditController(AppDbContext dbContext) : BaseController
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<EventProcessingAuditResponseModel>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAudits(
        [FromQuery] string? eventName = null,
        [FromQuery] string? queueName = null,
        [FromQuery] string? status = null,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 500);

        var query = dbContext.EventProcessingAudits
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(eventName))
        {
            query = query.Where(audit => audit.EventName == eventName);
        }

        if (!string.IsNullOrWhiteSpace(queueName))
        {
            query = query.Where(audit => audit.QueueName == queueName);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(audit => audit.Status == status);
        }

        var audits = await query
            .OrderByDescending(audit => audit.ProcessedAtUtc)
            .Take(take)
            .Select(audit => new EventProcessingAuditResponseModel
            {
                Id = audit.Id,
                EventName = audit.EventName,
                QueueName = audit.QueueName,
                Payload = audit.Payload,
                ProcessedAtUtc = audit.ProcessedAtUtc,
                Status = audit.Status,
                ErrorMessage = audit.ErrorMessage
            })
            .ToListAsync(cancellationToken);

        return Ok(audits);
    }
}
