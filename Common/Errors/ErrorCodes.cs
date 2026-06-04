namespace StreetSignalApi.Common.Errors;

public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string NotFound = "NOT_FOUND";
    public const string EmailAlreadyExists = "EMAIL_ALREADY_EXISTS";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string ReportNotEditable = "REPORT_NOT_EDITABLE";
    public const string FileTooLarge = "FILE_TOO_LARGE";
    public const string InvalidFileType = "INVALID_FILE_TYPE";
    public const string InternalError = "INTERNAL_ERROR";
}
