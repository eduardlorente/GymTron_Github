using GymTron.Domain.Entities;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;
using MediatR;

namespace GymTron.Application.Logs.Commands.Handlers;

internal class RegisterLogCommandHandler(ILogRepository logRepository, IClock clock)
    : IRequestHandler<RegisterLogCommand>
{


    private readonly ILogRepository _logRepository = logRepository;
    private readonly IClock _clock = clock;


    public async Task Handle(RegisterLogCommand request, CancellationToken cancellationToken)
    {
        Log log = Log.New(request.Message, _clock);

        await _logRepository.Add(log, cancellationToken);
    }
}
