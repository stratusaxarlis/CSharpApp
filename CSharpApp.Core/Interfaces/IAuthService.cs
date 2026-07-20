namespace CSharpApp.Core.Interfaces;

public interface IAuthService
{
    Task<JwtTokenResponse?> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
}
