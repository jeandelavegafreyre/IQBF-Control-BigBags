using IQBF.Domain.Interfaces;

namespace IQBF.Infrastructure.Storage;

public class LocalPhotoStorageService : IPhotoStorageService
{
    private readonly string _rootPath;

    public LocalPhotoStorageService()
    {
        _rootPath = Path.Combine(
            AppContext.BaseDirectory,
            "storage",
            "photos");
    }

    public async Task<StoredPhotoResult> SaveAsync(
        Stream stream,
        string fileName,
        string contentType,
        string category,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        string[] allowedExtensions =
        [
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        ];

        string[] allowedContentTypes =
        [
            "image/jpeg",
            "image/png",
            "image/webp"
        ];

        if (!allowedExtensions.Contains(extension))
        {
            throw new ArgumentException(
                "Formato de imagen no permitido.");
        }

        if (!allowedContentTypes.Contains(
                contentType,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Tipo de archivo no permitido.");
        }

        var categoryFolder = category.ToLowerInvariant() switch
        {
            "receptions" => "receptions",
            "dispatches" => "dispatches",

            _ => throw new ArgumentException(
                "Categoría de fotografía no válida.")
        };

        var now = DateTime.UtcNow;

        var folder = Path.Combine(
            _rootPath,
            categoryFolder,
            now.ToString("yyyy"),
            now.ToString("MM"));

        Directory.CreateDirectory(folder);

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var fullPath = Path.Combine(
            folder,
            storedFileName);

        await using (var fileStream = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            useAsync: true))
        {
            await stream.CopyToAsync(
                fileStream,
                cancellationToken);
        }

        var fileInfo = new FileInfo(fullPath);

        var photoUrl =
            $"/photos/{categoryFolder}/" +
            $"{now:yyyy/MM}/" +
            storedFileName;

        return new StoredPhotoResult(
            photoUrl,
            storedFileName,
            contentType,
            fileInfo.Length);
    }

    public Task DeleteAsync(
        string photoUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(photoUrl))
        {
            return Task.CompletedTask;
        }

        var relativePath = photoUrl
            .TrimStart('/')
            .Replace('/', Path.DirectorySeparatorChar);

        var photosPrefix =
            "photos" + Path.DirectorySeparatorChar;

        if (relativePath.StartsWith(
                photosPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            relativePath =
                relativePath[photosPrefix.Length..];
        }

        var rootPath =
            Path.GetFullPath(_rootPath);

        var fullPath =
            Path.GetFullPath(
                Path.Combine(rootPath, relativePath));

        if (!fullPath.StartsWith(
                rootPath + Path.DirectorySeparatorChar,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Ruta de fotografía no válida.");
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}