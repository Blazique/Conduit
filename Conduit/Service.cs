using Conduit;

namespace Conduit;

public interface Service<TInterface, TComponent> 
    where TInterface : Service<TInterface, TComponent> 
    where TComponent : Component<TComponent>
{
    public static abstract string Name { get; }

    public static abstract string Description { get; }
}   