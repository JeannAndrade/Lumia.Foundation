namespace LumiaFoundation.AspNetCore.Pagination;

internal static class PaginationWindow
{
    /// <summary>
    /// Calcula quais páginas exibir: primeira, última e as vizinhas da atual. Cada <c>null</c>
    /// no resultado representa um trecho omitido (reticências). Um buraco de uma única página
    /// mostra a própria página, pois as reticências ocupariam o mesmo espaço.
    /// </summary>
    internal static IReadOnlyList<int?> Build(int page, int totalPages, int siblingCount)
    {
        siblingCount = Math.Max(0, siblingCount);

        var pages = new SortedSet<int> { 1, totalPages };

        // long evita estouro de int quando page é muito grande (página inexistente na querystring).
        var inicio = (int)Math.Max(1L, (long)page - siblingCount);
        var fim = (int)Math.Min(totalPages, (long)page + siblingCount);
        for (var atual = inicio; atual <= fim; atual++)
            pages.Add(atual);

        var janela = new List<int?>();
        int? anterior = null;

        foreach (var atual in pages)
        {
            if (anterior is { } ultima)
            {
                if (atual - ultima == 2)
                    janela.Add(ultima + 1);
                else if (atual - ultima > 2)
                    janela.Add(null);
            }

            janela.Add(atual);
            anterior = atual;
        }

        return janela;
    }
}
