using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using GymTron.Application.ExerciseParameters.Queries.DTO;
using GymTron.Application.Routines.Queries.DTO;
using GymTron.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GymTron.Web.Services.Api;

public class GymTronWebApiClient(HttpClient httpClient) : IGymTronWebApiClient
{
    private readonly HttpClient _httpClient = httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed record CreatedIdDto(int Id);

    public async Task<List<RoutineDto>> GetRoutinesAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("api/routines", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<List<RoutineDto>>(JsonOptions, ct) ?? [];
    }

    public async Task<RoutineDto?> GetRoutineByIdAsync(int id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"api/routines/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessOrThrowAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<RoutineDto>(JsonOptions, ct);
    }

    public async Task<int> CreateRoutineAsync(CreateRoutineRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/routines", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        var created = await response.Content.ReadFromJsonAsync<CreatedIdDto>(JsonOptions, ct);
        return created?.Id ?? 0;
    }

    public async Task UpdateRoutineAsync(int id, UpdateRoutineRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/routines/{id}", request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new EntityNotFoundException($"Routine with ID {id} was not found.");
        }

        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task<List<ExerciseParameterDto>> GetExerciseParametersAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("api/exercise-parameters", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<List<ExerciseParameterDto>>(JsonOptions, ct) ?? [];
    }

    public async Task<ExerciseParameterDto?> GetExerciseParameterByIdAsync(int id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"api/exercise-parameters/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessOrThrowAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<ExerciseParameterDto>(JsonOptions, ct);
    }

    public async Task<int> CreateExerciseParameterAsync(CreateExerciseParameterRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/exercise-parameters", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        var created = await response.Content.ReadFromJsonAsync<CreatedIdDto>(JsonOptions, ct);
        return created?.Id ?? 0;
    }

    public async Task UpdateExerciseParameterAsync(int id, UpdateExerciseParameterRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/exercise-parameters/{id}", request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new EntityNotFoundException($"Exercise parameter with ID {id} was not found.");
        }

        await EnsureSuccessOrThrowAsync(response, ct);
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            string rawBody = await response.Content.ReadAsStringAsync(ct);
            try
            {
                var validationProblem = JsonSerializer.Deserialize<ValidationProblemDetails>(rawBody, JsonOptions);
                if (validationProblem?.Errors != null && validationProblem.Errors.Count > 0)
                {
                    var failures = validationProblem.Errors.SelectMany(kvp =>
                        kvp.Value.Select(msg => new ValidationFailure(kvp.Key, msg))
                    ).ToList();
                    throw new ValidationException(failures);
                }

                var problem = JsonSerializer.Deserialize<ProblemDetails>(rawBody, JsonOptions);
                if (problem != null && (!string.IsNullOrEmpty(problem.Detail) || !string.IsNullOrEmpty(problem.Title)))
                {
                    throw new InvalidDomainOperationException(problem.Detail ?? problem.Title ?? "Invalid request.");
                }
            }
            catch (JsonException)
            {
                // Not standard problem details JSON, fall through to EnsureSuccessStatusCode
            }
        }

        response.EnsureSuccessStatusCode();
    }
}
