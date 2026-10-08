using LumiaFoundation.Core.Pagination;

namespace LumiaFoundation.Core.Test.Pagination;

public class PagedListTests
{
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 10, 1)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(25, 10, 3)]
    public void TotalPages_ArredondaParaCima(int totalCount, int pageSize, int esperado)
    {
        var lista = new PagedList<int>([], 1, pageSize, totalCount);

        Assert.Equal(esperado, lista.TotalPages);
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(-1, 10, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(1, -5, 0)]
    [InlineData(1, 10, -1)]
    public void Construtor_ComValoresInvalidos_LancaArgumentOutOfRangeException(int page, int pageSize, int totalCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PagedList<int>([], page, pageSize, totalCount));
    }

    [Fact]
    public void Construtor_ComItensNulos_LancaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PagedList<int>(null!, 1, 10, 0));
    }

    [Fact]
    public void Map_ConverteItensEPreservaMetadados()
    {
        var lista = new PagedList<int>([1, 2, 3], page: 2, pageSize: 3, totalCount: 8);

        var convertida = lista.Map(n => $"item {n}");

        Assert.Equal(["item 1", "item 2", "item 3"], convertida.Items);
        Assert.Equal(2, convertida.Page);
        Assert.Equal(3, convertida.PageSize);
        Assert.Equal(8, convertida.TotalCount);
        Assert.Equal(3, convertida.TotalPages);
    }

    [Fact]
    public void Map_ComMapperNulo_LancaArgumentNullException()
    {
        var lista = new PagedList<int>([], 1, 10, 0);

        Assert.Throws<ArgumentNullException>(() => lista.Map<string>(null!));
    }
}
