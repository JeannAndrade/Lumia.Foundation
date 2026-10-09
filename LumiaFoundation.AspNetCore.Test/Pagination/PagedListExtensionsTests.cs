using LumiaFoundation.AspNetCore.Pagination;
using LumiaFoundation.Core.Pagination;

namespace LumiaFoundation.AspNetCore.Test.Pagination;

public class PagedListExtensionsTests
{
    [Fact]
    public void ToPagedResponse_PreservaItensEMetadados()
    {
        var lista = new PagedList<string>(["a", "b"], page: 3, pageSize: 2, totalCount: 5);

        var resposta = lista.ToPagedResponse();

        Assert.Equal(["a", "b"], resposta.Items);
        Assert.Equal(3, resposta.Page);
        Assert.Equal(2, resposta.PageSize);
        Assert.Equal(5, resposta.TotalCount);
        Assert.Equal(3, resposta.TotalPages);
    }

    [Fact]
    public void ToPagedResponse_ComListaNula_LancaArgumentNullException()
    {
        PagedList<string> lista = null!;

        Assert.Throws<ArgumentNullException>(() => lista.ToPagedResponse());
    }
}
