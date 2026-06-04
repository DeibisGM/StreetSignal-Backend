using StreetSignalApi.Common.Errors;
using StreetSignalApi.Common.Exceptions;
using StreetSignalApi.DTOs.Responses;
using StreetSignalApi.Services.Interfaces;

namespace StreetSignalApi.Services.Implementations;

public sealed class FileService : IFileService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp"
    };
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };
    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _http;

    public FileService(IWebHostEnvironment env, IHttpContextAccessor http)
    {
        _env = env;
        _http = http;
    }

    public async Task<FileUploadResponse> UploadImageAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            throw new BadRequestException(ErrorCodes.ValidationError, "File is required.");

        if (file.Length > MaxBytes)
            throw new PayloadTooLargeException();

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedContentTypes.Contains(file.ContentType) || !AllowedExtensions.Contains(ext))
            throw new BadRequestException(ErrorCodes.InvalidFileType,
                "Only jpg, jpeg, png, and webp files are allowed.");

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var uploadsDir = Path.Combine(webRoot, "uploads", "reports");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var fullPath = Path.Combine(uploadsDir, fileName);

        await using (var stream = File.Create(fullPath))
            await file.CopyToAsync(stream, ct);

        var request = _http.HttpContext?.Request;
        var baseUrl = request is null ? "" : $"{request.Scheme}://{request.Host}";
        var fileUrl = $"{baseUrl}/uploads/reports/{fileName}";

        return new FileUploadResponse
        {
            FileUrl = fileUrl,
            FileName = fileName,
            ContentType = file.ContentType,
            SizeBytes = file.Length
        };
    }
}
