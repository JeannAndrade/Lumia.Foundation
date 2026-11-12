namespace LumiaFoundation.AspNetCore.Pagination;

/// <summary>
/// Configurações do <see cref="PaginationTagHelper"/>. Os padrões usam Bootstrap 5 e textos em português.
/// </summary>
public sealed class PaginationOptions
{
    /// <summary>Nome do parâmetro de querystring que carrega o número da página.</summary>
    public string PageParameterName { get; set; } = "pagina";

    /// <summary>Quantidade de páginas exibidas de cada lado da página atual.</summary>
    public int SiblingCount { get; set; } = 2;

    public string NavigationLabel { get; set; } = "Paginação";
    public string PreviousLabel { get; set; } = "Anterior";
    public string NextLabel { get; set; } = "Próxima";

    public string ListClass { get; set; } = "pagination";
    public string ItemClass { get; set; } = "page-item";
    public string LinkClass { get; set; } = "page-link";
    public string ActiveClass { get; set; } = "active";
    public string DisabledClass { get; set; } = "disabled";
}
