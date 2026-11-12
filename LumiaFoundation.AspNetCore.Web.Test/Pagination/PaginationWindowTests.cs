using LumiaFoundation.AspNetCore.Pagination;

namespace LumiaFoundation.AspNetCore.Test.Pagination;

public class PaginationWindowTests
{
    [Theory]
    [InlineData(1, 10, 2, "1 2 3 … 10")]
    [InlineData(5, 10, 2, "1 2 3 4 5 6 7 … 10")]
    [InlineData(6, 10, 2, "1 … 4 5 6 7 8 9 10")]
    [InlineData(10, 10, 2, "1 … 8 9 10")]
    [InlineData(1, 3, 2, "1 2 3")]
    [InlineData(2, 2, 2, "1 2")]
    [InlineData(1, 1, 2, "1")]
    [InlineData(3, 100, 0, "1 2 3 … 100")]
    [InlineData(4, 10, -3, "1 … 4 … 10")]
    public void Build_RetornaJanelaComReticencias(int page, int totalPages, int siblingCount, string esperado)
    {
        var janela = PaginationWindow.Build(page, totalPages, siblingCount);

        Assert.Equal(esperado, Formatar(janela));
    }

    [Fact]
    public void Build_ComPaginaMuitoAlemDoTotal_NaoEstouraEMostraPrimeiraEUltima()
    {
        var janela = PaginationWindow.Build(int.MaxValue, 10, 2);

        Assert.Equal("1 … 10", Formatar(janela));
    }

    private static string Formatar(IReadOnlyList<int?> janela) =>
        string.Join(' ', janela.Select(pagina => pagina?.ToString() ?? "…"));
}
