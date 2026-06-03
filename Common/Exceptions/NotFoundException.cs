using StreetSignalApi.Common.Errors;

namespace StreetSignalApi.Common.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message = "The requested resource was not found.")
        : base(StatusCodes.Status404NotFound, ErrorCodes.NotFound, message) { }
}
