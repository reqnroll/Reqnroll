Reqnroll.Microsoft.Extensions.DependencyInjection provides [Microsoft.Extensions.DependencyInjection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-usage) integration for Reqnroll.

This package enables Reqnroll to use the standard .NET dependency injection container for resolving step definitions, hooks, and other dependencies in your BDD tests.

## Key Features

- **Microsoft DI Integration**: Use the standard .NET DI container in Reqnroll projects
- **Step Definition Injection**: Automatic dependency injection for step definition classes
- **Hook Injection**: Dependency injection support for before/after hooks
- **Service Collection Support**: Easy registration using IServiceCollection
- **Scope Management**: Proper scoping and lifecycle management of dependencies

## Installation

Install this package to add Microsoft.Extensions.DependencyInjection support to your Reqnroll project:

```powershell
dotnet add package Reqnroll.Microsoft.Extensions.DependencyInjection
```

## Documentation

For more information about using dependency injection with Reqnroll, visit the [Reqnroll documentation](https://docs.reqnroll.net/).

## Support

- [Reqnroll Documentation](https://docs.reqnroll.net/)
- [GitHub Issues](https://github.com/reqnroll/Reqnroll/issues)
- [Community Discussions](https://github.com/reqnroll/Reqnroll/discussions)