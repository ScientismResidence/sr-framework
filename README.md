This is the set of classes used across my projects to simplify boiler-plating, configuration and services design.

### v.1.1

- Autofac is not used anymore as container for dependency injection
- `IContext` is removed
- `ApplicationDbContext` and `Console.ApplicationBuilder` use default `IServiceProvider` instead of `IContext`
