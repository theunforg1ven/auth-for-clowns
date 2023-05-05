namespace StudyAuthApp.WebApi.Interfaces
{
    public interface ITokenService
    {
        string CreateAccessToken(int id);

        string CreateRefreshToken(int id);

        int DecodeToken(string token, out bool hasTokenExpired);
    }
}
