using System.Text.Encodings.Web;
using System.Text.Unicode;
using LumiaFoundation.AspNetCore.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;

namespace LumiaFoundation.AspNetCore.Test.Pagination;

public class PaginationTagHelperTests
{
    private static readonly HtmlEncoder Encoder = HtmlEncoder.Create(UnicodeRanges.All);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Process_ComUmaPaginaOuMenos_NaoRenderizaNada(int totalPages)
    {
        var (tagHelper, context, output) = Criar(page: 1, totalPages: totalPages);

        tagHelper.Process(context, output);

        Assert.Null(output.TagName);
        Assert.True(output.Content.IsEmptyOrWhiteSpace);
    }

    [Fact]
    public void Process_RenderizaNavComRotuloELinks()
    {
        var (tagHelper, context, output) = Criar(page: 2, totalPages: 3, path: "/Movimentos");

        tagHelper.Process(context, output);

        Assert.Equal("nav", output.TagName);
        Assert.Equal("Paginação", output.Attributes["aria-label"].Value);
        Assert.Contains("""href="/Movimentos?pagina=3" """.TrimEnd(), output.Content.GetContent(Encoder));
    }

    [Fact]
    public void Process_PreservaOutrosParametrosEPathBaseETrocaOParametroDaPagina()
    {
        var (tagHelper, context, output) = Criar(
            page: 3, totalPages: 5, path: "/Movimentos", pathBase: "/app",
            query: "?ordem=data&ordem=valor&pagina=3");

        tagHelper.Process(context, output);

        var html = output.Content.GetContent(Encoder);
        Assert.Contains("""href="/app/Movimentos?ordem=data&amp;ordem=valor&amp;pagina=2" """.TrimEnd(), html);
        Assert.DoesNotContain("pagina=3", html);
    }

    [Fact]
    public void Process_ComNomeDeParametroCustomizado_UsaONomeEPreservaOParametroPadrao()
    {
        var opcoes = new PaginationOptions { PageParameterName = "p" };
        var (tagHelper, context, output) = Criar(
            page: 1, totalPages: 3, path: "/Movimentos", query: "?pagina=9", opcoes: opcoes);

        tagHelper.Process(context, output);

        Assert.Contains("""href="/Movimentos?pagina=9&amp;p=2" """.TrimEnd(), output.Content.GetContent(Encoder));
    }

    [Fact]
    public void Process_RemoveOParametroDaPaginaIgnorandoMaiusculas()
    {
        var (tagHelper, context, output) = Criar(page: 3, totalPages: 5, path: "/Movimentos", query: "?PAGINA=3");

        tagHelper.Process(context, output);

        Assert.DoesNotContain("PAGINA", output.Content.GetContent(Encoder));
    }

    private static (PaginationTagHelper TagHelper, TagHelperContext Context, TagHelperOutput Output) Criar(
        int page,
        int totalPages,
        string path = "/lista",
        string pathBase = "",
        string query = "",
        PaginationOptions? opcoes = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.PathBase = pathBase;
        httpContext.Request.Path = path;
        httpContext.Request.QueryString = new QueryString(query);

        var viewContext = new ViewContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            Mock.Of<IView>(),
            new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
            Mock.Of<ITempDataDictionary>(),
            TextWriter.Null,
            new HtmlHelperOptions());

        var tagHelper = new PaginationTagHelper(Options.Create(opcoes ?? new PaginationOptions()))
        {
            Page = page,
            TotalPages = totalPages,
            ViewContext = viewContext
        };

        var context = new TagHelperContext(
            "lumia-pagination", new TagHelperAttributeList(), new Dictionary<object, object>(), Guid.NewGuid().ToString("N"));
        var output = new TagHelperOutput(
            "lumia-pagination",
            new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        return (tagHelper, context, output);
    }
}
