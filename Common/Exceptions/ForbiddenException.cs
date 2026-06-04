using StreetSignalApi.Common.Errors;

namespace StreetSignalApi.Common.Exceptions;

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You do not have permission to access this resource.")
        : base(StatusCodes.Status403Forbidden, ErrorCodes.Forbidden, message) { }
}
