using OpenTelemetryExample.Client.Models;

namespace OpenTelemetryExample.Client.Services;

public interface IUserService
{
    Task<List<UserDto>> GetUsersAsync();
    Task<UserDto?> GetUserAsync(int id);
    Task<int> CreateUserAsync(CreateUserRequest request);
    Task UpdateUserAsync(int id, UpdateUserRequest request);
    Task DeleteUserAsync(int id);
}