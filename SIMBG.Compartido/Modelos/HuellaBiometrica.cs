using System.ComponentModel.DataAnnotations;

namespace SIMBG.Compartido.Modelos
{
    public class HuellaBiometrica
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public string HashTemplate { get; set; } = string.Empty;

        [Required]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

    }
}
