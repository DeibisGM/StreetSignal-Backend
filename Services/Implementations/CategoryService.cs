using StreetSignalApi.DTOs.Responses;
using StreetSignalApi.Mappers;
using StreetSignalApi.Repositories.Interfaces;
using StreetSignalApi.Services.Interfaces;

namespace StreetSignalApi.Services.Implementations;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;
    public CategoryService(ICategoryRepository categories) => _categories = categories;

    public async Task<CategoryListResponse> ListAsync(bool includeInactive, bool requesterIsStaff, CancellationToken ct = default)
    {
        // Only staff may request inactive categories. Non-staff requests are silently
        // forced to active-only — per the contract description.
        var effectiveIncludeInactive = includeInactive && requesterIsStaff;
        var items = await _categories.ListAsync(effectiveIncludeInactive, ct);
        return new CategoryListResponse
        {
            Data = items.Select(c => c.ToDto()).ToList()
        };
    }
}
