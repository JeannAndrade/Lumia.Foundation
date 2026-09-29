
namespace LumiaFoundation.Http.Client.Authentication;

/// <summary>Dados aceitos pelo endpoint de registro do Lumia.Foundation.Auth.</summary>
/// <remarks>
/// Não inclui atribuição de roles: expor isso em um formulário de autocadastro público seria
/// uma falha de segurança. <see cref="Email"/> é obrigatório aqui — diferente do DTO da API,
/// que aceita nulo — porque a configuração padrão do Identity (<c>RequireUniqueEmail</c>) faz
/// o registro falhar sem ele.
/// </remarks>
public sealed record UserRegistration(
    string FirstName,
    string LastName,
    string UserName,
    string Password,
    string Email,
    string? PhoneNumber = null);
