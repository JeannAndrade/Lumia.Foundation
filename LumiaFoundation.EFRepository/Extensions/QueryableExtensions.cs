using LumiaFoundation.Core.Pagination;
using Microsoft.EntityFrameworkCore;

namespace LumiaFoundation.EFRepository.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Materializa uma página da consulta. Exige <see cref="IOrderedQueryable{T}"/> porque
    /// paginar sem ordenação determinística pode repetir ou omitir itens entre páginas.
    /// </summary>
    /// <param name="page">Número da página, começando em 1.</param>
    /// <remarks>
    /// Executa duas consultas (total e página). Uma página além da última devolve lista vazia
    /// com os metadados corretos, sem executar a segunda consulta.
    /// </remarks>
    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IOrderedQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var totalCount = await query.CountAsync(cancellationToken);

        // long evita estouro de int quando page é muito grande.
        var itensAPular = (long)(page - 1) * pageSize;
        if (itensAPular >= totalCount)
            return new PagedList<T>([], page, pageSize, totalCount);

        var itens = await query
            .Skip((int)itensAPular)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<T>(itens, page, pageSize, totalCount);
    }
}
