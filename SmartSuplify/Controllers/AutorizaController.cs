using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SmartSuplify.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartSuplify.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AutorizaController : ControllerBase
	{
		private readonly UserManager<IdentityUser> _userManager;

		private readonly SignInManager<IdentityUser> _signInManager;

		private readonly IConfiguration _configuration;

		public AutorizaController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_configuration = configuration;
		}

		[HttpPost("CadastrarUsuario")]
		public async Task<ActionResult> RegisterUser([FromBody] UsuarioDTO usuarioDTO)
		{
			var user = new IdentityUser
			{
				UserName = usuarioDTO.Email,
				Email = usuarioDTO.Email,
				EmailConfirmed = true
			};

			var result = await _userManager.CreateAsync(user, usuarioDTO.Password);

			if (!result.Succeeded)
			{
				return BadRequest(result.Errors);
			}
			await _signInManager.SignInAsync(user, false);
			return Ok(GeraToken(usuarioDTO));
		}

		[HttpPost("Login")]
		public async Task<ActionResult> Login([FromBody] UsuarioDTO usuarioDTO)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState.Values.SelectMany(e => e.Errors));
			}

			var resul = await _signInManager.PasswordSignInAsync(
								usuarioDTO.Email,
								usuarioDTO.Password,
								isPersistent: false,
								lockoutOnFailure: false
							);
			if (resul.Succeeded)
			{
				return Ok(GeraToken(usuarioDTO));
			}
			else
			{
				ModelState.AddModelError(string.Empty, "E-mail ou Senha inválidos");
				return BadRequest(ModelState);
			}
		}

		private UsuarioToken GeraToken(UsuarioDTO userInfo)
		{
			//Define declarações do usuário
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.Email),
				new Claim("SmartSuplify", "UsuarioSmartSuplify"),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			//gera uma chave com base em um algoritmo simetrico
			var key = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(_configuration["Jwt:key"]));

			//gera a assinatura digital do token usando o algoritmo Hmac e a chave privada
			var credenciais = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			//Tempo de expiracão do token.
			var expiracao = _configuration["TokenConfiguration:ExpireHours"];
			var expiration = DateTime.UtcNow.AddHours(double.Parse(expiracao));

			// classe que representa um token JWT e gera o token
			JwtSecurityToken token = new JwtSecurityToken(
			  issuer: _configuration["TokenConfiguration:Issuer"],
			  audience: _configuration["TokenConfiguration:Audience"],
			  claims: claims,
			  expires: expiration,
			  signingCredentials: credenciais);

			//retorna os dados com o token e informacoes
			return new UsuarioToken()
			{
				Authenticated = true,
				Token = new JwtSecurityTokenHandler().WriteToken(token),
				Expiration = expiration,
				Message = "Token JWT OK"
			};
		}
	}
}
