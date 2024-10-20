namespace Framework.Console.Exception;

public class CommandValidationException : System.Exception
{
    public CommandValidationException(string message): base(message)
    {}
}