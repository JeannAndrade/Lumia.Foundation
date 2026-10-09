using LumiaFoundation.Abstractions.Pagination;
using LumiaFoundation.Core.Pagination;

namespace LumiaFoundation.AspNetCore.Pagination;

public static class PagedListExtensions
{
    /// <summary>Converte um <see cref="PagedList{T}"/> no contrato de transporte <see cref="PagedResponse{T}"/>.</summary>
    public static PagedResponse<T> ToPagedResponse<T>(this PagedList<T> pagedList)
    {
        ArgumentNullException.ThrowIfNull(pagedList);

        return new PagedResponse<T>(
            pagedList.Items,
            pagedList.Page,
            pagedList.PageSize,
            pagedList.TotalCount,
            pagedList.TotalPages);
    }
}
