using MeuBarbeiro.Application.Abstractions.Messaging;
using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Contracts.Events;
using MeuBarbeiro.Application.DTOs.Appointments;
using MeuBarbeiro.Application.DTOs.Shared;
using MeuBarbeiro.Application.Mappings.Appointments;
using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.Domain.Enums;
using MeuBarbeiro.Domain.Exceptions;

namespace MeuBarbeiro.Application.Services;

public class AppointmentService(
    IAppointmentRepository appointmentRepository,
    IAppointmentServiceSelectionRepository appointmentServiceSelectionRepository,
    IBarberRepository barberRepository,
    IBarbershopRepository barbershopRepository,
    IClientRepository clientRepository,
    IReviewRepository reviewRepository,
    IServiceOfferingRepository serviceOfferingRepository,
    IUserRepository userRepository,
    IEventPublisher eventPublisher) : IAppointmentService
{
    public async Task<ServiceResult<Guid>> CreateAppointment(CreateAppointmentRequestDto request, Guid clientId,
        CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        if (request.ServiceIds.Count == 0)
        {
            validationResult.Errors.Add(new ValidationFailure(nameof(request.ServiceIds),
                "Selecione pelo menos um servico."));
            return ServiceResult<Guid>.Failure(validationResult);
        }

        var barber = await barberRepository.GetByBarbershopIdAsync(request.BarbershopId, cancellationToken);
        if (barber is null)
        {
            validationResult.Errors.Add(new ValidationFailure(nameof(request.BarbershopId),
                "Nao existe barbeiro vinculado a barbearia selecionada."));
            return ServiceResult<Guid>.Failure(validationResult);
        }

        var selectedServices = await serviceOfferingRepository.ListByIdsAsync(request.ServiceIds, cancellationToken);
        if (selectedServices.Count != request.ServiceIds.Distinct().Count() ||
            selectedServices.Any(service => service.BarbershopId != request.BarbershopId))
        {
            validationResult.Errors.Add(new ValidationFailure(nameof(request.ServiceIds),
                "Um ou mais servicos selecionados nao pertencem a barbearia."));
            return ServiceResult<Guid>.Failure(validationResult);
        }

        var totalPrice = selectedServices.Sum(service => service.Price);

        var appointment = request.ToEntity(clientId, barber.Id, totalPrice);
        validationResult = await appointmentRepository.AddAsync(appointment, cancellationToken);

        if (!validationResult.IsValid) return ServiceResult<Guid>.Failure(validationResult);

        var selections = request.ServiceIds
            .Distinct()
            .Select(serviceId => new AppointmentServiceSelection
            {
                AppointmentId = appointment.Id,
                ServiceOfferingId = serviceId
            })
            .ToArray();

        var selectionValidationResult =
            await appointmentServiceSelectionRepository.AddRangeAsync(selections, cancellationToken);
        if (!selectionValidationResult.IsValid)
        {
            return ServiceResult<Guid>.Failure(selectionValidationResult);
        }

        await eventPublisher.PublishAsync(new AppointmentRequestedIntegrationEvent(
            appointment.Id,
            appointment.ClientId,
            appointment.BarberId,
            appointment.BarbershopId,
            appointment.ScheduledAtUtc,
            appointment.TotalPrice), cancellationToken);

        return ServiceResult<Guid>.Success(appointment.Id);
    }

    public async Task<ServiceResult<AppointmentReviewResponseDto>> CreateReview(
        Guid appointmentId,
        Guid clientId,
        CreateAppointmentReviewRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null)
        {
            return ServiceResult<AppointmentReviewResponseDto>.NotFound();
        }

        if (appointment.ClientId != clientId)
        {
            var validationResult = new ValidationResult();
            validationResult.Errors.Add(new ValidationFailure(nameof(clientId),
                "O agendamento nao pertence ao cliente autenticado."));
            return ServiceResult<AppointmentReviewResponseDto>.Failure(validationResult);
        }

        if (appointment.Status != AppointmentStatus.Completed)
        {
            var validationResult = new ValidationResult();
            validationResult.Errors.Add(new ValidationFailure(nameof(appointment.Status),
                "Somente agendamentos concluidos podem ser avaliados."));
            return ServiceResult<AppointmentReviewResponseDto>.Failure(validationResult);
        }

        if (request.Stars < 1 || request.Stars > 5)
        {
            var validationResult = new ValidationResult();
            validationResult.Errors.Add(new ValidationFailure(nameof(request.Stars),
                "A avaliacao deve conter entre 1 e 5 estrelas."));
            return ServiceResult<AppointmentReviewResponseDto>.Failure(validationResult);
        }

        var existingReview = await reviewRepository.GetByAppointmentIdAsync(appointmentId, cancellationToken);
        if (existingReview != null)
        {
            var validationResult = new ValidationResult();
            validationResult.Errors.Add(new ValidationFailure(nameof(appointmentId),
                "Este agendamento ja foi avaliado."));
            return ServiceResult<AppointmentReviewResponseDto>.Failure(validationResult);
        }

        var review = new Review
        {
            AppointmentId = appointment.Id,
            ClientId = appointment.ClientId,
            BarberId = appointment.BarberId,
            BarbershopId = appointment.BarbershopId,
            Stars = request.Stars,
            CreatedAtUtc = DateTime.UtcNow
        };

        var addValidationResult = await reviewRepository.AddAsync(review, cancellationToken);
        if (!addValidationResult.IsValid)
        {
            return ServiceResult<AppointmentReviewResponseDto>.Failure(addValidationResult);
        }

        var barbershop = await barbershopRepository.GetByIdAsync(appointment.BarbershopId, cancellationToken);
        if (barbershop != null)
        {
            var averageStars =
                await reviewRepository.GetAverageStarsByBarbershopAsync(appointment.BarbershopId, cancellationToken);
            barbershop.UpdateAverageRating(averageStars ?? request.Stars);
            await barbershopRepository.UpdateAsync(barbershop, cancellationToken);
        }

        return ServiceResult<AppointmentReviewResponseDto>.Success(new AppointmentReviewResponseDto
        {
            Id = review.Id,
            AppointmentId = review.AppointmentId,
            Stars = review.Stars,
            CreatedAtUtc = review.CreatedAtUtc
        });
    }

    public async Task<ServiceResult<AppointmentResponseDto>> GetAppointmentAsync(Guid id)
    {
        var appointment = await appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
        {
            return ServiceResult<AppointmentResponseDto>.NotFound();
        }

        var response = await BuildResponseDtosAsync([appointment]);
        return ServiceResult<AppointmentResponseDto>.Success(response.Single());
    }

    public async Task<ServiceResult<IEnumerable<AppointmentResponseDto>>> GetListAppointments(Guid userId,
        AppointmentUserType userType, AppointmentStatus? status = null, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Appointment> appointments;

        switch (userType)
        {
            case AppointmentUserType.Client:
                appointments = await appointmentRepository.ListByClientAsync(userId, cancellationToken);
                break;
            case AppointmentUserType.Barber:
                appointments = await appointmentRepository.ListByBarberAsync(userId, cancellationToken);
                break;
            default:
            {
                var validationResult = new ValidationResult();
                validationResult.Errors.Add(new ValidationFailure(nameof(userType), "Tipo de usuário inválido."));

                return ServiceResult<IEnumerable<AppointmentResponseDto>>.Failure(validationResult);
            }
        }

        if (status.HasValue)
        {
            appointments = appointments
                .Where(appointment => appointment.Status == status.Value)
                .ToArray();
        }

        var response = await BuildResponseDtosAsync(appointments, cancellationToken);
        return ServiceResult<IEnumerable<AppointmentResponseDto>>.Success(response);
    }

    public async Task<ServiceResult> UpdateStatusAppointment(UpdateAppointmentStatusRequestDto request)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId);
        if (appointment == null)
        {
            return ServiceResult<bool>.NotFound();
        }

        //appointment.SetStatus(request.Status);

        var validationResult = await appointmentRepository.UpdateAsync(appointment);

        if (!validationResult.IsValid)
        {
            return ServiceResult<bool>.Failure(validationResult);
        }

        await eventPublisher.PublishAsync(new AppointmentStatusUpdatedIntegrationEvent(
            appointment.Id,
            appointment.BarberId,
            appointment.Status.ToString(),
            DateTime.UtcNow));

        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult> AcceptAppointment(Guid appointmentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();
        
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null) return ServiceResult.NotFound();

        var barber = await barberRepository.GetByUserIdAsync(userId, cancellationToken);
        if (barber == null) return ServiceResult.NotFound();

        try
        {
            appointment.Accept(barber.Id);
        }
        catch (AppointmentActorNotAllowedException ex)
        {
            return ServiceResult.Forbidden();
        }
        catch (AppointmentStatusTransitionException ex)
        {
            validationResult.Errors.Add(new ValidationFailure(nameof(Appointment.Status), "Somente agendamentos pendentes podem ser aceitos."));
            return ServiceResult.Failure(validationResult);
        }

        validationResult = await appointmentRepository.UpdateAsync(appointment, cancellationToken);
        if (!validationResult.IsValid) return ServiceResult.Failure(validationResult);
        
        await eventPublisher.PublishAsync(new AppointmentStatusUpdatedIntegrationEvent(appointment.Id,
            appointment.BarberId, appointment.Status.ToString(), DateTime.UtcNow), cancellationToken);

        return ServiceResult.Success();
    }

    private async Task<IReadOnlyCollection<AppointmentResponseDto>> BuildResponseDtosAsync(
        IEnumerable<Appointment> appointments, CancellationToken cancellationToken = default)
    {
        var appointmentList = appointments.ToArray();
        if (appointmentList.Length == 0)
        {
            return [];
        }

        var clients = await clientRepository.ListByIdsAsync(appointmentList.Select(appointment => appointment.ClientId),
            cancellationToken);
        var users = await userRepository.ListByIdsAsync(clients.Select(client => client.UserId), cancellationToken);
        var barbershops =
            await barbershopRepository.ListByIdsAsync(appointmentList.Select(appointment => appointment.BarbershopId),
                cancellationToken);
        var reviews =
            await reviewRepository.ListByAppointmentIdsAsync(appointmentList.Select(appointment => appointment.Id),
                cancellationToken);
        var selections =
            await appointmentServiceSelectionRepository.ListByAppointmentIdsAsync(
                appointmentList.Select(appointment => appointment.Id), cancellationToken);
        var services =
            await serviceOfferingRepository.ListByIdsAsync(selections.Select(selection => selection.ServiceOfferingId),
                cancellationToken);

        var clientsById = clients.ToDictionary(client => client.Id);
        var usersById = users.ToDictionary(user => user.Id);
        var barbershopsById = barbershops.ToDictionary(barbershop => barbershop.Id);
        var reviewsByAppointmentId = reviews.ToDictionary(review => review.AppointmentId);
        var servicesById = services.ToDictionary(service => service.Id);
        var selectionsByAppointmentId = selections
            .GroupBy(selection => selection.AppointmentId)
            .ToDictionary(group => group.Key,
                group => (IReadOnlyCollection<AppointmentServiceSelection>)group.ToArray());

        return appointmentList.Select(appointment =>
        {
            var dto = appointment.ToResponseDto();

            if (clientsById.TryGetValue(appointment.ClientId, out var client)
                && usersById.TryGetValue(client.UserId, out var user))
            {
                dto.ClientName = user.Name;
            }

            if (barbershopsById.TryGetValue(appointment.BarbershopId, out var barbershop))
            {
                dto.BarbershopName = barbershop.Name;
            }

            if (reviewsByAppointmentId.TryGetValue(appointment.Id, out var review))
            {
                dto.HasReview = true;
                dto.ReviewStars = review.Stars;
            }

            if (selectionsByAppointmentId.TryGetValue(appointment.Id, out var appointmentSelections))
            {
                dto.SelectedServices = appointmentSelections
                    .Where(selection => servicesById.ContainsKey(selection.ServiceOfferingId))
                    .Select(selection =>
                    {
                        var service = servicesById[selection.ServiceOfferingId];

                        return new AppointmentSelectedServiceDto
                        {
                            ServiceId = service.Id,
                            Name = service.Name,
                            Price = service.Price,
                            DurationMinutes = service.DurationMinutes
                        };
                    })
                    .ToArray();
            }

            return dto;
        }).ToArray();
    }
}