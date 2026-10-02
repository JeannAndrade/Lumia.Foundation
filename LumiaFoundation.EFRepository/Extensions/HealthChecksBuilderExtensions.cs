using LumiaFoundation.EFRepository.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LumiaFoundation.EFRepository.Extensions;

public static class HealthChecksBuilderExtensions
{
    public const string DefaultName = "banco-de-dados";

    // O padrão do framework (30 s) é longo demais para a sonda de um orquestrador.
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Registra a verificação de conectividade do <typeparamref name="TContext"/>.
    /// A lib não define tags: informe a tag usada pelo endpoint de readiness.
    /// </summary>
    public static IHealthChecksBuilder AddDbContextHealthCheck<TContext>(
        this IHealthChecksBuilder builder,
        string name = DefaultName,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null) where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddCheck<DbContextHealthCheck<TContext>>(name, failureStatus, tags, timeout ?? DefaultTimeout);
    }
}
