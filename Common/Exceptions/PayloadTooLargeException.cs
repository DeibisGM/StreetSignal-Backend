using StreetSignalApi.Common.Errors;

namespace StreetSignalApi.Common.Exceptions;

public sealed class PayloadTooLargeException : AppException
{
    public PayloadTooLargeException(string message = "The uploaded file exceeds the maximum allowed size.")
        : base(StatusCodes.Status413PayloadTooLarge, ErrorCodes.FileTooLarge, message) { }
}
