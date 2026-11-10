using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;
using System.Collections;

namespace RoyalVilla_API.Controllers
{
	[Route("api/villa")]
	[ApiController]
	//[Authorize(Roles = "Customer, Admin")]
	public class VillaController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		public VillaController(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		[HttpGet]
		//[Authorize(Roles ="Admin")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTO>>), statusCode:StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), statusCode:StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<IEnumerable<VillaDTO>>>> GetVillas()
		{
			var villas = await _db.Villa.ToListAsync();
			var dtoResponseVilla = _mapper.Map<List<VillaDTO>>(villas);
			var response = ApiResponse<IEnumerable<VillaDTO>>.Ok(dtoResponseVilla, "Villas retrived successfully");
			return Ok(response);
		}

		[HttpGet("{id:int}")]
		//[AllowAnonymous]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), statusCode: StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<VillaDTO>>> GetVillaById(int id)
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

				return Ok(ApiResponse<VillaDTO>.Ok(_mapper.Map<VillaDTO>(villa), "Records retrived successfully"));
			}
			catch(Exception ex)
			{
				var errorResponse = ApiResponse<object>.Error(500, $"An error occurred while creating villa: ", ex.Message);
				return StatusCode(500, errorResponse);
			}
		}

		[HttpPost]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), statusCode: StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<VillaDTO>>> CreateVilla(VillaCreateDTO villaCreateDTO)
		{
			try
			{
				if (villaCreateDTO == null)
				{
					return BadRequest(ApiResponse<object>.BadRequest("Villa data is required"));
				}

				var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Name.ToLower() == villaCreateDTO.Name.ToLower());

				if (duplicateVilla != null)
				{
					return Conflict(ApiResponse<object>.Conflict($"A villa with the name '{villaCreateDTO.Name}' already exists"));
				}

				Villa villa = _mapper.Map<Villa>(villaCreateDTO);

				await _db.Villa.AddAsync(villa);
				await _db.SaveChangesAsync();

				var respone = ApiResponse<VillaDTO>.CreatedAt(_mapper.Map<VillaDTO>(villa), "Villa created successfully");
				return CreatedAtAction(nameof(CreateVilla), new {id = villa.Id}, respone);
			}
			catch (Exception ex)
			{
				var errorResponse = ApiResponse<object>.Error(500, $"An error occurred while creating villa: " ,ex.Message);
				return StatusCode(500, errorResponse);
			}
		}

		[HttpPut("{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<VillaDTO>), statusCode: StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(ApiResponse<object>), statusCode: StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<ApiResponse<VillaDTO>>> UpdateVilla(int id, VillaUpdateDTO villaUpdateDTO)
		{
			try
			{
				if (villaUpdateDTO == null)
				{
					return BadRequest(ApiResponse<object>.BadRequest("Villa data is required"));
				}

				if (id != villaUpdateDTO.Id)
				{
					return BadRequest(ApiResponse<object>.BadRequest($"Villa ID in URL does not match Villa ID in request body"));
				}


				var existingVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Id == id);
				if (existingVilla == null)
				{
					return NotFound(ApiResponse<object>.NotFound());
				}

				var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Name.ToLower() == villaUpdateDTO.Name.ToLower()
				&& u.Id != id);

				if (duplicateVilla != null)
				{
					return Conflict(ApiResponse<object>.Conflict($"A villa with the name '{villaUpdateDTO.Name}' already exists"));
				}

				_mapper.Map(villaUpdateDTO, existingVilla);
				existingVilla.UpdatedDate = DateTime.Now;

				await _db.SaveChangesAsync();
				var response = ApiResponse<VillaDTO>.Ok(_mapper.Map<VillaDTO>(villaUpdateDTO), "Villa updated successfully.");
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
		public async Task<ActionResult<ApiResponse<object>>> DeleteVilla(int id)
		{
			try
			{
				var existingVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Id == id);
				if (existingVilla == null)
				{
					return NotFound(ApiResponse<object>.NotFound());
				}

				_db.Villa.Remove(existingVilla);
				await _db.SaveChangesAsync();

				var response = ApiResponse<object>.NoContent("Villa deleted successfully");
				return Ok(response);
			}
			catch (Exception ex)
			{
				var errorResponse = ApiResponse<object>.Error(500, $"An error occurred while creating villa: ", ex.Message);
				return StatusCode(500, errorResponse);
			}
		}
	}
}
