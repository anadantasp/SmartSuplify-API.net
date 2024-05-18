using System.ComponentModel.DataAnnotations;

namespace SmartSuplify.DTO
{
	public class FornecedorDTO
	{
		public int FornecedorId { get; set; }
		[Required]
		public string? Nome { get; set; }
		[Required]
		public string? Telefone { get; set; }
		[EmailAddress(ErrorMessage = "Favor digitar um e-mail válido.")]
		public string? Email { get; set; }
	}
}
