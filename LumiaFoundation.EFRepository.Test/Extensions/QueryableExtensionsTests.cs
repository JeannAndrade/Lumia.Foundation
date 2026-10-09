using LumiaFoundation.Core.Domain;
using LumiaFoundation.EFRepository.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LumiaFoundation.EFRepository.Test.Extensions;

public class QueryableExtensionsTests
{
    [Fact]
    public async Task ToPagedListAsync_PrimeiraPagina_RetornaItensEMetadados()
    {
        await using var contexto = await CriarContextoComItensAsync(25);

        var resultado = await Ordenada(contexto).ToPagedListAsync(page: 1, pageSize: 10);

        Assert.Equal(Enumerable.Range(1, 10), resultado.Items.Select(i => i.Ordem));
        Assert.Equal(1, resultado.Page);
        Assert.Equal(10, resultado.PageSize);
        Assert.Equal(25, resultado.TotalCount);
        Assert.Equal(3, resultado.TotalPages);
    }

    [Fact]
    public async Task ToPagedListAsync_UltimaPaginaParcial_RetornaSomenteOsItensRestantes()
    {
        await using var contexto = await CriarContextoComItensAsync(25);

        var resultado = await Ordenada(contexto).ToPagedListAsync(page: 3, pageSize: 10);

        Assert.Equal(Enumerable.Range(21, 5), resultado.Items.Select(i => i.Ordem));
    }

    [Fact]
    public async Task ToPagedListAsync_PaginaAlemDaUltima_RetornaVazioComMetadadosCorretos()
    {
        await using var contexto = await CriarContextoComItensAsync(25);

        var resultado = await Ordenada(contexto).ToPagedListAsync(page: 5, pageSize: 10);

        Assert.Empty(resultado.Items);
        Assert.Equal(5, resultado.Page);
        Assert.Equal(25, resultado.TotalCount);
        Assert.Equal(3, resultado.TotalPages);
    }

    [Fact]
    public async Task ToPagedListAsync_ComPaginaEnorme_NaoEstouraEDevolveVazio()
    {
        await using var contexto = await CriarContextoComItensAsync(5);

        var resultado = await Ordenada(contexto).ToPagedListAsync(page: int.MaxValue, pageSize: 100);

        Assert.Empty(resultado.Items);
        Assert.Equal(5, resultado.TotalCount);
    }

    [Fact]
    public async Task ToPagedListAsync_SemRegistros_RetornaVazioEZeroPaginas()
    {
        await using var contexto = await CriarContextoComItensAsync(0);

        var resultado = await Ordenada(contexto).ToPagedListAsync(page: 1, pageSize: 10);

        Assert.Empty(resultado.Items);
        Assert.Equal(0, resultado.TotalCount);
        Assert.Equal(0, resultado.TotalPages);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    public async Task ToPagedListAsync_ComParametrosInvalidos_LancaArgumentOutOfRangeException(int page, int pageSize)
    {
        await using var contexto = await CriarContextoComItensAsync(1);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => Ordenada(contexto).ToPagedListAsync(page, pageSize));
    }

    [Fact]
    public async Task ToPagedListAsync_ComConsultaNula_LancaArgumentNullException()
    {
        IOrderedQueryable<ItemTeste> consulta = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() => consulta.ToPagedListAsync(1, 10));
    }

    private static IOrderedQueryable<ItemTeste> Ordenada(ContextoTeste contexto) =>
        contexto.Itens.OrderBy(i => i.Ordem).ThenBy(i => i.Id);

    // Os itens entram embaralhados: sem o OrderBy da consulta, o InMemory devolveria na ordem
    // de inserção e o teste não provaria que a ordenação é aplicada antes da paginação.
    private static async Task<ContextoTeste> CriarContextoComItensAsync(int quantidade)
    {
        var opcoes = new DbContextOptionsBuilder<ContextoTeste>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var contexto = new ContextoTeste(opcoes);
        contexto.Itens.AddRange(Enumerable
            .Range(1, quantidade)
            .Select(n => new ItemTeste { Ordem = n })
            .OrderBy(_ => Guid.NewGuid()));
        await contexto.SaveChangesAsync();

        return contexto;
    }

    internal sealed class ItemTeste : Entity
    {
        public int Ordem { get; set; }
    }

    internal sealed class ContextoTeste(DbContextOptions<ContextoTeste> opcoes) : DbContext(opcoes)
    {
        public DbSet<ItemTeste> Itens => Set<ItemTeste>();
    }
}
