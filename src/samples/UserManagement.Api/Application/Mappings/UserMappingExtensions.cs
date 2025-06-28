using UserManagement.Api.Application.DTOs;
using UserManagement.Api.Domain.Entities;

namespace UserManagement.Api.Application.Mappings;

/// <summary>
/// Extension methods for mapping between User domain entities and DTOs.
/// </summary>
public static class UserMappingExtensions
{
    /// <summary>
    /// Converts a User domain entity to a UserDto.
    /// </summary>
    /// <param name="user">The user entity to convert.</param>
    /// <returns>A UserDto representation of the user.</returns>
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.GetFullName(),
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    /// <summary>
    /// Converts a collection of User domain entities to a list of UserDtos.
    /// </summary>
    /// <param name="users">The collection of user entities to convert.</param>
    /// <returns>A list of UserDto representations.</returns>
    public static List<UserDto> ToDto(this IEnumerable<User> users)
    {
        return users.Select(u => u.ToDto()).ToList();
    }

    /// <summary>
    /// Converts a collection of User domain entities to a paginated result with UserDtos.
    /// </summary>
    /// <param name="users">The collection of user entities to convert.</param>
    /// <param name="totalCount">The total number of users available.</param>
    /// <param name="page">The current page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated result containing UserDtos.</returns>
    public static PagedResult<UserDto> ToPagedDto(this IEnumerable<User> users, int totalCount, int page, int pageSize)
    {
        return new PagedResult<UserDto>
        {
            Items = users.ToDto(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
