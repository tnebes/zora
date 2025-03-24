namespace zora.Core.Utils;

public static class PaginationUtils
{
    public static int CalculateTotalPages(int totalCount, int pageSize) =>
        (int)Math.Ceiling(totalCount / (double)pageSize);

    public static int AdjustPage(int page, int totalPages) => totalPages > 0 ? Math.Min(page, totalPages) : 1;

    public static int CalculateSkip(int page, int pageSize) => (page - 1) * pageSize;
}
