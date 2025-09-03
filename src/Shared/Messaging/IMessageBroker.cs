namespace Shared.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string queueName, T message) where T : class;
}

public interface IMessageConsumer
{
    Task StartConsumingAsync<T>(string queueName, Func<T, Task> handler) where T : class;
    void StopConsuming();
}
