using System.ComponentModel.DataAnnotations;
using StreetSignalApi.Common.Enums;

namespace StreetSignalApi.DTOs.Requests;

public class RegisterDeviceTokenRequest
{
    [Required(ErrorMessage = "Token is required.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Token must be between 10 and 500 characters.")]
    public string Token { get; set; } = string.Empty;

    public Platform? Platform { get; set; }
}
