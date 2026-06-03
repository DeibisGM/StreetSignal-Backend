namespace StreetSignalApi.Common.Exceptions;

public sealed class BadRequestException : AppException
{
    public BadRequestException(string code, string message)
        : base(StatusCodes.Status400BadRequest, code, message) { }
}
