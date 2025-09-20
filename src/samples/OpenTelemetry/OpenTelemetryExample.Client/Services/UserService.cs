using OpenTelemetryExample.Shared.Models;
using System.Net.Http.Json;

namespace OpenTelemetryExample.Client.Services;

public sealed class UserService(HttpClient httpClient, ILogger<UserService> logger) : IUserService
{
    public async Task<List<UserDto>> GetUsersAsync()
    {
        try
        {
            logger.LogDebug("[->] Calling GET /api/users");
            var response = await httpClient.GetFromJsonAsync<List<UserDto>>("api/users");
            logger.LogDebug("[<-] Received {Count} users from API", response?.Count ?? 0);
            return response ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[!] Error calling GET /api/users");
            // Exception is logged and handled by returning an empty list.
            return [];
        }
    }

    public async Task<UserDto?> GetUserAsync(int id)
    {
        try
        {
            logger.LogDebug("[->] Calling GET /api/users/{Id}", id);
            var response = await httpClient.GetFromJsonAsync<UserDto>($"api/users/{id}");
            logger.LogDebug("[<-] Received user: {Name}", response?.Name ?? "null");
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[!] Error calling GET /api/users/{Id}", id);
            // Exception is logged and handled by returning null.
            return null;
        }
    }

    public async Task<int> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            logger.LogDebug("[->] Calling POST /api/users with Name: {Name}, Email: {Email}", request.Name, request.Email);

            // Convert CreateUserRequest to the format the API expects (CreateUserCommand)
            var command = new
            {
                request.Name,
                request.Email
            };

            var response = await httpClient.PostAsJsonAsync("api/users", command);

            if (response.IsSuccessStatusCode)
            {
                // The API returns the user ID directly in the response body
                var userId = await response.Content.ReadFromJsonAsync<int>();
                logger.LogDebug("[<-] Created user with ID: {UserId}", userId);
                return userId;
            }

            var error = await response.Content.ReadAsStringAsync();
            logger.LogError("[!] Failed to create user. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
            throw new HttpRequestException($"Failed to create user: {error}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[!] Error calling POST /api/users with Name: {Name}, Email: {Email}", request.Name, request.Email);
            throw new Exception($"Error occurred while creating user with Name: {request.Name}, Email: {request.Email}", ex);
        }
    }

    public async Task UpdateUserAsync(int id, UpdateUserRequest request)
    {
        try
        {
            logger.LogDebug("[->] Calling PUT /api/users/{Id} with Name: {Name}, Email: {Email}", id, request.Name, request.Email);

            // Convert UpdateUserRequest to the format the API expects (UpdateUserCommand)
            var command = new
            {
                UserId = id,
                request.Name,
                request.Email
            };

            var response = await httpClient.PutAsJsonAsync($"api/users/{id}", command);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                logger.LogError("[!] Failed to update user. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
                throw new HttpRequestException($"Failed to update user: {error}");
            }

            logger.LogDebug("[<-] Successfully updated user {Id}", id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[!] Error calling PUT /api/users/{Id} with Name: {Name}, Email: {Email}", id, request.Name, request.Email);
            throw new Exception($"Error occurred while updating user {id} with Name: {request.Name}, Email: {request.Email}", ex);
        }
    }

    public async Task DeleteUserAsync(int id)
    {
        try
        {
            logger.LogDebug("[->] Calling DELETE /api/users/{Id}", id);
            var response = await httpClient.DeleteAsync($"api/users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                logger.LogError("[!] Failed to delete user. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
                throw new HttpRequestException($"Failed to delete user: {error}");
            }

            logger.LogDebug("[<-] Successfully deleted user {Id}", id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[!] Error calling DELETE /api/users/{Id}", id);
            throw new Exception($"Error occurred while deleting user {id}", ex);
        }
    }
}