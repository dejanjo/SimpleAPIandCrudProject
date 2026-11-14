using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla.DTO;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;

namespace RoyalVilla_API.Controllers.v1
{
	[Route("api/v{version:apiVersion}/villa-amenitie")]
	[ApiVersion("1.0")]
	[ApiController]
	public class VillaAmenitiesController : ControllerBase
	{
		public  readonly ApplicationDbContext _db;
		public  readonly IMapper _mapper;
		public VillaAmenitiesController(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}
		[HttpGet]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaAmenitiesDTO>>), StatusCodes.Status200OK)]

		public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> GetAmenities()
		{
			var amenities = await _db.VillaAmenities.ToListAsync();
			var dtoResponseAmenities = _mapper.Map<List<VillaAmenitiesDTO>>(amenities);
			var response = ApiResponse<IEnumerable<VillaAmenitiesDTO>>.Ok(dtoResponseAmenities, "Villa amenities retrived successfully");
			return Ok(response);	
		}

		[HttpGet("{id:int}")]
		//[AllowAnonymous]
		[ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), statusCode: StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> GetVillaById(int id)
		{
			try
			{
				if (id <= 0)
				{
					return NotFound(ApiResponse<object>.NotFound());
				}

				var villa = await _db.Villa.FirstOrDefaultAsync(u => u.Id == id);

				if (villa == null)
				{
					return NotFound(ApiResponse<object>.NotFound());
				}

				return Ok(ApiResponse<VillaAmenitiesDTO>.Ok(_mapper.Map<VillaAmenitiesDTO>(villa), "Records retrived successfully"));
			}
			catch (Exception ex)
			{
				var errorResponse = ApiResponse<object>.Error(500, $"An error occurred while creating villa: ", ex.Message);
				return StatusCode(500, errorResponse);
			}
		}

		[HttpPost]
		[ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), statusCode: StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> CreateVillaAmenities(VillaAmenitiesCreateDTO villaAmenitiesCreateDTO)
		{
			try
			{
				if (villaAmenitiesCreateDTO == null)
				{
					return BadRequest(ApiResponse<object>.BadRequest("Villa amenities data is required"));
				}

				var villaExists = await _db.Villa.FirstOrDefaultAsync(u => u.Id == villaAmenitiesCreateDTO.VillaId);

				if (villaExists == null)
				{
					return Conflict(ApiResponse<object>.Conflict($"Villa with the ID: '{villaAmenitiesCreateDTO.VillaId}'does not exists"));
				}

				VillaAmenities villaAmenities = _mapper.Map<VillaAmenities>(villaAmenitiesCreateDTO);
				villaAmenities.CreatedDate = DateTime.Now;

				await _db.VillaAmenities.AddAsync(villaAmenities);
				await _db.SaveChangesAsync();

				var respone = ApiResponse<VillaAmenitiesDTO>.CreatedAt(_mapper.Map<VillaAmenitiesDTO>(villaAmenities), "Villa amenities created successfully");
				return CreatedAtAction(nameof(CreateVillaAmenities), new { id = villaAmenities.Id }, respone);
			}
			catch (Exception ex)
			{
				var errorResponse = ApiResponse<object>.Error(500, $"An error occurred while creating villa amenities: ", ex.Message);
				return StatusCode(500, errorResponse);
			}
		}

		[HttpPut("{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), statusCode: StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> UpdateVillaAmenities(int id, VillaAmenitiesUpdateDTO villaAmenitiesUpdateDTO)
		{
			try
			{
				if (villaAmenitiesUpdateDTO == null)
				{
					return BadRequest(ApiResponse<object>.BadRequest("Villa amenities data is required"));
				}

				if (id != villaAmenitiesUpdateDTO.Id)
				{
					return BadRequest(ApiResponse<object>.BadRequest($"Villa amenities ID in URL does not match Villa ID in request body"));
				}


				var existingVillaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Id == id);
				if (existingVillaAmenities == null)
				{
					return NotFound(ApiResponse<object>.NotFound());
				}

				var villaExists = await _db.Villa.FirstOrDefaultAsync(u => u.Id == villaAmenitiesUpdateDTO.VillaId);

				if (villaExists == null)
				{
					return Conflict(ApiResponse<object>.Conflict($"Villa with the ID: '{villaAmenitiesUpdateDTO.VillaId}'does not exists"));
				}

				_mapper.Map(villaAmenitiesUpdateDTO, existingVillaAmenities);
				existingVillaAmenities.UpdatedDate = DateTime.Now;

				await _db.SaveChangesAsync();
				var response = ApiResponse<VillaAmenitiesDTO>.Ok(_mapper.Map<VillaAmenitiesDTO>(existingVillaAmenities), "Villa amenities updated successfully.");
				return Ok(response);
			}
			catch (Exception ex)
			{
				var errorResponse = ApiResponse<object>.Error(500, $"An error occurred while creating villa: ", ex.Message);
				return StatusCode(500, errorResponse);
			}
		}

		[HttpDelete("{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<object>>> DeleteVillaAmenities(int id)
		{
			try
			{
				var existingVillaAmenities = await _db.Villa.FirstOrDefaultAsync(u => u.Id == id);
				if (existingVillaAmenities == null)
				{
					return NotFound(ApiResponse<object>.NotFound());
				}

				_db.Villa.Remove(existingVillaAmenities);
				await _db.SaveChangesAsync();

				var response = ApiResponse<object>.NoContent("Villa amenities deleted successfully");
				return Ok(response);
			}
			catch (Exception ex)
			{
				var errorResponse = ApiResponse<object>.Error(500, $"An error occurred while creating villa amenities: ", ex.Message);
				return StatusCode(500, errorResponse);
			}
		}
	}
}
