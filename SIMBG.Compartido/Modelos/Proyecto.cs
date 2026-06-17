using System.ComponentModel.DataAnnotations;

namespace SIMBG.Compartido.Modelos
{
    public class Proyecto
    {
        [Key]
        public int Id { get; set; }

        // Esta es la llave foránea que lo conecta con la tabla Usuarios
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public string NombreProyecto { get; set; } = string.Empty;

        [Required]
        public string Asesor { get; set; } = string.Empty;

        [Required]
        public int HorasMeta { get; set; }

        // Puede ser nulo porque quizá el alumno aún no tiene su repositorio Git
        public string? UrlGit { get; set; }

        public DateTime? UltimoCommit { get; set; }
    }
}
