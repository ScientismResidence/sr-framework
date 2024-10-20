namespace Framework.Console.Attribute;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class ArgumentAttribute : System.Attribute
{
    public ArgumentAttribute(string name)
    {
        Name = name;
    }
    
    public string Name { get; }
}