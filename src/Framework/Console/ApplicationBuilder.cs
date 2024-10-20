namespace Framework.Console;

public class ApplicationBuilder
{
    private readonly CommandStore _store = new();
    private IContext _provider;
    
    public ApplicationBuilder UseProvider(IContext provider)
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
        return new ApplicationRunner(_provider, _store);
    }
}