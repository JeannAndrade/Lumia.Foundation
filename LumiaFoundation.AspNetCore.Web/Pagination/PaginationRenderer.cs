using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LumiaFoundation.AspNetCore.Pagination;

internal static class PaginationRenderer
{
    private const string Reticencias = "…";

    internal static IHtmlContent Render(int page, int totalPages, PaginationOptions options, Func<int, string> urlFactory)
    {
        var lista = new TagBuilder("ul");
        lista.AddCssClass(options.ListClass);

        // Com página além da última, "Anterior" volta para a última página real.
        int? anterior = page > 1 ? Math.Min(page - 1, totalPages) : null;
        int? proxima = page < totalPages ? page + 1 : null;

        lista.InnerHtml.AppendHtml(CriarItem(options, options.PreviousLabel, anterior, ativo: false, urlFactory));

        foreach (var numero in PaginationWindow.Build(page, totalPages, options.SiblingCount))
        {
            lista.InnerHtml.AppendHtml(numero is { } n
                ? CriarItem(options, n.ToString(CultureInfo.InvariantCulture), n, ativo: n == page, urlFactory)
                : CriarItem(options, Reticencias, destino: null, ativo: false, urlFactory));
        }

        lista.InnerHtml.AppendHtml(CriarItem(options, options.NextLabel, proxima, ativo: false, urlFactory));

        return lista;
    }

    // TagBuilder codifica texto e atributos: nada vindo da requisição chega ao HTML sem escape.
    private static TagBuilder CriarItem(
        PaginationOptions options, string texto, int? destino, bool ativo, Func<int, string> urlFactory)
    {
        var item = new TagBuilder("li");
        item.AddCssClass(options.ItemClass);

        TagBuilder conteudo;
        if (destino is { } pagina && !ativo)
        {
            conteudo = new TagBuilder("a");
            conteudo.Attributes["href"] = urlFactory(pagina);
        }
        else
        {
            conteudo = new TagBuilder("span");
            item.AddCssClass(ativo ? options.ActiveClass : options.DisabledClass);
            if (ativo)
                item.Attributes["aria-current"] = "page";
        }

        conteudo.AddCssClass(options.LinkClass);
        conteudo.InnerHtml.Append(texto);
        item.InnerHtml.AppendHtml(conteudo);

        return item;
    }
}
