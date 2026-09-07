namespace IQBF.Application.DTOs.Photos;

public record PhotoDto(
    Guid Id,
    string PhotoUrl,
    string FileName,
    string ContentType,
    long FileSize
);