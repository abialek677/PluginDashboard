namespace Contracts;

public interface IEventAggregator
{
    void Publish<T>(T ev);
    void Subscribe<T>(Action<T> handler);
}