using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Framework.Console.Attribute;
using Framework.Console.Exception;

namespace Framework.Console;

public class CommandDefinition
{
    private readonly string _argumentPattern =
        @"(?<key>[^=]+)(?:=(?<value>.*))?";
    private List<KeyValuePair<string, string>> _arguments = new();

    public Type CommandType { get; init; }

    public string Name { get; init; }

    public CommandDefinition Parent { get; init; }

    public Dictionary<string, CommandDefinition> Store { get; } = new();

    public void ParseArguments(string[] arguments)
    {
        _arguments = arguments
            .Select(argument =>
            {
                Match match = Regex.Match(argument, _argumentPattern);

                string argumentName = SanitizeName(match.Groups["key"].Value);
                string argumentValue = SanitizeValue(match.Groups["value"].Value);

                return new KeyValuePair<string, string>(argumentName, argumentValue);
            })
            .ToList();

        // Remove the '-' or '--' from the argument name if it exists
        string SanitizeName(string name)
        {
            if (name.StartsWith("--")) return name[2..];
            return name.StartsWith('-') ? name[1..] : name;
        }

        // Remove the edge quotes from the argument value if it exists
        string SanitizeValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            if ((value.StartsWith('"') && value.EndsWith('"'))
                || (value.StartsWith('\'') && value.EndsWith('\'')))
            {
                return value[1..^1];
            }

            return value;
        }
    }

    public object CreateCommand()
    {
        Type type = CommandType;
        IEnumerable<PropertyInfo> argumentProperties = type
            .GetProperties().Where(property => property.GetCustomAttributes<ArgumentAttribute>().Any());

        var result = argumentProperties.SelectMany(property => property
            .GetCustomAttributes<ArgumentAttribute>().Select(attribute => new
            {
                Property = property,
                Attribute = attribute
            })
        );

        Dictionary<string, PropertyInfo> properties;
        try
        {
            properties = result
                .ToDictionary(value => value.Attribute.Name, value => value.Property);
        }
        catch (ArgumentException)
        {
            throw new CommandValidationException(
                $"An error happened on command [{Name}] parsing. " +
                $"There is an argument with a duplicated name for {nameof(ArgumentAttribute)}");
        }

        object command = Activator.CreateInstance(CommandType);
        
        HashSet<string> argumentNames = new();
        foreach (KeyValuePair<string, string> argument in _arguments)
        {
            string name = ApplyArgument(argument, properties, command);

            if (argumentNames.Contains(name))
            {
                throw new CommandValidationException(
                    $"The argument [{argument.Key}] duplicates another argument for command [{Name}]. " +
                    $"Use only one of them");
            }

            argumentNames.Add(name);
        }

        return command;
    }

    public StringBuilder GetHelp()
    {
        // Get class level attributes
        CommandAttribute commandClassAttribute = CommandType.GetCustomAttribute<CommandAttribute>();
        HelpAttribute helpClassAttribute = CommandType.GetCustomAttribute<HelpAttribute>();
        
        // Build the result string for the class
        StringBuilder help = new();
        if (commandClassAttribute == null)
        {
            throw new CommandValidationException(
                $"Unable to get help for command [{Name}]. " +
                $"Use [{nameof(CommandAttribute)}] and [{nameof(HelpAttribute)}]");
        }

        if (helpClassAttribute is not null)
        {
            help.Append($"{commandClassAttribute.Name}\t[{helpClassAttribute.Help}]");
        }
        else
        {
            help.Append($"{commandClassAttribute.Name}");
        }
        
        if (Parent is not null)
        {
            help = JoinParentNames(help, Parent);
        }

        // Iterate through properties
        foreach (var property in CommandType.GetProperties())
        {
            // Get property level attributes
            List<ArgumentAttribute> argumentAttributes = property
                .GetCustomAttributes<ArgumentAttribute>().ToList();
            HelpAttribute helpAttribute = property.GetCustomAttribute<HelpAttribute>();

            if (!argumentAttributes.Any())
            {
                continue;
            }
            
            help.Append($"{Environment.NewLine}\t");
            
            // Build the result string for each property
            foreach (var argumentAttribute in argumentAttributes)
            {
                help.Append($"-{argumentAttribute.Name} ");
            }
            
            if (helpAttribute != null)
            {
                help.Append($"\t[{helpAttribute.Help}]");
            }
            else
            {
                help.Append("[No help available for this argument]");
            }
        }

        return help;
    }

    private StringBuilder JoinParentNames(StringBuilder builder, CommandDefinition definition)
    {
        StringBuilder result = new();
        result.Append($"{definition.Name} {builder.ToString()}");

        if (definition.Parent is not null)
        {
            return JoinParentNames(result, definition.Parent);
        }

        return result;
    }

    private string ApplyArgument(
        KeyValuePair<string, string> argument, Dictionary<string, PropertyInfo> properties, object command)
    {
        if (!properties.TryGetValue(argument.Key, out PropertyInfo property))
        {
            throw new CommandValidationException(
                $"Unknown argument [{argument.Key}] for command [{Name}] with value [{argument.Value}]");
        }

        switch (property.PropertyType)
        {
            case Type type when type == typeof(string):
                SetStringProperty(command, property, argument);
                break;
            case Type type when type == typeof(bool):
                SetBooleanProperty(command, property, argument);
                break;
            case Type type when type == typeof(int):
                SetIntegerProperty(command, property, argument);
                break;
            default:
                throw new CommandValidationException(
                    $"The argument [{argument.Key}] for [{Name}] command " +
                    $"has unsupported type [{property.PropertyType}]");
        }

        return property.Name;
    }

    private void SetStringProperty(
        object command, PropertyInfo property, KeyValuePair<string, string> argument)
    {
        if (string.IsNullOrEmpty(argument.Value))
        {
            throw new CommandValidationException(
                $"The argument [{argument.Key}] for [{Name}] command " +
                $"supposed to have the value but it wasn't provided");
        }

        property.SetValue(command, argument.Value);
    }

    private void SetBooleanProperty(
        object command, PropertyInfo property, KeyValuePair<string, string> argument)
    {
        if (string.IsNullOrEmpty(argument.Value))
        {
            property.SetValue(command, true);
        }
        // Convert the string value to bool and set it to the property
        else if (bool.TryParse(argument.Value, out bool result))
        {
            property.SetValue(command, result);
        }
        else
        {
            throw new CommandValidationException(
                $"The argument [{argument.Key}] for [{Name}] command " +
                $"supposed to have the convertable to bool value but has [{argument.Value}]");
        }
    }

    private void SetIntegerProperty(
        object command, PropertyInfo property, KeyValuePair<string, string> argument)
    {
        if (string.IsNullOrEmpty(argument.Value))
        {
            throw new CommandValidationException(
                $"The argument [{argument.Key}] for [{Name}] command " +
                $"supposed to have the value but it wasn't provided");
        }

        // Convert the string value to int and set it to the property
        if (int.TryParse(argument.Value, out int result))
        {
            property.SetValue(command, result);
        }
        else
        {
            throw new CommandValidationException(
                $"The argument [{argument.Key}] for [{Name}] command " +
                $"supposed to have the convertable to int value but has [{argument.Value}]");
        }
    }
}