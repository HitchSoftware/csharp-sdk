using Microsoft.Extensions.DependencyInjection;
using Garden.ModelContextProtocol;
using Garden.ModelContextProtocol.Server;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Provides extension methods for configuring <see cref="McpServerOptions"/> directly
/// (useful for per-route MCP configuration).
/// </summary>
public static class McpServerOptionsExtensions
{
    /// <summary>
    /// Adds <see cref="McpServerTool"/> instances to the <see cref="McpServerOptions.ToolCollection"/> by scanning the given type.
    /// Instance methods will be created using <see cref="System.Activator.CreateInstance(System.Type)"/>.
    /// </summary>
    public static McpServerOptions WithTools<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.NonPublicMethods |
        DynamicallyAccessedMemberTypes.PublicConstructors)] TToolType>(
        this McpServerOptions options,
        JsonSerializerOptions? serializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.ToolCollection ??= new();

        var toolType = typeof(TToolType);

        foreach (var toolMethod in toolType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
        {
            if (toolMethod.GetCustomAttribute<McpServerToolAttribute>() is not null)
            {
                if (toolMethod.IsStatic)
                {
                    options.ToolCollection.Add(McpServerTool.Create(toolMethod, (object?)null, new() { SerializerOptions = serializerOptions }));
                }
                else
                {
                    options.ToolCollection.Add(McpServerTool.Create(
                        toolMethod,
                        _ => Activator.CreateInstance(toolType)!,
                        new() { SerializerOptions = serializerOptions }));
                }
            }
        }

        return options;
    }

    /// <summary>
    /// Adds <see cref="McpServerTool"/> instances to the <see cref="McpServerOptions.ToolCollection"/> using the provided target instance.
    /// </summary>
    public static McpServerOptions WithTools<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.NonPublicMethods)] TToolType>(
        this McpServerOptions options,
        TToolType target,
        JsonSerializerOptions? serializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(target);

        options.ToolCollection ??= new();

        if (target is IEnumerable<McpServerTool> tools)
        {
            foreach (var tool in tools)
                options.ToolCollection.Add(tool);
            return options;
        }

        foreach (var toolMethod in typeof(TToolType).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
        {
            if (toolMethod.GetCustomAttribute<McpServerToolAttribute>() is not null)
            {
                options.ToolCollection.Add(McpServerTool.Create(toolMethod, toolMethod.IsStatic ? null : target, new() { SerializerOptions = serializerOptions }));
            }
        }

        return options;
    }

    /// <summary>
    /// Adds <see cref="McpServerTool"/> instances discovered on the given type,
    /// using the provided factory to create target instances (supports DI).
    /// </summary>
    public static McpServerOptions WithTools(
        this McpServerOptions options,
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods |
            DynamicallyAccessedMemberTypes.NonPublicMethods |
            DynamicallyAccessedMemberTypes.PublicConstructors)] Type toolType,
        Func<IServiceProvider, object> createTarget,
        JsonSerializerOptions? serializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(toolType);
        ArgumentNullException.ThrowIfNull(createTarget);

        options.ToolCollection ??= new();

        foreach (var toolMethod in toolType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
        {
            if (toolMethod.GetCustomAttribute<McpServerToolAttribute>() is not null)
            {
                if (toolMethod.IsStatic)
                {
                    options.ToolCollection.Add(McpServerTool.Create(toolMethod, (object?)null, new() { SerializerOptions = serializerOptions }));
                }
                else
                {
                    options.ToolCollection.Add(McpServerTool.Create(
                        toolMethod,
                        ctx => createTarget(ctx.Services!),
                        new() { SerializerOptions = serializerOptions }));
                }
            }
        }

        return options;
    }

    /// <summary>
    /// Adds the specified <see cref="McpServerTool"/> instances to the <see cref="McpServerOptions.ToolCollection"/>.
    /// </summary>
    public static McpServerOptions WithTools(this McpServerOptions options, IEnumerable<McpServerTool> tools)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(tools);

        options.ToolCollection ??= new();

        foreach (var tool in tools)
            options.ToolCollection.Add(tool);

        return options;
    }

    // ==================== RESOURCES ====================

    /// <summary>
    /// Adds <see cref="McpServerResource"/> instances discovered on the given type.
    /// </summary>
    public static McpServerOptions WithResources<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.NonPublicMethods |
        DynamicallyAccessedMemberTypes.PublicConstructors)] TResourceType>(
        this McpServerOptions options,
        JsonSerializerOptions? serializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.ResourceCollection ??= new();

        var resourceType = typeof(TResourceType);
        bool foundAny = false;

        foreach (var method in resourceType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
        {
            if (method.GetCustomAttribute<McpServerResourceAttribute>() is not null)
            {
                foundAny = true;
                if (method.IsStatic)
                {
                    options.ResourceCollection.Add(McpServerResource.Create(method, (object?)null, new() { SerializerOptions = serializerOptions }));
                }
                else
                {
                    options.ResourceCollection.Add(McpServerResource.Create(
                        method,
                        _ => Activator.CreateInstance(resourceType)!,
                        new() { SerializerOptions = serializerOptions }));
                }
            }
        }

        if (!foundAny)
            return options; // nothing to register

        return options;
    }

    /// <summary>
    /// Adds <see cref="McpServerResource"/> instances using the provided target instance.
    /// </summary>
    public static McpServerOptions WithResources<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.NonPublicMethods |
        DynamicallyAccessedMemberTypes.PublicConstructors)] TResourceType>(
        this McpServerOptions options,
        TResourceType target,
        JsonSerializerOptions? serializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(target);

        options.ResourceCollection ??= new();

        if (target is IEnumerable<McpServerResource> resources)
        {
            foreach (var r in resources) options.ResourceCollection.Add(r);
            return options;
        }

        var resourceType = typeof(TResourceType);
        bool foundAny = false;

        foreach (var method in resourceType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
        {
            if (method.GetCustomAttribute<McpServerResourceAttribute>() is not null)
            {
                foundAny = true;
                options.ResourceCollection.Add(McpServerResource.Create(method, method.IsStatic ? null : target, new() { SerializerOptions = serializerOptions }));
            }
        }

        if (!foundAny)
            return options; // nothing to register

        return options;
    }

    /// <summary>
    /// Adds <see cref="McpServerResource"/> instances discovered on the given type using a DI factory.
    /// </summary>
    public static McpServerOptions WithResources(
        this McpServerOptions options,
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods |
            DynamicallyAccessedMemberTypes.NonPublicMethods |
            DynamicallyAccessedMemberTypes.PublicConstructors)] Type resourceType,
        Func<IServiceProvider, object> createTarget,
        JsonSerializerOptions? serializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(createTarget);

        options.ResourceCollection ??= new();
        bool foundAny = false;

        foreach (var method in resourceType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
        {
            if (method.GetCustomAttribute<McpServerResourceAttribute>() is not null)
            {
                foundAny = true;
                if (method.IsStatic)
                {
                    options.ResourceCollection.Add(McpServerResource.Create(method, (object?)null, new() { SerializerOptions = serializerOptions }));
                }
                else
                {
                    options.ResourceCollection.Add(McpServerResource.Create(
                        method,
                        ctx => createTarget(ctx.Services!),
                        new() { SerializerOptions = serializerOptions }));
                }
            }
        }

        if (!foundAny)
            return options; // nothing to register

        return options;
    }

    /// <summary>
    /// Adds the specified <see cref="McpServerResource"/> instances.
    /// </summary>
    public static McpServerOptions WithResources(this McpServerOptions options, IEnumerable<McpServerResource> resources)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(resources);

        options.ResourceCollection ??= new();
        foreach (var r in resources)
            options.ResourceCollection.Add(r);

        return options;
    }

    // ==================== PROMPTS ====================

    /// <summary>
    /// Adds <see cref="McpServerPrompt"/> instances discovered on the given type.
    /// </summary>
    public static McpServerOptions WithPrompts<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.NonPublicMethods |
        DynamicallyAccessedMemberTypes.PublicConstructors)] TPromptType>(
        this McpServerOptions options,
        JsonSerializerOptions? serializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.PromptCollection ??= new();

        var promptType = typeof(TPromptType);
        bool foundAny = false;

        foreach (var method in promptType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
        {
            if (method.GetCustomAttribute<McpServerPromptAttribute>() is not null)
            {
                foundAny = true;
                if (method.IsStatic)
                {
                    options.PromptCollection.Add(McpServerPrompt.Create(method, (object?)null, new() { SerializerOptions = serializerOptions }));
                }
                else
                {
                    options.PromptCollection.Add(McpServerPrompt.Create(
                        method,
                        _ => Activator.CreateInstance(promptType)!,
                        new() { SerializerOptions = serializerOptions }));
                }
            }
        }

        if (!foundAny)
            return options; // nothing to register

        return options;
    }

    /// <summary>
    /// Adds <see cref="McpServerPrompt"/> instances using the provided target instance.
    /// </summary>
    public static McpServerOptions WithPrompts<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.NonPublicMethods |
        DynamicallyAccessedMemberTypes.PublicConstructors)] TPromptType>(
        this McpServerOptions options,
        TPromptType target,
        JsonSerializerOptions? serializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(target);

        options.PromptCollection ??= new();

        if (target is IEnumerable<McpServerPrompt> prompts)
        {
            foreach (var p in prompts) options.PromptCollection.Add(p);
            return options;
        }

        var promptType = typeof(TPromptType);
        bool foundAny = false;

        foreach (var method in promptType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
        {
            if (method.GetCustomAttribute<McpServerPromptAttribute>() is not null)
            {
                foundAny = true;
                options.PromptCollection.Add(McpServerPrompt.Create(method, method.IsStatic ? null : target, new() { SerializerOptions = serializerOptions }));
            }
        }

        if (!foundAny)
            return options; // nothing to register

        return options;
    }

    /// <summary>
    /// Adds <see cref="McpServerPrompt"/> instances discovered on the given type using a DI factory.
    /// </summary>
    public static McpServerOptions WithPrompts(
        this McpServerOptions options,
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods |
            DynamicallyAccessedMemberTypes.NonPublicMethods |
            DynamicallyAccessedMemberTypes.PublicConstructors)] Type promptType,
        Func<IServiceProvider, object> createTarget,
        JsonSerializerOptions? serializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(promptType);
        ArgumentNullException.ThrowIfNull(createTarget);

        options.PromptCollection ??= new();
        bool foundAny = false;

        foreach (var method in promptType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
        {
            if (method.GetCustomAttribute<McpServerPromptAttribute>() is not null)
            {
                foundAny = true;
                if (method.IsStatic)
                {
                    options.PromptCollection.Add(McpServerPrompt.Create(method, (object?)null, new() { SerializerOptions = serializerOptions }));
                }
                else
                {
                    options.PromptCollection.Add(McpServerPrompt.Create(
                        method,
                        ctx => createTarget(ctx.Services!),
                        new() { SerializerOptions = serializerOptions }));
                }
            }
        }

        if (!foundAny)
            return options; // nothing to register

        return options;
    }

    /// <summary>
    /// Adds the specified <see cref="McpServerPrompt"/> instances.
    /// </summary>
    public static McpServerOptions WithPrompts(this McpServerOptions options, IEnumerable<McpServerPrompt> prompts)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(prompts);

        options.PromptCollection ??= new();
        foreach (var p in prompts)
            options.PromptCollection.Add(p);

        return options;
    }

    // ==================== PROVIDER-BASED CONVENIENCE OVERLOADS ====================

    /// <summary>
    /// Registers tools from the specified type using the given <see cref="IServiceProvider"/> for constructor resolution.
    /// </summary>
    public static McpServerOptions WithToolsFromProvider(
        this McpServerOptions options,
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods |
            DynamicallyAccessedMemberTypes.NonPublicMethods |
            DynamicallyAccessedMemberTypes.PublicConstructors)] Type toolType,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(toolType);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var instance = serviceProvider.ResolveOrConstruct(toolType)
            ?? throw new InvalidOperationException($"Failed to resolve/construct {toolType}");
        return options.WithTools(toolType, _ => instance);
    }

    /// <summary>
    /// Registers resources from the specified type using the given <see cref="IServiceProvider"/> for constructor resolution.
    /// </summary>
    public static McpServerOptions WithResourcesFromProvider(
        this McpServerOptions options,
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods |
            DynamicallyAccessedMemberTypes.NonPublicMethods |
            DynamicallyAccessedMemberTypes.PublicConstructors)] Type resourceType,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var instance = serviceProvider.ResolveOrConstruct(resourceType)
            ?? throw new InvalidOperationException($"Failed to resolve/construct {resourceType}");
        return options.WithResources(resourceType, _ => instance);
    }

    /// <summary>
    /// Registers prompts from the specified type using the given <see cref="IServiceProvider"/> for constructor resolution.
    /// </summary>
    public static McpServerOptions WithPromptsFromProvider(
        this McpServerOptions options,
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods |
            DynamicallyAccessedMemberTypes.NonPublicMethods |
            DynamicallyAccessedMemberTypes.PublicConstructors)] Type promptType,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(promptType);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var instance = serviceProvider.ResolveOrConstruct(promptType)
            ?? throw new InvalidOperationException($"Failed to resolve/construct {promptType}");
        return options.WithPrompts(promptType, _ => instance);
    }

    /// <summary>
    /// Registers tools, resources, and prompts from the specified type using the given <see cref="IServiceProvider"/>.
    /// This is the recommended method for registering types like <c>BaseMcpEntity&lt;T&gt;</c>.
    /// </summary>
    public static McpServerOptions WithAllFromProvider(
        this McpServerOptions options,
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods |
            DynamicallyAccessedMemberTypes.NonPublicMethods |
            DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType,
        IServiceProvider serviceProvider)
    {
        options.WithToolsFromProvider(implementationType, serviceProvider);
        options.WithResourcesFromProvider(implementationType, serviceProvider);
        options.WithPromptsFromProvider(implementationType, serviceProvider);

        return options;
    }
}
