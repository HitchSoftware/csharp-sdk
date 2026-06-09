using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Garden.ModelContextProtocol;

/// <summary>
/// Provides extension methods for <see cref="IServiceProvider"/> to support dynamic constructor argument building.
/// </summary>
public static class ServiceProviderExtensions {
    /// <summary>
    /// Attempts to resolve the type from the <see cref="IServiceProvider"/>. 
    /// If not registered, falls back to constructing it using explicit arguments and the service provider.
    /// </summary>
    public static object? ResolveOrConstruct(
        this IServiceProvider serviceProvider,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType,
        params object[] explicitArgs) {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(implementationType);

        // 1. Try direct resolution from IServiceProvider first
        object? existingInstance = null;
        try {
            existingInstance = serviceProvider.GetService(implementationType);
        }
        catch (InvalidOperationException) {
            // This can happen when trying to resolve a scoped service from the root provider.
            // In that case, we fall back to manual construction.
            existingInstance = null;
        }

        if (existingInstance != null) {
            return existingInstance;
        }

        // 2. Fall back to manual construction
        var constructors = implementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        if (constructors.Length == 0) {
            throw new InvalidOperationException($"Type '{implementationType.FullName}' has no public instance constructors.");
        }

        // Prefer the constructor with the most parameters
        var constructor = constructors
            .OrderByDescending(c => c.GetParameters().Length)
            .First();

        var parameters = constructor.GetParameters();
        var arguments = new object[parameters.Length];
        var explicitArgIndex = 0;

        for (int i = 0; i < parameters.Length; i++) {
            var parameter = parameters[i];
            object? argument = null;

            // Try to match an explicit argument first
            if (explicitArgIndex < explicitArgs.Length &&
                explicitArgs[explicitArgIndex] != null &&
                parameter.ParameterType.IsAssignableFrom(explicitArgs[explicitArgIndex].GetType())) {
                argument = explicitArgs[explicitArgIndex];
                explicitArgIndex++;
            }
            //else {
            //// Special handling for ILogger<T>
            //if (parameter.ParameterType.IsGenericType &&
            //    parameter.ParameterType.GetGenericTypeDefinition() == typeof(ILogger<>)) {
            //    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            //    // Create ILogger<T> using the non-generic method + ActivatorUtilities will handle it

            //    // DREWS TEST
            //    // argument = loggerFactory.CreateLogger(implementationType);
            //    argument = serviceProvider.GetRequiredService(parameter.ParameterType);
            //}
            //else if (parameter.ParameterType == typeof(ILogger)) {
            //    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            //    argument = loggerFactory.CreateLogger(implementationType);
            //}
            else {
                try {
                    argument = serviceProvider.GetRequiredService(parameter.ParameterType);
                }
                catch (InvalidOperationException ex) {
                    if (parameter.IsOptional) {
                        argument = parameter.DefaultValue;
                    }
                    else {
                        throw new ConstructorResolutionException(implementationType, parameter, ex);
                    }
                }
            }
            //}

            if (argument == null && !parameter.IsOptional) {
                throw new ConstructorResolutionException(implementationType, parameter);
            }

            arguments[i] = argument!;
        }

        if (explicitArgIndex < explicitArgs.Length) {
            var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger("Garden.ModelContextProtocol");
            logger?.LogDebug("Unused explicit arguments provided when constructing {Type}", implementationType);
        }

        try {
            return ActivatorUtilities.CreateInstance(serviceProvider, implementationType, arguments);
        }
        catch (InvalidOperationException ioe) {
            var message = ioe.Message;
            // ActivatorUtilities couldn't match the constructor.
            // Try one more time with explicit logger creation for generic ILogger<T>
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(implementationType);

            // Rebuild arguments with the logger in the correct position if needed
            // For now, just re-throw with better context
            throw new ConstructorResolutionException(implementationType, parameters[0],
                new InvalidOperationException("Could not match constructor arguments, including generic ILogger<T>"));
        }
    }
}
