using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Metriflow.Application;

[ServiceRegistration(
    Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped,
    typeof(IHandleMessageWithRetry)
)]
public class HandleMessageWithRetry(ILogger<HandleMessageWithRetry> logger) : IHandleMessageWithRetry
{
    private const int MaxRetryAttempts = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    public async Task<bool> HandleMessageWithRetryAsync<T>(
        T message,
        Func<T, Task> handleMessage,
        string queueName,
        CancellationToken cancellationToken
    )
    {
        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                logger.LogInformation(
                    $"Attempt {attempt}/{MaxRetryAttempts} to process message from queue {queueName}"
                );

                await handleMessage(message);

                // Success: Break the loop and return true
                logger.LogInformation(
                    "Message processed successfully on attempt {Attempt} from queue {QueueName}.",
                    attempt,
                    queueName
                );
                return true;
            }
            catch (OperationCanceledException)
            {
                // Cancellation requested - stop retrying
                logger.LogInformation(
                    "Message processing canceled for queue {QueueName}.",
                    queueName
                );
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                // Service provider is disposed, likely during shutdown - stop retrying
                logger.LogWarning(
                    ex,
                    "Service provider was disposed while processing message from queue {QueueName}. Application may be shutting down. Stopping retries.",
                    queueName
                );
                return false;
            }
            catch (Exception ex) when (attempt < MaxRetryAttempts)
            {
                // Log the failure, but only if we have more retries left
                logger.LogWarning(
                    ex,
                    "Message processing failed on attempt {Attempt} from queue {QueueName}. Retrying in {Delay} seconds...",
                    attempt,
                    queueName,
                    RetryDelay.TotalSeconds
                );

                // Wait for the delay before the next attempt
                try
                {
                    await Task.Delay(RetryDelay, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // If cancellation is requested while waiting, stop retrying
                    logger.LogInformation(
                        "Retry delay canceled for queue {QueueName}.",
                        queueName
                    );
                    return false;
                }
            }
            catch (Exception finalEx)
            {
                // Final failure after the last attempt
                logger.LogError(
                    finalEx,
                    "Final attempt {Attempt} failed to process message from queue {QueueName}. No more retries.",
                    attempt,
                    queueName
                );
                return false;
            }
        }

        return false; // Should not be reached, but included for completeness
    }
}
