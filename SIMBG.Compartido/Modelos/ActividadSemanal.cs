using System.ComponentModel.DataAnnotations;

namespace SIMBG.Compartido.Modelos
{
    public class ActividadSemanal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int NumSemana { get; set; } = 0;

        [Required]
        public string Descripcion { get; set; } = string.Empty;

        public bool Bloqueado { get; set; } = false;

    }
}
