using StreetSignalApi.DTOs.Responses;
using StreetSignalApi.Models;

namespace StreetSignalApi.Mappers;

public static class CategoryMapper
{
    public static CategoryDto ToDto(this Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        Icon = category.Icon,
        Color = category.Color,
        IsActive = category.IsActive,
        SortOrder = category.SortOrder,
        CreatedAt = category.CreatedAt
    };
}
