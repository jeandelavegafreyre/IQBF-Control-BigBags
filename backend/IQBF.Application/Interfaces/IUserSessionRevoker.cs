namespace IQBF.Application.Interfaces;

public interface IUserSessionRevoker
{
    void Revoke(Guid userId);
}
