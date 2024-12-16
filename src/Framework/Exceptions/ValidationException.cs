namespace Framework.Exceptions;

public class ValidationException(string message = "Bad request") : Exception(message);