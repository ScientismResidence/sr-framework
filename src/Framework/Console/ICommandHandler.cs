namespace Framework.Console;

public interface ICommandHandler<in TCommand> where TCommand : new()
{
    Task HandleAsync(TCommand command);
}