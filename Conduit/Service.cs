namespace Conduit;

public interface Service<T> where T : Service<T>
{
    public static abstract string Name { get; } 

    static virtual string Endpoint => $"https://{T.Name}";
}
