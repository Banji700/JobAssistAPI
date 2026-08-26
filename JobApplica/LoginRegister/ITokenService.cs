namespace JobApplica.LoginRegister
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(AppUser user);
    }
}
