namespace UserManagement.Api.Application.Queries;

/// <summary>
/// Data transfer object for user statistics information.
/// </summary>
public class UserStatisticsDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the age of the user account in days.
    /// </summary>
    public int AccountAgeInDays { get; set; }

    /// <summary>
    /// Gets or sets the current status of the user account.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}