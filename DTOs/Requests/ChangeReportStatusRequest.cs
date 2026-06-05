using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using StreetSignalApi.Common.Enums;

namespace StreetSignalApi.DTOs.Requests;

public class ChangeReportStatusRequest
{
    [Required(ErrorMessage = "Status is required.")]
    [EnumDataType(typeof(ReportStatus), ErrorMessage = "Status is not a valid value.")]
    [JsonPropertyName("newStatus")]
    public ReportStatus Status { get; set; }

    [StringLength(1000, ErrorMessage = "Message must not exceed 1000 characters.")]
    public string? Message { get; set; }
}
