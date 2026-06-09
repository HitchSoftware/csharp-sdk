using System.Reflection;

namespace Garden.ModelContextProtocol;

/// <summary>
/// Exception thrown when <see cref="ServiceProviderExtensions.ResolveOrConstruct"/> 
/// is unable to resolve a required constructor parameter.
/// </summary>
public class ConstructorResolutionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructorResolutionException"/> class.
    /// </summary>
    public ConstructorResolutionException(Type implementationType, ParameterInfo parameter, Exception? innerException = null)
        : base($"Unable to resolve parameter '{parameter.Name}' of type '{parameter.ParameterType.FullName}'.", innerException)
    {
        ImplementationType = implementationType;
        Parameter = parameter;
    }

    /// <summary>
    /// Gets the type that failed to be constructed.
    /// </summary>
    public Type ImplementationType { get; }

    /// <summary>
    /// Gets the parameter that could not be resolved.
    /// </summary>
    public ParameterInfo Parameter { get; }
}
