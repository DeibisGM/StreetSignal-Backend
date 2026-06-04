using StreetSignalApi.DTOs.Responses;

namespace StreetSignalApi.Services.Interfaces;

public interface IFileService
{
    Task<FileUploadResponse> UploadImageAsync(IFormFile file, CancellationToken ct = default);
}
