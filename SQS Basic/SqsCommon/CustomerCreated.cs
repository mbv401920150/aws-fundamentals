namespace SqsCommon;

public class CustomerCreated
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required string GitHubUser { get; init; }
    public required DateTime DateOfBirth { get; init; }
}