namespace LumiaFoundation.AspNetCore.ClientAuthentication;

public sealed record RegistrationResult(RegistrationOutcome Outcome, string? ErrorMessage = null)
{
    public static RegistrationResult Succeeded() => new(RegistrationOutcome.Succeeded);

    /// <param name="message">Mensagem devolvida pela API (por exemplo, "usuário já existe"), segura para mostrar ao usuário final.</param>
    public static RegistrationResult Rejected(string message) => new(RegistrationOutcome.Rejected, message);

    public static RegistrationResult Unavailable() => new(RegistrationOutcome.Unavailable);
}
