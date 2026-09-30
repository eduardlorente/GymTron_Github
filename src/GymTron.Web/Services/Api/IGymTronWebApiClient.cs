using GymTron.Application.ExerciseParameters.Queries.DTO;
using GymTron.Application.Routines.Commands;
using GymTron.Application.Routines.Queries.DTO;
using GymTron.Application.Users.DTO;
using GymTron.Domain.Enums;

namespace GymTron.Web.Services.Api;

public record CreateRoutineRequest(string Name, List<RoutineItemInput> Items);
public record UpdateRoutineRequest(string Name, List<RoutineItemInput> Items);

public record CreateExerciseParameterRequest(
    string Name,
    string Description,
    string Pattern,
    ExerciseTypes Type,
    int? ReplaysInReserve);

public record UpdateExerciseParameterRequest(
    string Name,
    string Description,
    string Pattern,
    ExerciseTypes Type,
    int? ReplaysInReserve);

public record CreateUserRequest(
    string Username,
    string Email,
    string Password,
    UserTypes TypeId);

public record UpdateUserRequest(
    string Username,
    string Email,
    UserTypes TypeId,
    bool IsActive,
    string? NewPassword = null);

public interface IGymTronWebApiClient
{
    Task<List<RoutineDto>> GetRoutinesAsync(CancellationToken ct = default);
    Task<RoutineDto?> GetRoutineByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateRoutineAsync(CreateRoutineRequest request, CancellationToken ct = default);
    Task UpdateRoutineAsync(int id, UpdateRoutineRequest request, CancellationToken ct = default);

    Task<List<ExerciseParameterDto>> GetExerciseParametersAsync(CancellationToken ct = default);
    Task<ExerciseParameterDto?> GetExerciseParameterByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateExerciseParameterAsync(CreateExerciseParameterRequest request, CancellationToken ct = default);
    Task UpdateExerciseParameterAsync(int id, UpdateExerciseParameterRequest request, CancellationToken ct = default);

    Task<List<UserDto>> GetUsersAsync(CancellationToken ct = default);
    Task<UserDto?> GetUserByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default);
    Task UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken ct = default);
    Task DeleteUserAsync(int id, CancellationToken ct = default);
}
