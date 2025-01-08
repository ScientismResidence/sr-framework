using Framework.Console.Exception;
using Framework.Logger;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.Console;

public class ApplicationRunner(IServiceProvider provider, CommandStore store)
{
    private readonly ILogger _logger = provider.GetService<ILogger>();

    public async Task ExecuteAsync(string command)
    {
        _logger.Log($"Executing command: [{command}]...");
        CommandDefinition definition = store.GetDefinition(command);
        
        if (definition is null)
        {
            _logger.Log("Unknown command.", LogLevel.Error);
            _logger.Log("List of commands (help):");
            _logger.Log(store.GetHelp().ToString());
            _logger.Log("-----------------");
        }
        else
        {
            await ExecuteAsync(definition);
        }
    }

    public async Task InteractAsync()
    {
        _logger.Log("Type a command...");
        
        while (true)
        {
            string command = System.Console.ReadLine();

            if (string.IsNullOrWhiteSpace(command))
            {
                continue;
            }

            if (command.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            await ExecuteAsync(command);
        }
    }

    private async Task ExecuteAsync(CommandDefinition definition)
    {
        try
        {
            using IServiceScope scope = provider.CreateScope();
            
            Type handlerType = typeof(ICommandHandler<>).MakeGenericType(definition.CommandType);
            dynamic handler = scope.ServiceProvider.GetService(handlerType)
                ?? throw new ApplicationException("Can't resolve handler type.");
            
            object commandResult = definition.CreateCommand();

            await handler.HandleAsync((dynamic)commandResult);

            _logger.Log("Command executed successfully.");
            _logger.Log("-----------------");
        }
        catch (CommandValidationException exception)
        {
            _logger.Log(exception.Message, LogLevel.Error);
            DisplayHelp(definition);
        }
        catch (System.Exception exception)
        {
            _logger.Log("Command finished with error", exception);
            _logger.Log("-----------------");
        }
    }

    private void DisplayHelp(CommandDefinition definition)
    {
        try
        {
            _logger.Log("Command help:");
            _logger.Log(definition.GetHelp().ToString());
            _logger.Log("-----------------");
        }
        catch (CommandValidationException exception)
        {
            _logger.Log(exception.Message);
        }
        catch (System.Exception exception)
        {
            _logger.Log("Unable to compose help information.", exception);
        }
    }
}