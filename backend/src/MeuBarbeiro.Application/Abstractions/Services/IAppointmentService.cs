using MeuBarbeiro.Application.DTOs.Appointments;
using MeuBarbeiro.Application.DTOs.Shared;
using MeuBarbeiro.Domain.Enums;

namespace MeuBarbeiro.Application.Abstractions.Services;

public interface IAppointmentService
{
    Task<ServiceResult<Guid>> CreateAppointment(CreateAppointmentRequestDto request, Guid clientId);
    Task<ServiceResult<AppointmentReviewResponseDto>> CreateReview(Guid appointmentId, Guid clientId, CreateAppointmentReviewRequestDto request, CancellationToken cancellationToken = default);
    Task<ServiceResult<AppointmentResponseDto>> GetAppointment(Guid id);
    Task<ServiceResult<IEnumerable<AppointmentResponseDto>>> GetListAppointments(Guid userId, AppointmentUserType userType, AppointmentStatus? status = null, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> UpdateStatusAppointment(UpdateAppointmentStatusRequestDto request);
}
