using System.Text.Json.Serialization;

namespace Streaming.Api.Models;

/// <summary>
/// Contact model representing a person's information
/// </summary>
public class Contact
{
    public int Id { get; set; }
    public string Avatar { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;
    public string? Suffix { get; set; }
    public string Gender { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Company Company { get; set; } = new();

    public string FullName => $"{FirstName} {LastName}".Trim();
}

/// <summary>
/// Address information for a contact
/// </summary>
public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;

    public string FormattedAddress =>
        $"{Street}, {City}{(string.IsNullOrEmpty(State) ? "" : $", {State}")}{(string.IsNullOrEmpty(PostalCode) ? "" : $" {PostalCode}")}, {Country}";
}

/// <summary>
/// Company information for a contact
/// </summary>
public class Company
{
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
