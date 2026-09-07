using IQBF.Application.DTOs.Photos;
using IQBF.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IQBF.API.Controllers;

[ApiController]
[Route("api/dispatches/{dispatchId:guid}/photos")]
[Authorize]
public class DispatchPhotosController : ControllerBase
{
    private readonly IPhotoEvidenceService _photoService;

    public DispatchPhotosController(
        IPhotoEvidenceService photoService)
    {
        _photoService = photoService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<PhotoDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<PhotoDto>>>
        GetPhotos(
            Guid dispatchId,
            CancellationToken cancellationToken)
    {
        var photos =
            await _photoService.GetDispatchPhotosAsync(
                dispatchId,
                cancellationToken);

        return Ok(photos);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(
        typeof(PhotoDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PhotoDto>> UploadPhoto(
        Guid dispatchId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new
            {
                error = "Debe seleccionar una fotografía."
            });
        }

        await using var stream = file.OpenReadStream();

        var photo =
            await _photoService.AddDispatchPhotoAsync(
                dispatchId,
                stream,
                file.FileName,
                file.ContentType,
                file.Length,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            photo);
    }
}