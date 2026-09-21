using MediatR;

namespace GymTron.Application.Base;

public class QueryBase<TResponse> : IRequest<TResponse>
{


    public Guid CorrelationId { get; set; }


    public QueryBase(Guid correlationId)
    {
        if (correlationId == Guid.Empty)
            throw new ArgumentException("CorrelationId cannot be empty", nameof(correlationId));

        CorrelationId = correlationId;
    }
}