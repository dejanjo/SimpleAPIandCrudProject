using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IServices;

namespace RoyalVillaWeb.Services
{
	public class VillaService : BaseService, IVillaService
	{
		private const string APIEndPoint = "/api/villa";
		public VillaService(IHttpClientFactory httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) 
			: base(httpClient, httpContextAccessor)
		{
		}

		public Task<T?> CreateAsync<T>(VillaCreateDTO villaCreateDTO)
		{
			return SendAsync<T>(new ApiRequest
			{
				ApiType = SD.ApiType.POST,
				Data = villaCreateDTO,
				Url = APIEndPoint,
			});
		}

		public Task<T?> DeleteAsync<T>(int id)
		{
			return SendAsync<T>(new ApiRequest
			{
				ApiType = SD.ApiType.DELETE,
				Url = $"{APIEndPoint}/{id}",
			});
		}

		public Task<T?> GetAllAsync<T>()
		{
			return SendAsync<T>(new ApiRequest
			{
				ApiType = SD.ApiType.GET,
				Url = $"{APIEndPoint}",
			});

		}

		public Task<T?> GetAsync<T>(int id)
		{
			return SendAsync<T>(new ApiRequest
			{
				ApiType = SD.ApiType.GET,
				Url = $"{APIEndPoint}/{id}",
			});
		}

		public Task<T?> UpdateAsync<T>(VillaUpdateDTO villaUpdateDTO)
		{
			return SendAsync<T>(new ApiRequest
			{
				ApiType = SD.ApiType.PUT,
				Data = villaUpdateDTO,
				Url = $"{APIEndPoint}/{villaUpdateDTO.Id}",
			});
		}
	}
}
