using Autofac;
using Framework;

namespace Infrastructure;

public class AutofacContext : IContext
{
    public AutofacContext(ILifetimeScope scope)
    {
        Scope = scope;
    }

    private ILifetimeScope Scope { get; }

    public T Get<T>()
    {
        return Scope.Resolve<T>();
    }

    public object Get(Type type)
    {
        return Scope.Resolve(type);
    }
}
