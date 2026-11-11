using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IServices;

namespace RoyalVillaWeb.Services
{
	public class VillaService : BaseService, IVillaService
	{
		private const string APIEndPoint = "/api/villa";
		public VillaService(IHttpClientFactory httpClient, IConfiguration configuration) : base(httpClient)
		{
		}

		public Task<T?> CreateAsync<T>(VillaCreateDTO villaCreateDTO, string token)
		{
			return SendAsync<T>(new ApiRequest
			{
				ApiType = SD.ApiType.POST,
				Data = villaCreateDTO,
				Url = APIEndPoint,
				Token = token
			});
		}

		public Task<T?> DeleteAsync<T>(int id, string token)
		{
			return SendAsync<T>(new ApiRequest
			{
				ApiType = SD.ApiType.DELETE,
				Url = $"{APIEndPoint}/{id}",
				Token = token
			});
		}

		public Task<T?> GetAllAsync<T>(string token)
		{
			return SendAsync<T>(new ApiRequest
			{
				ApiType = SD.ApiType.GET,
				Url = $"{APIEndPoint}",
				Token = token
			});

		}

		public Task<T?> GetAsync<T>(int id, string token)
		{
			return SendAsync<T>(new ApiRequest
			{
				ApiType = SD.ApiType.GET,
				Url = $"{APIEndPoint}/{id}",
				Token = token
			});
		}

		public Task<T?> UpdateAsync<T>(VillaUpdateDTO villaUpdateDTO, string token)
		{
			return SendAsync<T>(new ApiRequest
			{
				ApiType = SD.ApiType.GET,
				Data = villaUpdateDTO,
				Url = $"{APIEndPoint}/{villaUpdateDTO.Id}",
				Token = token
			});
		}
	}
}
