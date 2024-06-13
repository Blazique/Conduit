namespace Conduit;

public interface Component<TComponent> 
    where TComponent : Component<TComponent>
{
    public static abstract string Name { get; }

    public static abstract string Description { get; }

}
