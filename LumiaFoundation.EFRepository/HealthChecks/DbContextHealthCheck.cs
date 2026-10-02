using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LumiaFoundation.EFRepository.HealthChecks;

/// <summary>
/// Verifica se o <typeparamref name="TContext"/> consegue se conectar ao banco de dados.
/// Pensada para o endpoint de readiness: indica se a aplicação já pode receber requisições.
/// </summary>
public class DbContextHealthCheck<TContext>(TContext dbContext) : IHealthCheck where TContext : DbContext
{
    private const string UnavailableMessage = "Banco de dados indisponível.";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Garante o cancelamento em qualquer provider (o InMemory, por exemplo, ignora o token).
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy(UnavailableMessage);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // A descrição é genérica de propósito; o detalhe fica apenas em Exception.
            return HealthCheckResult.Unhealthy(UnavailableMessage, exception);
        }
    }
}
