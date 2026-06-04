namespace StreetSignalApi.DTOs.Responses;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
