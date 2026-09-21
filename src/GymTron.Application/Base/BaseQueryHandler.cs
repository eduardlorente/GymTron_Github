using GymTron.Domain.Services;
using MediatR;

namespace GymTron.Application.Base;

internal abstract class BaseQueryHandler<TRequest, TResponse>(IExceptionLogger<TRequest> logger)
    : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{


    protected readonly IExceptionLogger<TRequest> _logger = logger;


    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await HandleQuery(request, cancellationToken);
        }
        catch (Exception ex)
        {
            await _logger.LogException(ex);
            throw;
        }
    }


    protected abstract Task<TResponse> HandleQuery(TRequest request, CancellationToken cancellationToken);
}
