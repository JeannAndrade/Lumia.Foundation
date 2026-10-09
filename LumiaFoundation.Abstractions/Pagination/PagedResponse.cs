namespace LumiaFoundation.Abstractions.Pagination;

/// <summary>Contrato de transporte para respostas paginadas entre APIs e clientes HTTP.</summary>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
