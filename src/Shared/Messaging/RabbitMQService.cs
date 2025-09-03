using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Shared.Messaging;

public class RabbitMQService : IMessagePublisher, IMessageConsumer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQService> _logger;
    private readonly List<string> _consumingQueues = new();

    public RabbitMQService(ILogger<RabbitMQService> logger, string connectionString = "amqp://localhost")
    {
        _logger = logger;
        
        try
        {
            var factory = new ConnectionFactory() { Uri = new Uri(connectionString) };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            _logger.LogInformation("Connected to RabbitMQ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ");
            throw;
        }
    }

    public async Task PublishAsync<T>(string queueName, T message) where T : class
    {
        try
        {
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            
            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);
            
            _logger.LogInformation("Message published to queue {QueueName}: {Message}", queueName, json);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to queue {QueueName}", queueName);
            throw;
        }
    }

    public async Task StartConsumingAsync<T>(string queueName, Func<T, Task> handler) where T : class
    {
        try
        {
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            
            var consumer = new EventingBasicConsumer(_channel);
            
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<T>(json);
                    
                    if (message != null)
                    {
                        await handler(message);
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        _logger.LogInformation("Message processed from queue {QueueName}: {Message}", queueName, json);
                    }
                    else
                    {
                        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                        _logger.LogWarning("Failed to deserialize message from queue {QueueName}: {Json}", queueName, json);
                    }
                }
                catch (Exception ex)
                {
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    _logger.LogError(ex, "Error processing message from queue {QueueName}", queueName);
                }
            };
            
            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            _consumingQueues.Add(queueName);
            
            _logger.LogInformation("Started consuming messages from queue {QueueName}", queueName);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start consuming from queue {QueueName}", queueName);
            throw;
        }
    }

    public void StopConsuming()
    {
        try
        {
            foreach (var queueName in _consumingQueues)
            {
                _channel.BasicCancel(queueName);
                _logger.LogInformation("Stopped consuming from queue {QueueName}", queueName);
            }
            _consumingQueues.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping consumption");
        }
    }

    public void Dispose()
    {
        try
        {
            StopConsuming();
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
            _logger.LogInformation("RabbitMQ connection disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ connection");
        }
    }
}
