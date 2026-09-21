namespace GymTron.Domain.Exceptions;

public class InvalidDomainOperationException(string errorMessage) : DomainException(errorMessage)
{ }
