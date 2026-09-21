using GymTron.Domain.Services;
using MediatR;

namespace GymTron.Application.Base;
internal abstract class BaseCommandHandler<TRequest>(IExceptionLogger<TRequest> logger) : IRequestHandler<TRequest> where TRequest : IRequest
{


    protected readonly IExceptionLogger<TRequest> _logger = logger;


    public async Task Handle(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await HandleCommand(request, cancellationToken);
        }
        catch (Exception ex)
        {
            await _logger.LogException(ex);
            throw;
        }
    }


    protected abstract Task HandleCommand(TRequest request, CancellationToken cancellationToken);
}


internal abstract class BaseCommandHandlerWithResponse<TRequest, TResponse>(IExceptionLogger<TRequest> logger)
    : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{


    protected readonly IExceptionLogger<TRequest> _logger = logger;


    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await HandleCommand(request, cancellationToken);
        }
        catch (Exception ex)
        {
            await _logger.LogException(ex);
            throw;
        }
    }


    protected abstract Task<TResponse> HandleCommand(TRequest request, CancellationToken cancellationToken);
}
