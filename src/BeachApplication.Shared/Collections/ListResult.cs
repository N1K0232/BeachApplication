namespace BeachApplication.Shared.Collections;

public record class ListResult<T>(IEnumerable<T>? Content, long TotalCount, int TotalPages, bool HasNextPage = false) where T : class;