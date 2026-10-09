namespace LumiaFoundation.Core.Pagination;

/// <summary>Uma página de resultados com os metadados necessários para navegar entre páginas.</summary>
public sealed class PagedList<T>
{
    public PagedList(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public IReadOnlyList<T> Items { get; }

    /// <summary>Número da página, começando em 1.</summary>
    public int Page { get; }

    public int PageSize { get; }

    /// <summary>Total de itens em todas as páginas.</summary>
    public int TotalCount { get; }

    /// <summary>Total de páginas. É 0 quando não há nenhum item.</summary>
    public int TotalPages => TotalCount / PageSize + (TotalCount % PageSize > 0 ? 1 : 0);

    /// <summary>Converte os itens mantendo os metadados de paginação.</summary>
    public PagedList<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return new PagedList<TResult>([.. Items.Select(mapper)], Page, PageSize, TotalCount);
    }
}
