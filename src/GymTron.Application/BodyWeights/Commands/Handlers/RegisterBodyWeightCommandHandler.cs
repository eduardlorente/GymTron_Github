using GymTron.Application.Base;
using GymTron.Domain.Entities;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.BodyWeights.Commands.Handlers;

internal class RegisterBodyWeightCommandHandler(IBodyWeightRepository bodyWeightRepository,
                                                IExceptionLogger<RegisterBodyWeightCommand> logger,
                                                IClock clock)
    : BaseCommandHandler<RegisterBodyWeightCommand>(logger)
{


    private readonly IBodyWeightRepository _bodyWeightRepository = bodyWeightRepository;
    private readonly IClock _clock = clock;


    protected override async Task HandleCommand(RegisterBodyWeightCommand request, CancellationToken cancellationToken)
    {
        BodyWeight bodyWeight = BodyWeight.New(request.Weight, request.BodyFatPercentage, _clock.UtcNow, request.UserId);

        await _bodyWeightRepository.Add(bodyWeight, cancellationToken);
    }
}
