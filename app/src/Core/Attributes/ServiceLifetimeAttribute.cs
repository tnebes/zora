namespace zora.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ServiceLifetimeAttribute : Attribute
{
    public ServiceLifetimeAttribute(ServiceLifetime lifetime) => this.Lifetime = lifetime;
    public ServiceLifetime Lifetime { get; }
}
