using System.ComponentModel.DataAnnotations;

namespace StreetSignalApi.DTOs.Requests;

public class CreateReportUpdateRequest
{
    [Required(ErrorMessage = "Message is required.")]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 1000 characters.")]
    public string Message { get; set; } = string.Empty;
}
