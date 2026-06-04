namespace StreetSignalApi.Common.Exceptions;

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string code, string message)
        : base(StatusCodes.Status401Unauthorized, code, message) { }
}
