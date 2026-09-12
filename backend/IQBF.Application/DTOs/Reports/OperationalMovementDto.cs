namespace IQBF.Application.DTOs.Reports;

public sealed record OperationalMovementDto(
    Guid Id,
    string MovementType,
    int TransactionNumber,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy,
    string Reference,
    string? Comment,
    IReadOnlyList<OperationalMovementItemDto> Items,
    IReadOnlyList<OperationalMovementPhotoDto> Photos);

public sealed record OperationalMovementItemDto(
    Guid BLId,
    string BLCode,
    string ProductName,
    int Quantity);

public sealed record OperationalMovementPhotoDto(
    Guid Id,
    string PhotoUrl,
    string? FileName,
    string? ContentType,
    long? FileSize);
