using GymTron.Api.Endpoints.Auth;
using GymTron.Api.Endpoints.BodyWeights;
using GymTron.Api.Endpoints.Exercises;
using GymTron.Api.Endpoints.ExerciseParameters;
using GymTron.Api.Endpoints.Routines;
using GymTron.Api.Endpoints.Trainings;

namespace GymTron.Api.Endpoints;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        // Auth
        app.MapRegisterEndpoint();
        app.MapLoginEndpoint();
        app.MapRefreshTokenEndpoint();
        app.MapRevokeEndpoint();

        // Routines
        app.MapListAllRoutinesEndpoint();
        app.MapGetRoutineByIdEndpoint();
        app.MapCreateRoutineEndpoint();
        app.MapUpdateRoutineEndpoint();

        // ExerciseParameters
        app.MapListExerciseParametersEndpoint();
        app.MapGetExerciseParameterByIdEndpoint();
        app.MapCreateExerciseParameterEndpoint();
        app.MapUpdateExerciseParameterEndpoint();

        // Exercises
        app.MapGetExerciseHistoryEndpoint();

        // BodyWeights
        app.MapGetBodyWeightHistoryEndpoint();
        app.MapRegisterBodyWeightEndpoint();

        // Trainings
        app.MapGetCurrentTrainingEndpoint();
        app.MapGetTrainingHistoryEndpoint();
        app.MapStartTrainingEndpoint();
        app.MapFinishTrainingEndpoint();
        app.MapCancelTrainingEndpoint();
        app.MapAddExerciseToTrainingEndpoint();

        return app;
    }
}
