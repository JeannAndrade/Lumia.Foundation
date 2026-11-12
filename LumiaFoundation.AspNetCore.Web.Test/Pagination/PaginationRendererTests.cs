using System.Text.Encodings.Web;
using System.Text.Unicode;
using LumiaFoundation.AspNetCore.Pagination;

namespace LumiaFoundation.AspNetCore.Test.Pagination;

public class PaginationRendererTests
{
    [Fact]
    public void Render_NaPrimeiraPagina_DesabilitaAnteriorEMarcaAtual()
    {
        var html = Renderizar(page: 1, totalPages: 3);

        Assert.Contains("""<li class="page-item disabled"><span class="page-link">Anterior</span></li>""", html);
        Assert.Contains("""aria-current="page" """.TrimEnd(), html);
        Assert.Contains("""<span class="page-link">1</span>""", html);
        Assert.Contains("""<a class="page-link" href="/lista?pagina=2">2</a>""", html);
        Assert.Contains("""<a class="page-link" href="/lista?pagina=2">Próxima</a>""", html);
        Assert.DoesNotContain("""href="/lista?pagina=1" """.TrimEnd(), html);
    }

    [Fact]
    public void Render_NaUltimaPagina_DesabilitaProximaELinkaAnterior()
    {
        var html = Renderizar(page: 3, totalPages: 3);

        Assert.Contains("""<a class="page-link" href="/lista?pagina=2">Anterior</a>""", html);
        Assert.Contains("""<li class="page-item disabled"><span class="page-link">Próxima</span></li>""", html);
    }

    [Fact]
    public void Render_NoMeio_LinkaAnteriorEProxima()
    {
        var html = Renderizar(page: 3, totalPages: 5);

        Assert.Contains("""<a class="page-link" href="/lista?pagina=2">Anterior</a>""", html);
        Assert.Contains("""<a class="page-link" href="/lista?pagina=4">Próxima</a>""", html);
    }

    [Fact]
    public void Render_ComTrechoOmitido_MostraReticenciasDesabilitadas()
    {
        var html = Renderizar(page: 1, totalPages: 10);

        Assert.Contains("""<li class="page-item disabled"><span class="page-link">…</span></li>""", html);
    }

    [Fact]
    public void Render_ComPaginaAlemDaUltima_AnteriorApontaParaUltimaENenhumaFicaAtual()
    {
        var html = Renderizar(page: 7, totalPages: 3);

        Assert.Contains("""<a class="page-link" href="/lista?pagina=3">Anterior</a>""", html);
        Assert.Contains("""<li class="page-item disabled"><span class="page-link">Próxima</span></li>""", html);
        Assert.DoesNotContain("aria-current", html);
    }

    [Fact]
    public void Render_ComClassesCustomizadas_UsaAsClassesDasOpcoes()
    {
        var opcoes = new PaginationOptions { ListClass = "pagination pagination-sm", LinkClass = "link" };

        var html = Renderizar(page: 1, totalPages: 3, opcoes);

        Assert.Contains("""<ul class="pagination pagination-sm">""", html);
        Assert.Contains("""<span class="link">1</span>""", html);
    }

    private static string Renderizar(int page, int totalPages, PaginationOptions? opcoes = null)
    {
        var conteudo = PaginationRenderer.Render(
            page, totalPages, opcoes ?? new PaginationOptions(), pagina => $"/lista?pagina={pagina}");

        using var escritor = new StringWriter();
        conteudo.WriteTo(escritor, HtmlEncoder.Create(UnicodeRanges.All));

        return escritor.ToString();
    }
}
