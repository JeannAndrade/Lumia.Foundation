using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace LumiaFoundation.AspNetCore.Pagination;

/// <summary>
/// Renderiza a navegação entre páginas: <c>&lt;lumia-pagination page="..." total-pages="..." /&gt;</c>.
/// Nada é renderizado quando há uma página ou menos.
/// </summary>
[HtmlTargetElement("lumia-pagination")]
public sealed class PaginationTagHelper(IOptions<PaginationOptions> options) : TagHelper
{
    /// <summary>Página atual, começando em 1.</summary>
    [HtmlAttributeName("page")]
    public int Page { get; set; }

    [HtmlAttributeName("total-pages")]
    public int TotalPages { get; set; }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (TotalPages <= 1)
        {
            output.SuppressOutput();
            return;
        }

        var configuracao = options.Value;

        output.TagName = "nav";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("aria-label", configuracao.NavigationLabel);
        output.Content.SetHtmlContent(PaginationRenderer.Render(
            Page, TotalPages, configuracao, CriarFabricaDeUrls(configuracao.PageParameterName)));
    }

    // Mantém path e demais parâmetros da requisição; troca só o parâmetro da página.
    // Mantém path e demais parâmetros da requisição; troca só o parâmetro da página.
    // Em métodos de escrita (POST, PUT, PATCH, DELETE) a querystring descreve a ação
    // (ex.: ?handler=Delete&id=...), não a listagem: reaproveitá-la quebraria a navegação.
    private Func<int, string> CriarFabricaDeUrls(string parametro)
    {
        var request = ViewContext.HttpContext.Request;
        var caminho = $"{request.PathBase}{request.Path}";

        var metodoDeEscrita = HttpMethods.IsPost(request.Method)
            || HttpMethods.IsPut(request.Method)
            || HttpMethods.IsPatch(request.Method)
            || HttpMethods.IsDelete(request.Method);

        var preservados = metodoDeEscrita
            ? []
            : request.Query
                .Where(par => !string.Equals(par.Key, parametro, StringComparison.OrdinalIgnoreCase))
                .SelectMany(par => par.Value.Select(valor => KeyValuePair.Create(par.Key, valor ?? string.Empty)))
                .ToList();

        return pagina =>
        {
            var consulta = new QueryBuilder(preservados)
            {
                { parametro, pagina.ToString(CultureInfo.InvariantCulture) }
            };

            return $"{caminho}{consulta.ToQueryString()}";
        };
    }
}
