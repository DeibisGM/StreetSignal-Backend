namespace StreetSignalApi.DTOs.Responses;

public class ReportDetailDto : ReportSummaryDto
{
    public IReadOnlyList<ReportUpdateDto> Updates { get; set; } = Array.Empty<ReportUpdateDto>();
}
