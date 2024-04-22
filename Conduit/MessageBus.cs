using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Conduit;

public class MessageBus
{
    private readonly Subject<object> _subject = new();

    public IObservable<T> OfType<T>()
    {
        return _subject.OfType<T>();
    }

    public void Publish<T>(T message)
    {
        if (message is not null)
        {
            _subject.OnNext(message);
        }
    }
}