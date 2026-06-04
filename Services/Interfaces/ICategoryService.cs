using StreetSignalApi.DTOs.Responses;

namespace StreetSignalApi.Services.Interfaces;

public interface ICategoryService
{
    Task<CategoryListResponse> ListAsync(bool includeInactive, bool requesterIsStaff, CancellationToken ct = default);
}
