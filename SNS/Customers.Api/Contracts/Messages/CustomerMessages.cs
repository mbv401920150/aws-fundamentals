using Customers.Api.Messaging;

namespace Customers.Api.Contracts.Messages;

public class CustomerCreated : IMessage
{
    public required Guid Id { get; init; }
    
    public required string FullName { get; init; }

    public required string Email { get; init; }
    
    public required string GitHubUsername { get; init; }

    public required DateTime DateOfBirth { get; init; }
}

public class CustomerUpdated : IMessage
{
    public required Guid Id { get; init; }
    
    public required string FullName { get; init; }

    public required string Email { get; init; }
    
    public required string GitHubUsername { get; init; }

    public required DateTime DateOfBirth { get; init; }
}

public class CustomerDeleted : IMessage
{
    public required Guid Id { get; init; }
}