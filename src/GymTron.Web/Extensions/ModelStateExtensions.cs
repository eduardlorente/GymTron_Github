using FluentValidation;
using GymTron.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GymTron.Web.Extensions;

public static class ModelStateExtensions
{
    public static void AddValidationException(this ModelStateDictionary modelState, ValidationException exception)
    {
        foreach (var error in exception.Errors)
        {
            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }

    public static void AddDomainException(this ModelStateDictionary modelState, DomainException exception)
    {
        modelState.AddModelError(string.Empty, exception.Message);
    }
}
