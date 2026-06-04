namespace StreetSignalApi.Common.Exceptions;

public sealed class ConflictException : AppException
{
    public ConflictException(string code, string message)
        : base(StatusCodes.Status409Conflict, code, message) { }
}
