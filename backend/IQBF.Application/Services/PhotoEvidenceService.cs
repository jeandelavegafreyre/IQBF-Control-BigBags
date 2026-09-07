using IQBF.Application.DTOs.Photos;
using IQBF.Application.Interfaces;
using IQBF.Domain.Entities;
using IQBF.Domain.Interfaces;
using IQBF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IQBF.Application.Services;

public class PhotoEvidenceService : IPhotoEvidenceService
{
    private const int MaxPhotosPerOperation = 3;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    private readonly IQBFDbContext _db;
    private readonly IPhotoStorageService _photoStorage;

    public PhotoEvidenceService(
        IQBFDbContext db,
        IPhotoStorageService photoStorage)
    {
        _db = db;
        _photoStorage = photoStorage;
    }

    public async Task<PhotoDto> AddReceptionPhotoAsync(
        Guid receptionId,
        Stream stream,
        string fileName,
        string contentType,
        long fileSize,
        CancellationToken cancellationToken = default)
    {
        ValidateFile(fileName, contentType, fileSize);

        var receptionExists = await _db.Receptions
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == receptionId,
                cancellationToken);

        if (!receptionExists)
        {
            throw new KeyNotFoundException(
                "Recepción no encontrada.");
        }

        var photoCount = await _db.ReceptionPhotos
            .CountAsync(
                x => x.ReceptionId == receptionId,
                cancellationToken);

        if (photoCount >= MaxPhotosPerOperation)
        {
            throw new InvalidOperationException(
                "La recepción ya tiene el máximo de 3 fotografías.");
        }

        var storedPhoto = await _photoStorage.SaveAsync(
            stream,
            fileName,
            contentType,
            "receptions",
            cancellationToken);

        try
        {
            var photo = new ReceptionPhoto
            {
                ReceptionId = receptionId,
                PhotoUrl = storedPhoto.PhotoUrl,
                FileName = storedPhoto.FileName,
                ContentType = storedPhoto.ContentType,
                FileSize = storedPhoto.FileSize
            };

            _db.ReceptionPhotos.Add(photo);

            await _db.SaveChangesAsync(cancellationToken);

            return ToDto(
                photo.Id,
                photo.PhotoUrl,
                photo.FileName ?? string.Empty,
                photo.ContentType ?? string.Empty,
                photo.FileSize);
        }
        catch
        {
            await _photoStorage.DeleteAsync(
                storedPhoto.PhotoUrl,
                cancellationToken);

            throw;
        }
    }

    public async Task<IReadOnlyCollection<PhotoDto>>
        GetReceptionPhotosAsync(
            Guid receptionId,
            CancellationToken cancellationToken = default)
    {
        var receptionExists = await _db.Receptions
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == receptionId,
                cancellationToken);

        if (!receptionExists)
        {
            throw new KeyNotFoundException(
                "Recepción no encontrada.");
        }

        return await _db.ReceptionPhotos
            .AsNoTracking()
            .Where(x => x.ReceptionId == receptionId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new PhotoDto(
                x.Id,
                x.PhotoUrl,
                x.FileName ?? string.Empty,
                x.ContentType ?? string.Empty,
                x.FileSize ?? 0L))
            .ToListAsync(cancellationToken);
    }

    public async Task<PhotoDto> AddDispatchPhotoAsync(
        Guid dispatchId,
        Stream stream,
        string fileName,
        string contentType,
        long fileSize,
        CancellationToken cancellationToken = default)
    {
        ValidateFile(fileName, contentType, fileSize);

        var dispatchExists = await _db.Dispatches
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == dispatchId,
                cancellationToken);

        if (!dispatchExists)
        {
            throw new KeyNotFoundException(
                "Despacho no encontrado.");
        }

        var photoCount = await _db.DispatchPhotos
            .CountAsync(
                x => x.DispatchId == dispatchId,
                cancellationToken);

        if (photoCount >= MaxPhotosPerOperation)
        {
            throw new InvalidOperationException(
                "El despacho ya tiene el máximo de 3 fotografías.");
        }

        var storedPhoto = await _photoStorage.SaveAsync(
            stream,
            fileName,
            contentType,
            "dispatches",
            cancellationToken);

        try
        {
            var photo = new DispatchPhoto
            {
                DispatchId = dispatchId,
                PhotoUrl = storedPhoto.PhotoUrl,
                FileName = storedPhoto.FileName,
                ContentType = storedPhoto.ContentType,
                FileSize = storedPhoto.FileSize
            };

            _db.DispatchPhotos.Add(photo);

            await _db.SaveChangesAsync(cancellationToken);

            return ToDto(
                photo.Id,
                photo.PhotoUrl,
                photo.FileName ?? string.Empty,
                photo.ContentType ?? string.Empty,
                photo.FileSize);
        }
        catch
        {
            await _photoStorage.DeleteAsync(
                storedPhoto.PhotoUrl,
                cancellationToken);

            throw;
        }
    }

    public async Task<IReadOnlyCollection<PhotoDto>>
        GetDispatchPhotosAsync(
            Guid dispatchId,
            CancellationToken cancellationToken = default)
    {
        var dispatchExists = await _db.Dispatches
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == dispatchId,
                cancellationToken);

        if (!dispatchExists)
        {
            throw new KeyNotFoundException(
                "Despacho no encontrado.");
        }

        return await _db.DispatchPhotos
            .AsNoTracking()
            .Where(x => x.DispatchId == dispatchId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new PhotoDto(
                x.Id,
                x.PhotoUrl,
                x.FileName ?? string.Empty,
                x.ContentType ?? string.Empty,
                x.FileSize ?? 0L))
            .ToListAsync(cancellationToken);
    }

    private static void ValidateFile(
        string fileName,
        string contentType,
        long fileSize)
    {
        if (fileSize <= 0)
        {
            throw new ArgumentException(
                "La fotografía está vacía.");
        }

        if (fileSize > MaxFileSize)
        {
            throw new ArgumentException(
                "La fotografía supera el tamaño máximo de 10 MB.");
        }

        var extension =
            Path.GetExtension(fileName).ToLowerInvariant();

        string[] allowedExtensions =
        [
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        ];

        if (!allowedExtensions.Contains(extension))
        {
            throw new ArgumentException(
                "Formato de fotografía no permitido.");
        }

        string[] allowedContentTypes =
        [
            "image/jpeg",
            "image/png",
            "image/webp"
        ];

        if (!allowedContentTypes.Contains(
                contentType,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Tipo de fotografía no permitido.");
        }
    }

    private static PhotoDto ToDto(
        Guid id,
        string photoUrl,
        string fileName,
        string contentType,
        long? fileSize)
    {
        return new PhotoDto(
            id,
            photoUrl,
            fileName,
            contentType,
            fileSize ?? 0L);
    }
}