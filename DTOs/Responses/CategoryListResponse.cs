namespace StreetSignalApi.DTOs.Responses;

public class CategoryListResponse
{
    public IReadOnlyList<CategoryDto> Data { get; set; } = Array.Empty<CategoryDto>();
}
