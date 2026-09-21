namespace GymTron.Application.Base;

public abstract class PaginatedResultsQueryBase<T>
    : QueryBase<T>
{


    public int Page { get; }
    public int PageSize { get; }
    public string? SortBy { get; }
    public bool SortAscending { get; } = true;


    public PaginatedResultsQueryBase(Guid correlationId,
                                     int page,
                                     int pageSize)
        : base(correlationId)
    {
        Page = page;
        PageSize = pageSize;
    }


    public PaginatedResultsQueryBase(Guid correlationId,
                                     int page,
                                     int pageSize,
                                     string? sortBy,
                                     bool sortAscending)
        : base(correlationId)
    {
        Page = page;
        PageSize = pageSize;
        SortBy = sortBy;
        SortAscending = sortAscending;
    }
}
