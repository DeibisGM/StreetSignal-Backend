namespace StreetSignalApi.DTOs.Responses;

public class ReportUpdateListResponse
{
    public IReadOnlyList<ReportUpdateDto> Data { get; set; } = Array.Empty<ReportUpdateDto>();
}
