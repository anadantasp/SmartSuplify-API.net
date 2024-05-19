using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SmartSuplify.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
		public async Task<ActionResult> RegisterUser([FromBody] UsuarioDTO usuarioDTO, HttpContext context)
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

			var token = GeraToken(usuarioDTO, context);

			return Ok(token);
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
				var token = GeraToken(usuarioDTO, HttpContext);

				

				return Ok(token);
			}
			else
			{
				ModelState.AddModelError(string.Empty, "E-mail ou Senha inválidos");
				return BadRequest(ModelState);
			}
		}

		private UsuarioToken GeraToken(UsuarioDTO userInfo, HttpContext context)
		{
			var sessionId = GenerateSecureSessionId();
			//Define declarações do usuário
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.Email),
				new Claim("SmartSuplify", "UsuarioSmartSuplify"),
				new Claim(JwtRegisteredClaimNames.Jti, sessionId)
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

			var thirdClaim = claims[2];
			var claimValue = thirdClaim.Value;

			// Adiciona o ID da sessão no cabeçalho da resposta
			Response.Headers.Add("JSESSIONID", sessionId);

			// Adiciona o ID da sessão no cookie
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Secure = context.Request.IsHttps,
				SameSite = SameSiteMode.Lax
			};
			context.Response.Cookies.Append("JSESSIONID", sessionId, cookieOptions);


			//retorna os dados com o token e informacoes
			return new UsuarioToken()
			{
				Authenticated = true,
				Token = new JwtSecurityTokenHandler().WriteToken(token),
				Expiration = expiration,
				Message = "Token JWT OK"

			};
		}

		// Método para gerar um ID de sessão seguro
		private string GenerateSecureSessionId()
		{
			// Gera um array de bytes aleatórios
			byte[] randomBytes = new byte[16]; // 128 bits
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(randomBytes);
			}

			// Converte os bytes para uma string hexadecimal
			var sessionId = new StringBuilder();
			foreach (var b in randomBytes)
			{
				sessionId.Append(b.ToString("x2"));
			}

			return sessionId.ToString();
		}
	}
}
