using AutoMapper;

namespace DreamSoft.Application.Common.Mappings;

/// <summary>
/// Marker interface for self-registering AutoMapper mappings.
/// Implement this on any DTO/response class to automatically create a
/// default map from <typeparamref name="T"/> to the implementing type.
/// </summary>
/// <typeparam name="T">The source type to map from (typically a domain entity).</typeparam>
/// <example>
/// <code>
/// public class UserDto : IMapFrom&lt;User&gt;
/// {
///     public int Id { get; set; }
///     public string Username { get; set; } = null!;
///
///     // Override Mapping() for custom configuration
///     public void Mapping(Profile profile)
///         => profile.CreateMap&lt;User, UserDto&gt;()
///                   .ForMember(d => d.FullName, o => o.MapFrom(s => s.GetFullName()));
/// }
/// </code>
/// </example>
public interface IMapFrom<T>
{
    /// <summary>
    /// Override this method to customise the AutoMapper mapping configuration.
    /// The default implementation creates a straight <c>CreateMap&lt;T, TDto&gt;()</c>
    /// with no special options, which is sufficient for most DTOs.
    /// </summary>
    /// <param name="profile">The AutoMapper <see cref="Profile"/> to configure.</param>
    void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
}
