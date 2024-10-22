using Costumers.Consumer.Messages;
using MediatR;

namespace Costumers.Consumer.Handlers;

public class CustomerCreatedHandler : IRequestHandler<CustomerCreated>
{
    private readonly ILogger<CustomerCreatedHandler> _logger;

    public CustomerCreatedHandler(ILogger<CustomerCreatedHandler> logger)
    {
        _logger = logger;
    }
    
    public Task Handle(CustomerCreated request, CancellationToken cancellationToken)
    {
        throw new Exception("INTENTIONAL ERROR TO TEST DEAD LETTER QUEUE");
        _logger.LogInformation($"Customer Created - {request.FullName}");
        return Unit.Task;
    }
}