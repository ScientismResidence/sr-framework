namespace Framework.Console.Attribute;

[AttributeUsage(AttributeTargets.Class)]
public class CommandAttribute : System.Attribute
{
    public string Name { get; }
    public Type[] Commands { get; }

    public CommandAttribute(string name, params Type[] commands)
    {
        Name = name;
        Commands = commands;
    }
}