using MediatR;

namespace GymTron.Application.Base;

public class CommandBase : IRequest
{


    public Guid CorrelationId { get; set; }


    public CommandBase(Guid correlationId)
    {
        if (correlationId == Guid.Empty)
            throw new ArgumentException("CorrelationId cannot be empty", nameof(correlationId));

        CorrelationId = correlationId;
    }
}


public class CommandBaseWithResponse<TResponse> : IRequest<TResponse>
{


    public Guid CorrelationId { get; set; }


    public CommandBaseWithResponse(Guid correlationId)
    {
        if (correlationId == Guid.Empty)
            throw new ArgumentException("CorrelationId cannot be empty", nameof(correlationId));

        CorrelationId = correlationId;
    }
}
