namespace IQBF.Domain.Interfaces;

public interface IPhotoStorageService
{
    Task<StoredPhotoResult> SaveAsync(
        Stream stream,
        string fileName,
        string contentType,
        string category,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string photoUrl,
        CancellationToken cancellationToken = default);
}

public record StoredPhotoResult(
    string PhotoUrl,
    string FileName,
    string ContentType,
    long FileSize);
