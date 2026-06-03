namespace StreetSignalApi.DTOs.Responses;

public class PaginatedReportListResponse
{
    public IReadOnlyList<ReportSummaryDto> Data { get; set; } = Array.Empty<ReportSummaryDto>();
    public PaginationMeta Pagination { get; set; } = new();
}
