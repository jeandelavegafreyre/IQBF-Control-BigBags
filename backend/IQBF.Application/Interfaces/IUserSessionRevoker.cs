namespace IQBF.Application.Interfaces;

public interface IUserSessionRevoker
{
    Task RevokeAsync(Guid userId, CancellationToken cancellationToken = default);
}
