namespace MiniShop.Dtos;

public class PagedResult<T>
{
    public required List<T> Items { get; init; }
    public required int Total { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
}