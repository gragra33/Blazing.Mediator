namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Command test type implementing IRequest with Command in name.
/// </summary>
public class RequestBasedTestCommand : IRequest
{
    public bool Flag { get; set; }
}