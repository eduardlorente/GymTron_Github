namespace GymTron.Domain.Exceptions;

public class EntityNotFoundException(string entityName) : DomainException($"Entity not found: {entityName}")
{ }
