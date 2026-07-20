namespace CSharpApp.Core.Interfaces;

public interface IAuthService
{
    Task<JwtTokenResponse?> LoginAsync(ThirdPartyAuthDto dto);
}
