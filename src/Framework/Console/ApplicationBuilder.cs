namespace Framework.Console;

public class ApplicationBuilder
{
    private readonly CommandStore _store = new();
    private IServiceProvider? _provider;
    
    public ApplicationBuilder UseProvider(IServiceProvider provider)
    {
        _provider = provider;

        return this;
    }

    public ApplicationBuilder AddCommand<TCommand>() where TCommand : class
    {
        _store.Add<TCommand>();

        return this;
    }

    public ApplicationRunner Build()
    {
        if (_provider is null)
        {
            throw new InvalidOperationException("No provider is configured.");
        }
        
        return new ApplicationRunner(_provider, _store);
    }
}