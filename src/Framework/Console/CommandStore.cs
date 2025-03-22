using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Framework.Console.Attribute;

namespace Framework.Console;

public class CommandStore
{
    private readonly Dictionary<string, CommandDefinition> _commands = new();
    private readonly string _commandPattern = 
        @"\s(?=(?:[^'""]*(?:""(?:[^""\\]*(?:\\.)?)*""|'(?:[^'\\]*(?:\\.)?)*'))*[^'""]*$)";

    public void Add<TCommand>() where TCommand : class
    {
        Type commandType = typeof(TCommand);

        Add(commandType, _commands);
    }
    
    public CommandDefinition? GetDefinition(string command)
    {
        string[] parts = SplitCommand(command);
        return GetDefinition(parts, _commands);
    }

    public StringBuilder GetHelp()
    {
        return GetStoreHelp(new StringBuilder(), _commands);
    }

    private StringBuilder GetStoreHelp(StringBuilder builder, Dictionary<string, CommandDefinition> store)
    {
        List<CommandDefinition> commands = store.Select(value => value.Value).ToList();

        foreach (CommandDefinition definition in commands)
        {
            builder = GetCommandHelp(builder, definition);
        }

        return builder;
    }

    private StringBuilder GetCommandHelp(StringBuilder builder, CommandDefinition definition)
    {
        builder.Append($"{definition.GetHelp()}");

        if (definition.Store.Count > 0)
        {
            builder = GetStoreHelp(builder, definition.Store);
        }

        return builder;
    }

    /// <summary>
    /// Adds the command definition to the store based on the command type to specified store.
    /// In a case of command has sub-commands, the method will be called recursively and
    /// the sub-commands will be added to the store of the current command definition.
    /// </summary>
    /// <param name="commandType">Type of command to turn into command definition</param>
    /// <param name="commands">Store is used to place the command definition</param>
    /// <param name="parent">Parent command definition</param>
    /// <exception cref="InvalidOperationException"></exception>
    private void Add(
        Type commandType, Dictionary<string, CommandDefinition> commands, CommandDefinition? parent = null)
    {
        CommandAttribute? commandAttribute = commandType.GetCustomAttribute<CommandAttribute>();

        if (commandAttribute is null)
        {
            throw new InvalidOperationException(
                $"The {commandType.Name} type does not have the required CommandAttribute.");
        }
        
        var definition = new CommandDefinition
        {
            Name = commandAttribute.Name,
            CommandType = commandType,
            Parent = parent
        };

        // Check the definition existence in store by name, if not a case place it in a store
        if (!commands.TryAdd(definition.Name, definition))
        {
            throw new InvalidOperationException(
                $"The {commandType.Name} type has a duplicate command name {definition.Name}.");
        }

        foreach (var subCommand in commandAttribute.Commands)
        {
            Add(subCommand, definition.Store, definition);
        }
    }

    // Split the string into parts by space character using regular expression
    // but ignore the space character if it is inside the quotes
    private string[] SplitCommand(string command)
    {
        return Regex
            .Split(command, _commandPattern)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
    }
    
    /// <summary>
    /// Look for the command definition in a store by the command parts.
    /// Order of parts that define the command is important.
    /// If the command has sub-commands, the method will find related command recursively
    /// in the sub-command store.
    /// </summary>
    /// <param name="parts">The parts of the command - user input.</param>
    /// <param name="store">The store containing definitions to lookup for</param>
    /// <returns>Matched CommandDefinition or null for an unmatched case</returns>
    private CommandDefinition? GetDefinition(string[] parts, Dictionary<string, CommandDefinition> store)
    {
        if (parts.Length <= 0) return null;
        
        string name = parts.First();
        if (!store.TryGetValue(name, out var definition)) return null;

        string[] remainingParts = parts.Skip(1).ToArray();
        CommandDefinition? subDefinition = GetDefinition(remainingParts, definition.Store);

        if (subDefinition is not null) return subDefinition;
        
        definition.ParseArguments(remainingParts);
        return definition;
    }
}