using IQBF.Application.DTOs.Photos;

namespace IQBF.Application.Interfaces;

public interface IPhotoEvidenceService
{
    Task<PhotoDto> AddReceptionPhotoAsync(
        Guid receptionId,
        Stream stream,
        string fileName,
        string contentType,
        long fileSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PhotoDto>> GetReceptionPhotosAsync(
        Guid receptionId,
        CancellationToken cancellationToken = default);

    Task<PhotoDto> AddDispatchPhotoAsync(
        Guid dispatchId,
        Stream stream,
        string fileName,
        string contentType,
        long fileSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PhotoDto>> GetDispatchPhotosAsync(
        Guid dispatchId,
        CancellationToken cancellationToken = default);
}