namespace Framework.Exceptions;

public class ValidationException(string message = "Bad request", string? code = null) : Exception(message)
{
    public string? Code { get; } = code;
}