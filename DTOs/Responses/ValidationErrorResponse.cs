namespace StreetSignalApi.DTOs.Responses;

public class ValidationErrorResponse
{
    public string Code { get; set; } = "VALIDATION_ERROR";
    public string Message { get; set; } = "One or more validation errors occurred.";
    public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
}
