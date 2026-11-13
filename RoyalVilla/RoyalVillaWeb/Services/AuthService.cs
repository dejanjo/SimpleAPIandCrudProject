using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IServices;

namespace RoyalVillaWeb.Services
{
	public class AuthService : BaseService, IAuthService
	{
		private const string APIEndPoint = "/api/auth";
		public AuthService(IHttpClientFactory httpClient, IConfiguration configuration , IHttpContextAccessor httpContextAccessor)
		: base(httpClient, httpContextAccessor)
		{
		}

		public Task<T?> LoginAsync<T>(LoginRequestDTO loginRequestDTO)
		{
			return SendAsync<T>(new ApiRequest
			{
				ApiType = SD.ApiType.POST,
				Data = loginRequestDTO,
				Url = APIEndPoint + "/login",
			});
		}

		public Task<T?> RegisterAsync<T>(RegistrationRequestDTO registrationRequestDTO)
		{
			return SendAsync<T>(new ApiRequest
			{
				ApiType = SD.ApiType.POST,
				Data = registrationRequestDTO,
				Url = APIEndPoint + "/register",
			});
		}
	}
}
