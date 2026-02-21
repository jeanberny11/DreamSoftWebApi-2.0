using System.Reflection;
using AutoMapper;

namespace DreamSoft.Application.Common.Mappings;

/// <summary>
/// AutoMapper <see cref="Profile"/> that auto-discovers and registers every
/// class in the Application assembly that implements <see cref="IMapFrom{T}"/>.
///
/// How it works:
///   1. At startup AutoMapper calls <see cref="Profile"/> constructors.
///   2. This constructor scans the executing assembly for concrete types
///      that implement <see cref="IMapFrom{T}"/>.
///   3. For each found type it creates a temporary instance and calls
///      <see cref="IMapFrom{T}.Mapping"/> so the type can register its own map.
///   4. Types that rely on the default interface implementation get a plain
///      <c>CreateMap&lt;TSource, TDestination&gt;()</c> for free; types that
///      override <c>Mapping()</c> get full control over the configuration.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        // Find every concrete (non-abstract, non-interface) type that
        // closes the IMapFrom<T> generic interface at least once.
        var mapFromInterfaceType = typeof(IMapFrom<>);

        var types = assembly.GetExportedTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == mapFromInterfaceType))
            .ToList();

        foreach (var type in types)
        {
            // A single DTO can implement IMapFrom<T> more than once
            // (e.g., map from two different source types).
            var mapFromInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == mapFromInterfaceType);

            foreach (var iface in mapFromInterfaces)
            {
                // Resolve the Mapping(Profile) method — could be the default
                // interface implementation or an explicit override on the type.
                var mappingMethod = iface.GetMethod(nameof(IMapFrom<object>.Mapping))
                                    ?? type.GetMethod(nameof(IMapFrom<object>.Mapping));

                if (mappingMethod is null)
                    continue;

                // Create a lightweight instance just to invoke Mapping().
                // DTOs typically have parameterless constructors; if not,
                // RuntimeHelpers.GetUninitializedObject is the fallback.
                var instance = TryCreateInstance(type);
                if (instance is null)
                    continue;

                mappingMethod.Invoke(instance, [this]);
            }
        }
    }

    /// <summary>
    /// Attempts to create an instance of <paramref name="type"/> without
    /// requiring constructor parameters, so we can call <c>Mapping()</c> on it.
    /// </summary>
    private static object? TryCreateInstance(Type type)
    {
        try
        {
            // Fast path: parameterless constructor
            return Activator.CreateInstance(type);
        }
        catch
        {
            try
            {
                // Fallback: bypass the constructor entirely (no field init)
                return System.Runtime.CompilerServices.RuntimeHelpers
                    .GetUninitializedObject(type);
            }
            catch
            {
                return null;
            }
        }
    }
}
