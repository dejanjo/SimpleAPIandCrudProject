using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVilla_API.Services;

namespace RoyalVilla_API.Controllers
{
	[Route("api/auth")]
	[ApiController]
	public class AuthController(IAuthService authService) : ControllerBase
	{
		private readonly IAuthService _authService = authService;

		[HttpPost("register")]
		[ProducesResponseType(typeof(ApiResponse<UserDTO>), statusCode: StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<UserDTO>>> Register([FromBody]RegistrationRequestDTO registrationRequestDTO)
		{
			try
			{
				if (registrationRequestDTO == null)
				{
					return BadRequest(ApiResponse<object>.BadRequest("Registration data is required"));
				}

				if (await _authService.IsEmailExistsAsync(registrationRequestDTO.Email))
				{
					return Conflict(ApiResponse<object>.Conflict($"A user with the name '{registrationRequestDTO.Name}' already exists"));
				}

				var user = await _authService.RegisterAsync(registrationRequestDTO);

				if (user == null)
				{
					return BadRequest(ApiResponse<object>.BadRequest("Registration failed"));
				}

				var response = ApiResponse<UserDTO>.CreatedAt(user, "User registered successfully");
				return CreatedAtAction(nameof(Register), response);
			}
			catch(Exception ex)
			{
				var errorResponse = ApiResponse<object>.Error(500, "An error occurred durign registration", ex.Message);
				return StatusCode(500, errorResponse);
			}
			
		}

		[HttpPost("login")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<LoginRequestDTO>>), statusCode: StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), statusCode: StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), statusCode: StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> Login([FromBody] LoginRequestDTO loginRequestDTO)
		{
			try
			{
				if (loginRequestDTO == null)
				{
					return BadRequest(ApiResponse<object>.BadRequest("Login data is required"));
				}

				var loginResponse = await _authService.LoginAsync(loginRequestDTO);

				if (loginResponse == null)
				{
					return BadRequest(ApiResponse<object>.BadRequest("Login failed"));
				}

				var response = ApiResponse<LoginResponseDTO>.Ok(loginResponse, "Login successfully");
				return Ok(response);
			}
			catch (Exception ex)
			{
				var errorResponse = ApiResponse<object>.Error(500, "An error occurred durign login", ex.Message);
				return StatusCode(500, errorResponse);
			}

		}
	}
}
