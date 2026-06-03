using System.ComponentModel.DataAnnotations;

namespace StreetSignalApi.DTOs.Requests;

public class UpdateReportRequest
{
    [StringLength(150, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 150 characters.")]
    public string? Title { get; set; }

    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters.")]
    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }

    [Url(ErrorMessage = "Image URL must be a valid URI.")]
    [StringLength(1000, ErrorMessage = "Image URL must not exceed 1000 characters.")]
    public string? ImageUrl { get; set; }
}
