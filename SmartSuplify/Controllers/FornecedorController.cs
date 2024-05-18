using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSuplify.Data;
using SmartSuplify.DTO;
using SmartSuplify.Models;

namespace SmartSuplify.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class FornecedorController : ControllerBase
	{
		public readonly ApplicationDbContext _dbContext;

		public FornecedorController(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}


		[HttpGet]
		public async Task<IActionResult> Get()
		{
			var fornecedores = await _dbContext.Fornecedores.ToListAsync();
			return Ok(fornecedores);
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<Fornecedor>> GetById(int id)
		{
			var fornecedor = await _dbContext.Fornecedores.FindAsync(id);
			if (fornecedor == null)
			{
				return NotFound();
			}
			return Ok(fornecedor);
		}


		[HttpPost]
		public async Task<IActionResult> Create([FromBody]FornecedorDTO fornecedorDTO)
		{
			if (ModelState.IsValid)
			{
				Fornecedor fornecedor = new Fornecedor();
				fornecedor.Nome = fornecedorDTO.Nome;
				fornecedor.Telefone = fornecedorDTO.Telefone;
				fornecedor.Email = fornecedorDTO.Email;

				_dbContext.Fornecedores.Add(fornecedor);
				await _dbContext.SaveChangesAsync();
				return CreatedAtAction(nameof(GetById), new { id = fornecedor.FornecedorId }, fornecedor);
			}
			return BadRequest();
		}

		[HttpPut("{id:int}")]
		public async Task<IActionResult> Edit(int id, FornecedorDTO fornecedorDTO)
		{
			if (id != fornecedorDTO.FornecedorId)
			{
				return BadRequest();
			}

			if (ModelState.IsValid)
			{
				var fornecedor = _dbContext.Fornecedores.Find(fornecedorDTO.FornecedorId);
				if (fornecedor == null)
				{
					return NotFound();
				}

				fornecedor.Nome = fornecedorDTO.Nome;
				fornecedor.Telefone = fornecedorDTO.Telefone;
				fornecedor.Email = fornecedorDTO.Email;

				_dbContext.Update(fornecedor);
				await _dbContext.SaveChangesAsync();
				return Ok(fornecedor);
			}
			return BadRequest();
		}

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id)
		{
			var fornecedor = await _dbContext.Fornecedores.FindAsync(id);
			if (fornecedor == null)
			{
				return NotFound();
			}
			_dbContext.Fornecedores.Remove(fornecedor);
			await _dbContext.SaveChangesAsync();
			return Ok(fornecedor);
		}


	}
}
