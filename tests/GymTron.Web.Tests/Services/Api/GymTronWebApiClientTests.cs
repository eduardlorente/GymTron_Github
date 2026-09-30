using System.Net;
using System.Text;
using System.Text.Json;
using FluentValidation;
using GymTron.Domain.Enums;
using GymTron.Domain.Exceptions;
using GymTron.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace GymTron.Web.Tests.Services.Api;

public class GymTronWebApiClientTests
{
    private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler = handler;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }

    [Fact]
    public async Task GetRoutinesAsync_ReturnsListOfRoutines_OnSuccess()
    {
        var expectedJson = """
        [
            { "id": 1, "name": "Push", "items": [] }
        ]
        """;

        var handler = new FakeHttpMessageHandler(req =>
        {
            Assert.Equal(HttpMethod.Get, req.Method);
            Assert.Equal("/api/routines", req.RequestUri?.AbsolutePath);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(expectedJson, Encoding.UTF8, "application/json")
            };
        });

        var client = new GymTronWebApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") });
        var routines = await client.GetRoutinesAsync();

        Assert.Single(routines);
        Assert.Equal(1, routines[0].Id);
        Assert.Equal("Push", routines[0].Name);
    }

    [Fact]
    public async Task GetRoutineByIdAsync_ReturnsNull_WhenNotFound()
    {
        var handler = new FakeHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.NotFound));

        var client = new GymTronWebApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") });
        var routine = await client.GetRoutineByIdAsync(999);

        Assert.Null(routine);
    }

    [Fact]
    public async Task CreateRoutineAsync_ReturnsCreatedId_OnSuccess()
    {
        var handler = new FakeHttpMessageHandler(req =>
        {
            Assert.Equal(HttpMethod.Post, req.Method);
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"id": 42}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new GymTronWebApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") });
        var id = await client.CreateRoutineAsync(new CreateRoutineRequest("Routine", []));

        Assert.Equal(42, id);
    }

    [Fact]
    public async Task UpdateRoutineAsync_ThrowsEntityNotFoundException_WhenNotFound()
    {
        var handler = new FakeHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.NotFound));

        var client = new GymTronWebApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") });

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            client.UpdateRoutineAsync(999, new UpdateRoutineRequest("Routine", [])));
    }

    [Fact]
    public async Task CreateRoutineAsync_ThrowsValidationException_WhenValidationProblemDetailsReturned()
    {
        var validationJson = """
        {
            "title": "Validation Error",
            "status": 400,
            "errors": {
                "Name": ["Name is required"]
            }
        }
        """;

        var handler = new FakeHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(validationJson, Encoding.UTF8, "application/problem+json")
            });

        var client = new GymTronWebApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") });

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            client.CreateRoutineAsync(new CreateRoutineRequest("", [])));

        Assert.Contains(ex.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task CreateRoutineAsync_ThrowsInvalidDomainOperationException_WhenProblemDetailsReturned()
    {
        var problemJson = """
        {
            "title": "Business Rule Violation",
            "status": 400,
            "detail": "Routine name already exists."
        }
        """;

        var handler = new FakeHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(problemJson, Encoding.UTF8, "application/problem+json")
            });

        var client = new GymTronWebApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") });

        var ex = await Assert.ThrowsAsync<InvalidDomainOperationException>(() =>
            client.CreateRoutineAsync(new CreateRoutineRequest("Existing", [])));

        Assert.Equal("Routine name already exists.", ex.Message);
    }

    [Fact]
    public async Task ExerciseParameters_CrudMethods_FunctionAsExpected()
    {
        var handler = new FakeHttpMessageHandler(req =>
        {
            if (req.Method == HttpMethod.Get && req.RequestUri?.AbsolutePath == "/api/exercise-parameters")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""[{"id": 1, "name": "Squat", "pattern": "Squat", "type": 0, "replaysInReserve": 2}]""", Encoding.UTF8, "application/json")
                };
            }
            if (req.Method == HttpMethod.Get && req.RequestUri?.AbsolutePath == "/api/exercise-parameters/1")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"id": 1, "name": "Squat", "pattern": "Squat", "type": 0, "replaysInReserve": 2}""", Encoding.UTF8, "application/json")
                };
            }
            if (req.Method == HttpMethod.Post)
            {
                return new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent("""{"id": 1}""", Encoding.UTF8, "application/json")
                };
            }
            if (req.Method == HttpMethod.Put)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var client = new GymTronWebApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") });

        var list = await client.GetExerciseParametersAsync();
        Assert.Single(list);

        var item = await client.GetExerciseParameterByIdAsync(1);
        Assert.NotNull(item);
        Assert.Equal("Squat", item.Name);

        var id = await client.CreateExerciseParameterAsync(new CreateExerciseParameterRequest("Squat", "", "Squat", ExerciseTypes.WEIGHT, 2));
        Assert.Equal(1, id);

        await client.UpdateExerciseParameterAsync(1, new UpdateExerciseParameterRequest("Squat", "", "Squat", ExerciseTypes.WEIGHT, 2));
    }

    [Fact]
    public async Task Users_CrudOperations_Succeed()
    {
        var handler = new FakeHttpMessageHandler(req =>
        {
            if (req.Method == HttpMethod.Get && req.RequestUri?.AbsolutePath == "/api/users")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""[{"id": 1, "username": "adm", "email": "adm@gymtron.local", "typeId": 2, "isActive": true, "createdAt": "2026-01-01T00:00:00Z"}]""", Encoding.UTF8, "application/json")
                };
            }
            if (req.Method == HttpMethod.Get && req.RequestUri?.AbsolutePath == "/api/users/1")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"id": 1, "username": "adm", "email": "adm@gymtron.local", "typeId": 2, "isActive": true, "createdAt": "2026-01-01T00:00:00Z"}""", Encoding.UTF8, "application/json")
                };
            }
            if (req.Method == HttpMethod.Post && req.RequestUri?.AbsolutePath == "/api/users")
            {
                return new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent("""{"id": 10}""", Encoding.UTF8, "application/json")
                };
            }
            if (req.Method == HttpMethod.Put && req.RequestUri?.AbsolutePath == "/api/users/10")
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            if (req.Method == HttpMethod.Delete && req.RequestUri?.AbsolutePath == "/api/users/10")
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var client = new GymTronWebApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") });

        var users = await client.GetUsersAsync();
        Assert.Single(users);
        Assert.Equal("adm", users[0].Username);
        Assert.Equal(UserTypes.Administrator, users[0].TypeId);

        var user = await client.GetUserByIdAsync(1);
        Assert.NotNull(user);
        Assert.Equal(1, user.Id);

        var createdId = await client.CreateUserAsync(new CreateUserRequest("newu", "newu@gymtron.local", "Pass12345!", UserTypes.Standard));
        Assert.Equal(10, createdId);

        await client.UpdateUserAsync(10, new UpdateUserRequest("newu2", "newu2@gymtron.local", UserTypes.Administrator, true));
        await client.DeleteUserAsync(10);
    }
}
