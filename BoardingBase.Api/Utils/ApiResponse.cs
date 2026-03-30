namespace BoardingBase.Api.Utils;

public record ApiResponse
(
    int status,
    string message,
    object? data = null
);