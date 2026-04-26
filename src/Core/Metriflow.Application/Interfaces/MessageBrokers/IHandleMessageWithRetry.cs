namespace Metriflow.Application.Interfaces;

public interface IHandleMessageWithRetry
{
    Task<bool> HandleMessageWithRetryAsync<T>(
        T message,
        Func<T, Task> handleMessage,
        string queueName,
        CancellationToken cancellationToken
    );
}
