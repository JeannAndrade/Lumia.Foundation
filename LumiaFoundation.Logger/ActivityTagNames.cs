namespace LumiaFoundation.Logger;

public static class ActivityTagNames
{
    /// <summary>
    /// Nome da tag que o middleware do host grava na <see cref="System.Diagnostics.Activity"/>
    /// da requisição depois da autenticação e que o renderizador <c>lumia-ownerid</c> lê.
    /// </summary>
    public const string OwnerId = "lumia.owner_id";
}
