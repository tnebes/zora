namespace zora.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceLifetimeAttribute : Attribute
    {
        public ServiceLifetime Lifetime { get; }

        public ServiceLifetimeAttribute(ServiceLifetime lifetime)
        {
            this.Lifetime = lifetime;
        }
    }
}
